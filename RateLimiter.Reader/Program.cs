using MongoDB.Driver;
using RateLimiter.Reader.Application.Abstractions.Repositories;
using RateLimiter.Reader.Application.Abstractions.Services;
using RateLimiter.Reader.Application.Service;
using RateLimiter.Reader.Controllers;
using RateLimiter.Reader.Infrastructure.DbModels;
using RateLimiter.Reader.Infrastructure.Factories;
using RateLimiter.Reader.Infrastructure.HostedServices;
using RateLimiter.Reader.Infrastructure.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddSingleton<IMongoClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var connectionString = configuration["MongoDb:ConnectionString"] ?? throw new InvalidOperationException("Не задана настройка MongoDb:ConnectionString.");

    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var databaseName = configuration["MongoDb:DatabaseName"] ?? throw new InvalidOperationException("Не задана настройка MongoDb:DatabaseName.");

    var client = provider.GetRequiredService<IMongoClient>();

    return client.GetDatabase(databaseName);
});

builder.Services.AddSingleton<IMongoCollection<DbRateLimitModel>>(
    provider =>
    {
        var database = provider.GetRequiredService<IMongoDatabase>();

        return database.GetCollection<DbRateLimitModel>("rate_limits");
    });

builder.Services.AddSingleton<LimitMappingFactory>();
builder.Services.AddSingleton<ChangeMappingFactory>();

builder.Services.AddSingleton<IRateLimitRepository, RateLimitRepository>();

builder.Services.AddSingleton<IRateLimitService, RateLimitService>();

builder.Services.AddHostedService<RateLimitHostedService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var connectionString = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Не задана строка подключения Redis.");

    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddSingleton<IDatabase>(provider =>
{
    var connection = provider.GetRequiredService<IConnectionMultiplexer>();

    return connection.GetDatabase();
});

builder.Services.AddSingleton<IUserBlockRepository, UserBlockRepository>();
builder.Services.AddSingleton<UserBlockService>();
builder.Services.AddSingleton<IRequestProcessingService, RequestProcessingService>();

builder.Services.AddHostedService<UserRequestsConsumer>();

var app = builder.Build();

app.MapGrpcService<ExampleController>();

await app.RunAsync("http://*:5000");
