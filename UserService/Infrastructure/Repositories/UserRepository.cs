using System.Data;
using Dapper;
using Npgsql;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Services.Models;
using UserService.Domain;
using UserService.Infrastructure.DbModels;
using UserService.Infrastructure.Factories;

namespace UserService.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly UserMappingFactory _mappingFactory;

    public UserRepository(NpgsqlDataSource dataSource, UserMappingFactory mappingFactory)
    {
        _dataSource = dataSource;
        _mappingFactory = mappingFactory;
    }

    public async Task<int> CreateUserAsync(
        string login,
        string password,
        string name,
        string surname,
        int age,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();

        parameters.Add("login", login);
        parameters.Add("password", password);
        parameters.Add("name", name);
        parameters.Add("surname", surname);
        parameters.Add("age", age);

        var command = new CommandDefinition(
            "SELECT * FROM public.create_user(@login, @password, @name, @surname, @age);",
            parameters, cancellationToken : cancellationToken);

        try
        {
            return await connection.QuerySingleAsync<int>(command);
        }
        catch (PostgresException e)
            when (e.SqlState == PostgresErrorCodes.UniqueViolation && e.ConstraintName == "users_login_key")
        {
            throw new InvalidOperationException("User with login already exists", e);
        }
        catch (PostgresException e)
            when (e.SqlState == PostgresErrorCodes.CheckViolation && e.ConstraintName == "users_age_check")
        {
            throw new ArgumentOutOfRangeException(nameof(age), age,"Invalid user age");
        }

    }

    public async Task<User?> GetUserByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        
        var parameters = new DynamicParameters();
        parameters.Add("id_p", id);

        var command = new CommandDefinition(
            "SELECT id, login, password, name, surname, age, is_deleted AS IsDeleted " +
            "FROM public.get_user_by_id(@id_p);", 
            parameters, cancellationToken : cancellationToken);

        var dbUser = await connection.QuerySingleOrDefaultAsync<DbUserModel>(command);
        
        return dbUser is null ? null : _mappingFactory.ToDomain(dbUser);
    }

    public async Task<User[]> GetUserByNameAsync(
        string name,
        string surname,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("name_p", name);
        parameters.Add("surname_p", surname);

        var command = new CommandDefinition(
            "SELECT id, login, password, name, surname, age, is_deleted AS IsDeleted " +
            "FROM public.get_user_by_name(@name_p, @surname_p); ", 
            parameters, cancellationToken: cancellationToken);
        
        var dbUsers = await connection.QueryAsync<DbUserModel>(command);
        
        return dbUsers.Select(_mappingFactory.ToDomain).ToArray();
    }

    public async Task<User?> UpdateUserAsync(
        int id,
        UpdateUser updateUser,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("id_p", id, DbType.Int32);
        parameters.Add("password_p", updateUser.Password, DbType.String);
        parameters.Add("name_p", updateUser.Name, DbType.String);
        parameters.Add("surname_p", updateUser.Surname, DbType.String);
        parameters.Add("age_p", updateUser.Age, DbType.Int32);

        var command = new CommandDefinition(
            "SELECT id, login, password, name, surname, age, is_deleted AS IsDeleted " +
            "FROM public.update_user(id_p => @id_p, password_p => @password_p, " +
            "name_p => @name_p, surname_p => @surname_p, age_p => @age_p); ",
            parameters, cancellationToken: cancellationToken);

        try
        {
            var dbUser = await connection.QuerySingleOrDefaultAsync<DbUserModel>(command);
        
            return dbUser is null ? null : _mappingFactory.ToDomain(dbUser); 
        }
        catch (PostgresException e)
            when (e.SqlState == PostgresErrorCodes.CheckViolation && e.ConstraintName == "users_age_check")
        {
            throw new ArgumentOutOfRangeException(nameof(updateUser.Age), updateUser.Age,"Invalid user age");
        }
        
    }

    public async Task<bool> DeleteUserAsync(
        int id,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("id_p", id);

        var command = new CommandDefinition(
            "SELECT * FROM public.delete_user(@id_p);", 
            parameters, cancellationToken: cancellationToken);
        
        var dbAnswer = await connection.QuerySingleAsync<bool?>(command);
        
        return dbAnswer ??  false;
    }
}