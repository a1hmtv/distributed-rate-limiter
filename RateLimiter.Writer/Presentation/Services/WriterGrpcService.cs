using Grpc.Core;
using RateLimiter.Writer.Application.Abstractions.Services;
using RateLimiter.Writer.Presentation.Mappers;
using RateLimiter.Writer.Protos;

using WriterGrpc = RateLimiter.Writer.Protos.Writer;

namespace RateLimiter.Writer.Presentation.Services;

public sealed class WriterGrpcService : WriterGrpc.WriterBase
{
    private readonly IRateLimitService _rateLimitService;
    private readonly RateLimitGrpcMapper _mapper;

    public WriterGrpcService(
        IRateLimitService rateLimitService,
        RateLimitGrpcMapper mapper)
    {
        _rateLimitService = rateLimitService;
        _mapper = mapper;
    }

    public override async Task<CreateLimitResponse> CreateLimit(
        CreateLimitRequest request,
        ServerCallContext context)
    {
        var createLimit = _mapper.ToCreateLimit(request);

        var route = await _rateLimitService.CreateLimitAsync(
            createLimit,
            context.CancellationToken);

        return new CreateLimitResponse
        {
            Route = route
        };
    }

    public override async Task<RateLimitResponse> GetLimitByRoute(
        GetLimitByRouteRequest request,
        ServerCallContext context)
    {
        var requestsPerMinute =
            await _rateLimitService.GetLimitByRouteAsync(
                request.Route,
                context.CancellationToken);

        if (requestsPerMinute is null)
        {
            throw new KeyNotFoundException(
                $"Rate limit for route '{request.Route}' was not found.");
        }

        return new RateLimitResponse
        {
            Route = request.Route,
            RequestsPerMinute = requestsPerMinute.Value
        };
    }

    public override async Task<RateLimitResponse> UpdateLimit(
        UpdateLimitRequest request,
        ServerCallContext context)
    {
        var updateLimit = _mapper.ToUpdateLimit(request);

        var rateLimit = await _rateLimitService.UpdateLimitAsync(
            updateLimit,
            context.CancellationToken);

        return _mapper.ToResponse(rateLimit);
    }

    public override async Task<DeleteLimitResponse> DeleteLimit(
        DeleteLimitRequest request,
        ServerCallContext context)
    {
        var deleted = await _rateLimitService.DeleteLimitAsync(
            request.Route,
            context.CancellationToken);

        if (!deleted)
        {
            throw new KeyNotFoundException(
                $"Rate limit for route '{request.Route}' was not found.");
        }

        return new DeleteLimitResponse
        {
            Deleted = true
        };
    }
}