using RateLimiter.Reader.Domain;

namespace RateLimiter.Reader.Application.Abstractions.Services;

public interface IRateLimitService
{
    Task InitializeAsync(CancellationToken cancellationToken);
    IEnumerable<RateLimit> GetAllLimits();
}