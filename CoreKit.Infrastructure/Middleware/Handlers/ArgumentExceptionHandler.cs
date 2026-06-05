// CoreKit.Infrastructure/Middleware/ExceptionHandling/Handlers/ArgumentExceptionHandler.cs
namespace CoreKit.Infrastructure.Middleware.ExceptionHandling.Handlers;

public sealed class ArgumentExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception)
        => exception is ArgumentException;

    public (int StatusCode, string ErrorCode, string Message) Handle(Exception exception)
        => (400, "BAD_REQUEST", exception.Message);
}