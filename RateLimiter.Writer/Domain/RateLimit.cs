namespace RateLimiter.Writer.Domain;

public class RateLimit
{
    public string Route { get; init; } = string.Empty;
    public int RequestsPerMinute { get; init; }
}