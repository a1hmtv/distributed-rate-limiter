using StackExchange.Redis;
using UserService.Application.Abstractions.Repositories;

namespace UserService.Infrastructure.Repositories;

public class UserBlockRepository(IDatabase database) : IUserBlockRepository
{
    public Task<bool> IsBlockedAsync(int userId, string endpoint)
    {
        return database.KeyExistsAsync($"{userId}:{endpoint}");
    }
}