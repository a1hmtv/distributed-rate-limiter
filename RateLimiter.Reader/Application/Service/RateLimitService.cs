using System.Collections.Concurrent;
using RateLimiter.Reader.Application.Abstractions.Repositories;
using RateLimiter.Reader.Application.Abstractions.Services;
using RateLimiter.Reader.Domain;

namespace RateLimiter.Reader.Application.Service;

public class RateLimitService(IRateLimitRepository repository) : IRateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimit> _rateLimits = new();

    private Task? _watchTask;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await foreach (var limit in repository.GetAllAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            _rateLimits[limit.Route] = limit;
        }

        cancellationToken.ThrowIfCancellationRequested();

        _watchTask = Task.Factory
            .StartNew(
                () => WatchChangesAsync(cancellationToken),
                cancellationToken,
                TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default)
            .Unwrap();
    }

    private async Task WatchChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (
                var change in repository.WatchChangesAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (change.Limit is not null)
                {
                    _rateLimits[change.Route] = change.Limit;
                }
                else
                {
                    _rateLimits.TryRemove(change.Route, out _);
                }
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        
        }
    }

    public IEnumerable<RateLimit> GetAllLimits()
    {
        return _rateLimits.Values;
    }
}