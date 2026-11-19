using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RateLimiter.Reader.Application.Abstractions.Services;

namespace RateLimiter.Reader.Controllers;

public class ExampleController(
    IRateLimitService rateLimitService) : Reader.ReaderBase
{
    public override Task<PingResponse> Ping(
        PingRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new PingResponse
        {
            Status = "Alive"
        });
    }

    public override Task<GetAllLimitsResponse> GetAllLimits(
        Empty request,
        ServerCallContext context)
    {
        var response = new GetAllLimitsResponse();

        foreach (var limit in rateLimitService.GetAllLimits())
        {
            response.Limits.Add(new RateLimitItem
            {
                Route = limit.Route,
                RequestsPerMinute = limit.RequestsPerMinute
            });
        }

        return Task.FromResult(response);
    }
}