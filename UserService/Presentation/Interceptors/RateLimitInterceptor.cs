using Grpc.Core;
using Grpc.Core.Interceptors;
using UserService.Application.Abstractions.Services;

namespace UserService.Presentation.Interceptors;

public class RateLimitInterceptor(IUserBlockService userBlockService) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var userId = (int)context.UserState["userId"];
        
        var endpoint = context.Method.Split('/')[^1];
        
        if (await userBlockService.IsBlockedAsync(userId, endpoint))
        {
            throw new RpcException(new Status(
                StatusCode.ResourceExhausted,
                $"Превышено разрешённое количество запросов в минуту к методу {endpoint}."));
        }

        return await continuation(request, context);
    }
}