using FluentValidation;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Application.Exceptions;
using UserService.Application.Services.Models;
using UserService.Domain;

namespace UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateUser> _createUserValidator;
    private readonly IValidator<UpdateUser> _updateUserValidator;

    public UserService(
        IUserRepository userRepository,
        IValidator<CreateUser> createUserValidator,
        IValidator<UpdateUser> updateUserValidator)
    {
        _userRepository = userRepository;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
    }

    public Task<int> CreateUserAsync(
        CreateUser createUser,
        CancellationToken cancellationToken)
    {
        _createUserValidator.ValidateAndThrow(createUser);

        return _userRepository.CreateUserAsync(
            createUser.Login,
            createUser.Password,
            createUser.Name,
            createUser.Surname,
            createUser.Age,
            cancellationToken);
    }

    public Task<User?> GetUserByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        ValidateId(id);

        return _userRepository.GetUserByIdAsync(
            id,
            cancellationToken);
    }

    public Task<User[]> GetUserByNameAsync(
        string name,
        string surname,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be empty");

        if (string.IsNullOrWhiteSpace(surname))
            throw new ArgumentException("Surname must not be empty");

        return _userRepository.GetUserByNameAsync(
            name,
            surname,
            cancellationToken);
    }

    public async Task<User> UpdateUserAsync(
        int id,
        UpdateUser updateUser,
        CancellationToken cancellationToken)
    {
        ValidateId(id);
        _updateUserValidator.ValidateAndThrow(updateUser);

        var user = await _userRepository.UpdateUserAsync(
            id,
            updateUser,
            cancellationToken);

        if (user is null)
            throw new UserNotFoundException(id);

        return user;
    }

    public Task<bool> DeleteUserAsync(
        int id,
        CancellationToken cancellationToken)
    {
        ValidateId(id);

        return _userRepository.DeleteUserAsync(
            id,
            cancellationToken);
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
            throw new ArgumentException(
                "Id must be greater than zero");
    }
}