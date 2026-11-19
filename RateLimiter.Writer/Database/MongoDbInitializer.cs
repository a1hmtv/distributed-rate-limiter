using MongoDB.Driver;
using RateLimiter.Writer.Infrastructure.DbModels;

namespace RateLimiter.Writer.Database;

public class MongoDbInitializer(IMongoCollection<DbRateLimitModel> rateLimits)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var keys = Builders<DbRateLimitModel>.IndexKeys.Ascending(x => x.Route);
        
        var options = new CreateIndexOptions<DbRateLimitModel>
        {
            Name = "unique_route",
            Unique = true,
            PartialFilterExpression = Builders<DbRateLimitModel>.Filter.Eq(x => x.IsDeleted, false),
        };
        
        var index = new CreateIndexModel<DbRateLimitModel>(keys, options);
        
        await rateLimits.Indexes.CreateOneAsync(index, cancellationToken: cancellationToken);
    }
}