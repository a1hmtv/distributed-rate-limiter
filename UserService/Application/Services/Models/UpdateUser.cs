namespace UserService.Application.Services.Models;

public sealed record UpdateUser
{
    public string? Password { get; init; }

    public string? Name { get; init; }

    public string? Surname { get; init; }

    public int? Age { get; init; }
}