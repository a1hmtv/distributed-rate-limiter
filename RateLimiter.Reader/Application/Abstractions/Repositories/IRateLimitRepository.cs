using RateLimiter.Reader.Domain;

namespace RateLimiter.Reader.Application.Abstractions.Repositories;

public interface IRateLimitRepository
{
    IAsyncEnumerable<RateLimit> GetAllAsync(
        CancellationToken cancellationToken);

    IAsyncEnumerable<RateLimitChange> WatchChangesAsync(
        CancellationToken cancellationToken);
}