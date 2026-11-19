using RateLimiter.Writer.Domain;
using RateLimiter.Writer.Infrastructure.DbModels;
using Riok.Mapperly.Abstractions;

namespace RateLimiter.Writer.Infrastructure.Factories;

[Mapper]
public partial class LimitMappingFactory
{
    [MapperIgnoreSource(nameof(DbRateLimitModel.Id))]
    [MapperIgnoreSource(nameof(DbRateLimitModel.IsDeleted))]
    public partial RateLimit ToDomain(DbRateLimitModel limit);
}