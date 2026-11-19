using FluentValidation;
using MongoDB.Driver;
using RateLimiter.Writer.Application.Abstractions.Repositories;
using RateLimiter.Writer.Application.Abstractions.Services;
using RateLimiter.Writer.Application.Services;
using RateLimiter.Writer.Application.Services.Models;
using RateLimiter.Writer.Application.Services.Validators;
using RateLimiter.Writer.Database;
using RateLimiter.Writer.Infrastructure.DbModels;
using RateLimiter.Writer.Infrastructure.Factories;
using RateLimiter.Writer.Infrastructure.Repositories;
using RateLimiter.Writer.Presentation.Interceptors;
using RateLimiter.Writer.Presentation.Mappers;
using RateLimiter.Writer.Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["MongoDb:ConnectionString"]
    ?? throw new InvalidOperationException("MongoDb:ConnectionString is not configured.");

var databaseName = builder.Configuration["MongoDb:DatabaseName"]
    ?? throw new InvalidOperationException("MongoDb:DatabaseName is not configured.");

builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase(databaseName);
});

builder.Services.AddSingleton<IMongoCollection<DbRateLimitModel>>(serviceProvider =>
{
    var database = serviceProvider.GetRequiredService<IMongoDatabase>();

    return database.GetCollection<DbRateLimitModel>("rate_limits");
});

builder.Services.AddSingleton<LimitMappingFactory>();

builder.Services.AddSingleton<IRateLimitRepository, RateLimitRepository>();

builder.Services.AddSingleton<MongoDbInitializer>();

builder.Services.AddSingleton<IValidator<CreateLimit>, CreateLimitValidator>();
builder.Services.AddSingleton<IValidator<UpdateLimit>, UpdateLimitValidator>();

builder.Services.AddSingleton<IRateLimitService, RateLimitService>();

builder.Services.AddSingleton<RateLimitGrpcMapper>();
builder.Services.AddSingleton<GrpcExceptionInterceptor>();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcExceptionInterceptor>();
});

var app = builder.Build();

var mongoDbInitializer =
    app.Services.GetRequiredService<MongoDbInitializer>();

await mongoDbInitializer.InitializeAsync(
    CancellationToken.None);

app.MapGrpcService<WriterGrpcService>();

await app.RunAsync("http://*:5001");
