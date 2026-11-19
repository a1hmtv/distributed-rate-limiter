using RateLimiter.Writer.Domain;

namespace RateLimiter.Writer.Application.Abstractions.Repositories;

public interface IRateLimitRepository
{
    Task<string> CreateLimitAsync(string route, int limit, CancellationToken cancellationToken);
    Task<int?> GetLimitByRouteAsync(string route, CancellationToken cancellationToken);
    Task<RateLimit?> UpdateLimitAsync(string route, int limit, CancellationToken cancellationToken);
    Task<bool> DeleteLimitAsync(string route, CancellationToken cancellationToken);

}