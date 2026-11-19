using Grpc.Core;
using UserService.Application.Abstractions.Services;
using UserService.Presentation.Protos;
using UserService.Presentation.Mappers;

namespace UserService.Presentation.Services;

public sealed class UserGrpcService : UserGrpc.UserGrpcBase
{
    private readonly IUserService _userService;
    private readonly UserGrpcMapper _mapper;

    public UserGrpcService(
        IUserService userService,
        UserGrpcMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    public override async Task<CreateUserResponse> CreateUser(
        CreateUserRequest request,
        ServerCallContext context)
    {
        var createUser = _mapper.ToCreateUser(request);

        var id = await _userService.CreateUserAsync(
            createUser,
            context.CancellationToken);

        return new CreateUserResponse
        {
            Id = id
        };
    }

    public override async Task<UserResponse> GetUserById(
        GetUserByIdRequest request,
        ServerCallContext context)
    {
        var user = await _userService.GetUserByIdAsync(
            request.Id,
            context.CancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                $"User with id {request.Id} was not found.");
        }

        return _mapper.ToResponse(user);
    }

    public override async Task<GetUserByNameResponse> GetUserByName(
        GetUserByNameRequest request,
        ServerCallContext context)
    {
        var users = await _userService.GetUserByNameAsync(
            request.Name,
            request.Surname,
            context.CancellationToken);

        var response = new GetUserByNameResponse();

        foreach (var user in users)
        {
            response.Users.Add(_mapper.ToResponse(user));
        }

        return response;
    }

    public override async Task<UserResponse> UpdateUser(
        UpdateUserRequest request,
        ServerCallContext context)
    {
        var updateUser = _mapper.ToUpdateUser(request);

        var user = await _userService.UpdateUserAsync(
            request.Id,
            updateUser,
            context.CancellationToken);

        return _mapper.ToResponse(user);
    }

    public override async Task<DeleteUserResponse> DeleteUser(
        DeleteUserRequest request,
        ServerCallContext context)
    {
        var deleted = await _userService.DeleteUserAsync(
            request.Id,
            context.CancellationToken);

        if (!deleted)
        {
            throw new KeyNotFoundException(
                $"User with id {request.Id} was not found.");
        }

        return new DeleteUserResponse
        {
            Deleted = true
        };
    }
}