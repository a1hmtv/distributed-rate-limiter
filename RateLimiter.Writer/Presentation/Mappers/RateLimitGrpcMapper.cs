using RateLimiter.Writer.Application.Services.Models;
using RateLimiter.Writer.Domain;
using RateLimiter.Writer.Protos;
using Riok.Mapperly.Abstractions;

namespace RateLimiter.Writer.Presentation.Mappers;

[Mapper]
public partial class RateLimitGrpcMapper
{
    public partial CreateLimit ToCreateLimit(
        CreateLimitRequest request);

    public partial UpdateLimit ToUpdateLimit(
        UpdateLimitRequest request);

    public partial RateLimitResponse ToResponse(
        RateLimit rateLimit);

    [ObjectFactory]
    private static RateLimitResponse CreateRateLimitResponse()
    {
        return new RateLimitResponse();
    }
}