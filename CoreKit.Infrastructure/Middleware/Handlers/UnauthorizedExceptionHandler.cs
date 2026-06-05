// CoreKit.Infrastructure/Middleware/ExceptionHandling/Handlers/UnauthorizedExceptionHandler.cs
namespace CoreKit.Infrastructure.Middleware.ExceptionHandling.Handlers;

public sealed class UnauthorizedExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception)
        => exception is UnauthorizedAccessException;

    public (int StatusCode, string ErrorCode, string Message) Handle(Exception exception)
        => (401,
            "UNAUTHORIZED",
            "You are not authorised to perform this action.");
}