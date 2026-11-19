using RateLimiter.Reader.Application.Abstractions.Repositories;
using StackExchange.Redis;

namespace RateLimiter.Reader.Infrastructure.Repositories;

public class UserBlockRepository(IDatabase database) : IUserBlockRepository
{
    
    public Task<bool> IsBlockedAsync(int userId, string endpoint)
    {
        return database.KeyExistsAsync($"{userId}:{endpoint}");
    }

    public Task<bool> TryBlockAsync(int userId, TimeSpan duration, string endpoint)
    {
        return database.StringSetAsync(
            key: $"{userId}:{endpoint}", 
            value:"blocked", 
            expiry: duration,
            when: When.NotExists);
    }
    
}