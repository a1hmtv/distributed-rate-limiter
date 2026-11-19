namespace UserService.Infrastructure.DbModels;

public sealed class DbUserModel
{
    public int Id  { get; set; }
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string Name  { get; set; }
    public required string Surname { get; set; }
    public int Age { get; set; }
    public bool IsDeleted { get; set; }
}