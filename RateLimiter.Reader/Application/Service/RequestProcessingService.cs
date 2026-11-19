using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using RateLimiter.Reader.Application.Abstractions.Services;
using RateLimiter.Reader.Domain;

namespace RateLimiter.Reader.Application.Service;

public sealed class RequestProcessingService(
    IRateLimitService rateLimitService,
    UserBlockService userBlockService)
    : IRequestProcessingService, IDisposable
{
    private static readonly TimeSpan RequestWindow = TimeSpan.FromMinutes(1);

    private readonly MemoryCache _counters = new(new MemoryCacheOptions());
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task ProcessAsync(
        UserRequestEvent request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Endpoint);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (await userBlockService.IsBlockedAsync(request.UserId, request.Endpoint))
                return;

            var limit = rateLimitService.GetAllLimits()
                .FirstOrDefault(x => x.Route == request.Endpoint);

            if (limit is null)
                return;

            var key = (request.UserId, request.Endpoint);
            var now = Stopwatch.GetTimestamp();

            var timestamps = _counters.Get<Queue<long>>(key) ?? new Queue<long>();

            while (timestamps.TryPeek(out var oldest) && Stopwatch.GetElapsedTime(oldest, now) >= RequestWindow)
            {
                timestamps.Dequeue();
            }

            if (timestamps.Count >= limit.RequestsPerMinute)
            {
                await userBlockService.TryBlockAsync(request.UserId, request.Endpoint);
                _counters.Remove(key);
                return;
            }

            timestamps.Enqueue(now);

            _counters.Set(
                key,
                timestamps,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = RequestWindow
                });
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose()
    {
        _counters.Dispose();
        _gate.Dispose();
    }
}