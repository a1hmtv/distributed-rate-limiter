using Riok.Mapperly.Abstractions;
using UserService.Domain;
using UserService.Infrastructure.DbModels;

namespace UserService.Infrastructure.Factories;

[Mapper]
public partial class UserMappingFactory
{
    [MapperIgnoreSource(nameof(DbUserModel.IsDeleted))]
    public partial User ToDomain(DbUserModel user);
}