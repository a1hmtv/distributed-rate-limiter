using UserService.Application.Services.Models;
using UserService.Domain;

namespace UserService.Application.Abstractions.Services;

public interface IUserService
{
    Task<int> CreateUserAsync(
        CreateUser createUser,
        CancellationToken cancellationToken);

    Task<User?> GetUserByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<User[]> GetUserByNameAsync(
        string name,
        string surname,
        CancellationToken cancellationToken);

    Task<User> UpdateUserAsync(
        int id,
        UpdateUser updateUser,
        CancellationToken cancellationToken);

    Task<bool> DeleteUserAsync(
        int id,
        CancellationToken cancellationToken);
}