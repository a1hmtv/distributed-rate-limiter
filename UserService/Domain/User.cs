namespace UserService.Domain;

public class User
{
    public int Id { get; private set; }
    public string Login { get; private set; }
    public string Password { get; private set; }
    public string Name{ get; private set; }
    public string Surname{ get; private set; }
    public int Age{ get; private set; }

    public User(int id, string login, string password, string name, string surname, int age)
    {
        Id = id;
        Login = login;
        Password = password;
        Name = name;
        Surname = surname;
        Age = age;
    }
}