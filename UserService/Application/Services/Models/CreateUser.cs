namespace UserService.Application.Services.Models;

public sealed record CreateUser
{
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public int Age { get; init; }
}