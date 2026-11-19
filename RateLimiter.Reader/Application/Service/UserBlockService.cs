using RateLimiter.Reader.Application.Abstractions.Repositories;

namespace RateLimiter.Reader.Application.Service;

public class UserBlockService(IUserBlockRepository repository)
{
    private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(5);

    public Task<bool> IsBlockedAsync(int userId, string endpoint)
    {
        return repository.IsBlockedAsync(userId, endpoint);
    }

    public Task<bool> TryBlockAsync(int userId, string endpoint)
    {
        return repository.TryBlockAsync(userId, BlockDuration, endpoint);
    }
}