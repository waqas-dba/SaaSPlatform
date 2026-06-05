// CoreKit.Infrastructure/Middleware/ExceptionHandling/Handlers/KeyNotFoundExceptionHandler.cs
namespace CoreKit.Infrastructure.Middleware.ExceptionHandling.Handlers;

public sealed class KeyNotFoundExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception)
        => exception is KeyNotFoundException;

    public (int StatusCode, string ErrorCode, string Message) Handle(Exception exception)
        => (404, "NOT_FOUND", "The requested resource was not found.");
}