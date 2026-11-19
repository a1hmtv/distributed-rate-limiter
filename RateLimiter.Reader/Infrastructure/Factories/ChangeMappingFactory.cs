using RateLimiter.Reader.Domain;
using RateLimiter.Reader.Infrastructure.DbModels;

namespace RateLimiter.Reader.Infrastructure.Factories;

public class ChangeMappingFactory(LimitMappingFactory limitMapper)
{
    public RateLimitChange ToDomain(DbRateLimitModel model)
    {
        return new RateLimitChange(
            model.Route,
            model.IsDeleted ? null : limitMapper.ToDomain(model));
    }
}