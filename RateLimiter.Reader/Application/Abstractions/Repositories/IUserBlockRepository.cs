namespace RateLimiter.Reader.Application.Abstractions.Repositories;

public interface IUserBlockRepository
{
    Task<bool> IsBlockedAsync(int userId, string endpoint);
    Task<bool> TryBlockAsync(int userId, TimeSpan timeSpan, string endpoint);
}