using RateLimiter.Reader.Domain;
using RateLimiter.Reader.Infrastructure.DbModels;
using Riok.Mapperly.Abstractions;

namespace RateLimiter.Reader.Infrastructure.Factories;

[Mapper]
public partial class LimitMappingFactory
{
    [MapperIgnoreSource(nameof(DbRateLimitModel.Id))]
    [MapperIgnoreSource(nameof(DbRateLimitModel.IsDeleted))]
    public partial RateLimit ToDomain(DbRateLimitModel limit);
}