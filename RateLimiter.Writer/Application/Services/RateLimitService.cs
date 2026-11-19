using FluentValidation;
using RateLimiter.Writer.Application.Abstractions.Repositories;
using RateLimiter.Writer.Application.Abstractions.Services;
using RateLimiter.Writer.Application.Exceptions;
using RateLimiter.Writer.Application.Services.Models;
using RateLimiter.Writer.Domain;

namespace RateLimiter.Writer.Application.Services;

public sealed class RateLimitService : IRateLimitService
{
    private readonly IRateLimitRepository _rateLimitRepository;
    private readonly IValidator<CreateLimit> _createLimitValidator;
    private readonly IValidator<UpdateLimit> _updateLimitValidator;

    public RateLimitService(
        IRateLimitRepository rateLimitRepository,
        IValidator<CreateLimit> createLimitValidator,
        IValidator<UpdateLimit> updateLimitValidator)
    {
        _rateLimitRepository = rateLimitRepository;
        _createLimitValidator = createLimitValidator;
        _updateLimitValidator = updateLimitValidator;
    }

    public Task<string> CreateLimitAsync(
        CreateLimit createLimit,
        CancellationToken cancellationToken)
    {
        _createLimitValidator.ValidateAndThrow(createLimit);

        return _rateLimitRepository.CreateLimitAsync(
            createLimit.Route,
            createLimit.RequestsPerMinute,
            cancellationToken);
    }

    public Task<int?> GetLimitByRouteAsync(
        string route,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            throw new ArgumentException("Route must not be empty.");
        }

        return _rateLimitRepository.GetLimitByRouteAsync(
            route,
            cancellationToken);
    }

    public async Task<RateLimit> UpdateLimitAsync(
        UpdateLimit updateLimit,
        CancellationToken cancellationToken)
    {
        _updateLimitValidator.ValidateAndThrow(updateLimit);

        RateLimit? result = await _rateLimitRepository.UpdateLimitAsync(
            updateLimit.Route,
            updateLimit.RequestsPerMinute,
            cancellationToken);

        if (result is null)
        {
            throw new RateLimitNotFoundException(updateLimit.Route);
        }

        return result;
    }

    public Task<bool> DeleteLimitAsync(
        string route,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            throw new ArgumentException("Route must not be empty.");
        }

        return _rateLimitRepository.DeleteLimitAsync(
            route,
            cancellationToken);
    }
}