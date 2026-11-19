using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;

namespace UserService.Application.Services;

public class UserBlockService(IUserBlockRepository repository) : IUserBlockService
{
    public Task<bool> IsBlockedAsync(int userId, string endpoint)
    {
        return repository.IsBlockedAsync(userId, endpoint);
    }
}