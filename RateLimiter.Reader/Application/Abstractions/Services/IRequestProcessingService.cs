using RateLimiter.Reader.Domain;

namespace RateLimiter.Reader.Application.Abstractions.Services;

public interface IRequestProcessingService
{
    Task ProcessAsync(
        UserRequestEvent request,
        CancellationToken cancellationToken);
}