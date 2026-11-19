namespace UserService.Application.Abstractions.Repositories;

public interface IUserBlockRepository
{
    Task<bool> IsBlockedAsync(int userId, string endpoint);
}