namespace RateLimiter.Writer.Application.Services.Models;

public sealed record CreateLimit
{
    public string Route { get; init; } = string.Empty;

    public int RequestsPerMinute { get; init; }
}