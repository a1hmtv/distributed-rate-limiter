using FluentValidation;
using Npgsql;
using StackExchange.Redis;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Application.Services;
using UserService.Application.Services.Models;
using UserService.Application.Services.Validators;
using UserService.Infrastructure.Factories;
using UserService.Infrastructure.Repositories;
using UserService.Presentation.Interceptors;
using UserService.Presentation.Mappers;
using UserService.Presentation.Services;

using ApplicationUserService =
    UserService.Application.Services.UserService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GrpcExceptionInterceptor>();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
    options.Interceptors.Add<UserIdInterceptor>();
    options.Interceptors.Add<RateLimitInterceptor>();
});

builder.Services.AddSingleton<NpgsqlDataSource>(serviceProvider =>
{
    var configuration =
        serviceProvider.GetRequiredService<IConfiguration>();

    var connectionString =
        configuration.GetConnectionString("UsersDatabase")
        ?? throw new InvalidOperationException(
            "Connection string 'UsersDatabase' was not found.");

    return NpgsqlDataSource.Create(connectionString);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Connection string 'Redis' was not found.");

    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddSingleton<IDatabase>(serviceProvider =>
    serviceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

builder.Services.AddSingleton<UserMappingFactory>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUserBlockRepository, UserBlockRepository>();

builder.Services.AddSingleton<
    IValidator<CreateUser>,
    CreateUserValidator>();

builder.Services.AddSingleton<
    IValidator<UpdateUser>,
    UpdateUserValidator>();

builder.Services.AddSingleton<
    IUserService,
    ApplicationUserService>();
builder.Services.AddSingleton<IUserBlockService, UserBlockService>();

builder.Services.AddSingleton<UserGrpcMapper>();
builder.Services.AddSingleton<UserGrpcService>();

var app = builder.Build();

app.MapGrpcService<UserGrpcService>();

app.Run();
