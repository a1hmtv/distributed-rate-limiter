using RateLimiter.Writer.Application.Services.Models;
using RateLimiter.Writer.Domain;

namespace RateLimiter.Writer.Application.Abstractions.Services;

public interface IRateLimitService
{
    Task<string> CreateLimitAsync(CreateLimit createLimit, CancellationToken cancellationToken);

    Task<int?> GetLimitByRouteAsync(string route, CancellationToken cancellationToken);

    Task<RateLimit> UpdateLimitAsync(UpdateLimit updateLimit, CancellationToken cancellationToken);

    Task<bool> DeleteLimitAsync(string route, CancellationToken cancellationToken);
}