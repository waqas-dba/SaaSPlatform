// CoreKit.Infrastructure/Middleware/ExceptionHandling/Handlers/InvalidOperationExceptionHandler.cs
namespace CoreKit.Infrastructure.Middleware.ExceptionHandling.Handlers;

public sealed class InvalidOperationExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception)
        => exception is InvalidOperationException;

    public (int StatusCode, string ErrorCode, string Message) Handle(Exception exception)
        => (400, "INVALID_OPERATION", exception.Message);
}