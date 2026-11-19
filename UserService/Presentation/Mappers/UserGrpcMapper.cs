using Riok.Mapperly.Abstractions;
using UserService.Application.Services.Models;
using UserService.Domain;
using UserService.Presentation.Protos;

namespace UserService.Presentation.Mappers;

[Mapper]
public partial class UserGrpcMapper
{
    public partial CreateUser ToCreateUser(CreateUserRequest request);
    
    public partial UserResponse ToResponse(User user);

    public UpdateUser ToUpdateUser(UpdateUserRequest request)
    {
        return new UpdateUser
        {
            Password = request.HasPassword ? request.Password : null,
            Name = request.HasName ? request.Name : null,
            Surname = request.HasSurname ? request.Surname : null,
            Age = request.HasAge ? request.Age : null
        };

    }

}