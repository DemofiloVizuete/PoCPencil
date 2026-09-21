using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PromptMarket.Desktop.Api;

public sealed class LocalApiHost : IAsyncDisposable
{
    private WebApplication? app;

    public Uri BaseAddress { get; private set; } = null!;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(LocalApiHost).Assembly.GetName().Name,
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
        builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.PropertyNamingPolicy = null);
        var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PromptMarket");
        Directory.CreateDirectory(dataDirectory);
        var databasePath = Path.Combine(dataDirectory, "prompt-market.db");
        builder.Services.AddDbContextFactory<PromptDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));

        app = builder.Build();

        MapEmbeddedFile("/", "PromptMarket.Desktop.Web.index.html", "text/html; charset=utf-8");
        MapEmbeddedFile("/index.html", "PromptMarket.Desktop.Web.index.html", "text/html; charset=utf-8");
        MapEmbeddedFile("/styles.css", "PromptMarket.Desktop.Web.styles.css", "text/css; charset=utf-8");
        MapEmbeddedFile("/app.js", "PromptMarket.Desktop.Web.app.js", "text/javascript; charset=utf-8");

        app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
        app.MapGet("/api/prompts", async (IDbContextFactory<PromptDbContext> factory, CancellationToken token) =>
        {
            await using var db = await factory.CreateDbContextAsync(token);
            return Results.Ok(await db.Prompts.AsNoTracking().OrderByDescending(prompt => prompt.Sales).ToListAsync(token));
        });

        await app.StartAsync(cancellationToken);
        await SeedAsync(cancellationToken);
        BaseAddress = new Uri(app.Urls.First().TrimEnd('/') + "/");
    }

    private void MapEmbeddedFile(string route, string resourceName, string contentType)
    {
        app!.MapGet(route, async context =>
        {
            await using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (resource is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.ContentType = contentType;
            await resource.CopyToAsync(context.Response.Body, context.RequestAborted);
        });
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        await using var scope = app!.Services.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PromptDbContext>>();
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
        if (await context.Prompts.AnyAsync(cancellationToken)) return;

        context.Prompts.AddRange(
            new Prompt { Title = "The Brand Voice Architect", Category = "Writing", Description = "Build a distinct voice guide from five examples and a feeling.", Creator = "Maya Chen", Price = 12, Sales = 218 },
            new Prompt { Title = "The Fast Research Partner", Category = "Research", Description = "Turn a fuzzy question into a source-backed research plan.", Creator = "Studio North", Price = 18, Sales = 52 },
            new Prompt { Title = "Your Weekly Chief of Staff", Category = "Productivity", Description = "A calm system for turning a week into momentum.", Creator = "Alex Rivera", Price = 9, Sales = 34 });
        await context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (app is not null) await app.StopAsync();
        if (app is not null) await app.DisposeAsync();
    }
}