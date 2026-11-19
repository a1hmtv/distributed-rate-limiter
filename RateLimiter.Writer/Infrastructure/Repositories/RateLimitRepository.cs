using MongoDB.Driver;
using RateLimiter.Writer.Application.Abstractions.Repositories;
using RateLimiter.Writer.Domain;
using RateLimiter.Writer.Infrastructure.DbModels;
using RateLimiter.Writer.Infrastructure.Factories;

namespace RateLimiter.Writer.Infrastructure.Repositories;

public class RateLimitRepository(IMongoCollection<DbRateLimitModel> rateLimits, LimitMappingFactory mapper) : IRateLimitRepository
{
    private static readonly FindOneAndUpdateOptions<DbRateLimitModel> ReturnAfterOptions =
        new()
        {
            ReturnDocument = ReturnDocument.After
        };
    
    public async Task<string> CreateLimitAsync(string route, int limit, CancellationToken cancellationToken)
    {
        var model = new DbRateLimitModel(route, limit);

        try
        {
            await rateLimits.InsertOneAsync(model, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException e) when (e.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new InvalidOperationException(e.Message);
        }
        return model.Route;
    }

    public async Task<int?> GetLimitByRouteAsync(string route, CancellationToken cancellationToken)
    {
        var model = await rateLimits.Find(x => x.Route == route && !x.IsDeleted ).SingleOrDefaultAsync(cancellationToken);
        return model?.RequestsPerMinute;
    }

    public async Task<RateLimit?> UpdateLimitAsync(string route, int limit, CancellationToken cancellationToken)
    {
        var update = Builders<DbRateLimitModel>.Update.Set(x => x.RequestsPerMinute, limit);
        
        var model = await rateLimits.FindOneAndUpdateAsync(x => x.Route == route && !x.IsDeleted, update, ReturnAfterOptions, cancellationToken);
        
        return model is null ? null : mapper.ToDomain(model);
    }

    public async Task<bool> DeleteLimitAsync(string route, CancellationToken cancellationToken)
    {
        var update = Builders<DbRateLimitModel>.Update.Set(x => x.IsDeleted, true);
        
        var model = await rateLimits.FindOneAndUpdateAsync(x => x.Route == route && !x.IsDeleted, update, ReturnAfterOptions, cancellationToken);

        return model is not null;
    }
    
}