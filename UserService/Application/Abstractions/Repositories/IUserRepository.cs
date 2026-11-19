using UserService.Application.Services.Models;
using UserService.Domain;

namespace UserService.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<int> CreateUserAsync(
        string login,
        string password,
        string name,
        string surname,
        int age,
        CancellationToken cancellationToken);

    Task<User?> GetUserByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<User[]> GetUserByNameAsync(
        string name,
        string surname,
        CancellationToken cancellationToken);

    Task<User?> UpdateUserAsync(
        int id,
        UpdateUser updateUser,
        CancellationToken cancellationToken);

    Task<bool> DeleteUserAsync(
        int id,
        CancellationToken cancellationToken);
}