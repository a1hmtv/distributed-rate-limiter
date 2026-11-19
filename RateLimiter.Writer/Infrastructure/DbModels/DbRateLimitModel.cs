using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RateLimiter.Writer.Infrastructure.DbModels;

public class DbRateLimitModel(string route, int limit)
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("route")] public string Route { get; set; } = route;

    [BsonElement("requests_per_minute")] public int RequestsPerMinute { get; set; } = limit;
    
    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; } 

}