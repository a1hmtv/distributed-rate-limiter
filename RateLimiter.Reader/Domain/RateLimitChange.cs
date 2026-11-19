namespace RateLimiter.Reader.Domain;

public record class RateLimitChange(string Route, RateLimit? Limit);