using RateLimiter.Reader.Application.Abstractions.Services;

namespace RateLimiter.Reader.Infrastructure.HostedServices;

public class RateLimitHostedService(
    IRateLimitService rateLimitService,
    IHostApplicationLifetime applicationLifetime) : IHostedService, IDisposable
{
    private CancellationTokenSource? _lifetimeCts;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetimeCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            applicationLifetime.ApplicationStopping);

        await rateLimitService.InitializeAsync(_lifetimeCts.Token);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _lifetimeCts?.Cancel();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _lifetimeCts?.Dispose();
    }
}