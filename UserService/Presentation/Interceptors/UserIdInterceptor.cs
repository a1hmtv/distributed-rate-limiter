using System.Globalization;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace UserService.Presentation.Interceptors;

public sealed class UserIdInterceptor : Interceptor
{
    private const string UserIdHeader = "user_id";
    internal const string UserIdContextKey = "userId";

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var headerValue = context.RequestHeaders.GetValue(UserIdHeader);

        if (!int.TryParse(headerValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId))
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated,
                "Pass the user_id header with the integer user identifier"));
        }

        context.UserState[UserIdContextKey] = userId;

        return continuation(request, context);
    }
}