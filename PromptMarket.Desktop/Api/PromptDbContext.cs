using Microsoft.EntityFrameworkCore;

namespace PromptMarket.Desktop.Api;

public sealed class PromptDbContext(DbContextOptions<PromptDbContext> options) : DbContext(options)
{
    public DbSet<Prompt> Prompts => Set<Prompt>();
}