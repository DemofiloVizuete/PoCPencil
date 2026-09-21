namespace PromptMarket.Desktop.Api;

public sealed class Prompt
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Category { get; set; }
    public required string Description { get; set; }
    public required string Creator { get; set; }
    public decimal Price { get; set; }
    public int Sales { get; set; }
}