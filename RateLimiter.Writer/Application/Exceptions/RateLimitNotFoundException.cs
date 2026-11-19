namespace RateLimiter.Writer.Application.Exceptions;

public sealed class RateLimitNotFoundException : Exception
{
    public RateLimitNotFoundException(string route): base($"Rate limit for route '{route}' was not found.")
    {
    }
}