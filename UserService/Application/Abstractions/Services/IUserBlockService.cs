namespace UserService.Application.Abstractions.Services;

public interface IUserBlockService
{
    Task<bool> IsBlockedAsync(int userId, string endpoint);
}