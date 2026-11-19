using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using UserService.Application.Exceptions;

namespace UserService.Presentation.Interceptors;

public sealed class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(
        ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (ValidationException exception)
        {
            throw CreateRpcException(
                StatusCode.InvalidArgument,
                exception.Message);
        }
        catch (ArgumentException exception)
        {
            throw CreateRpcException(
                StatusCode.InvalidArgument,
                exception.Message);
        }
        catch (UserNotFoundException exception)
        {
            throw CreateRpcException(
                StatusCode.NotFound,
                exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            throw CreateRpcException(
                StatusCode.NotFound,
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            throw CreateRpcException(
                StatusCode.AlreadyExists,
                exception.Message);
        }
        catch (OperationCanceledException)
        {
            throw CreateRpcException(
                StatusCode.Cancelled,
                "The request was cancelled.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled error while processing gRPC request.");

            throw CreateRpcException(
                StatusCode.Internal,
                "Internal server error.");
        }
    }

    private static RpcException CreateRpcException(
        StatusCode statusCode,
        string message)
    {
        return new RpcException(
            new Status(statusCode, message));
    }
}