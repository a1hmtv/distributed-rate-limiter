namespace RateLimiter.Writer.Application.Services.Models;

public sealed record UpdateLimit
{
    public string Route { get; init; } = string.Empty;

    public int RequestsPerMinute { get; init; }
}