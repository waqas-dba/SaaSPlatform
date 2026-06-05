// CoreKit.Infrastructure/Middleware/ExceptionHandling/Handlers/FallbackExceptionHandler.cs
namespace CoreKit.Infrastructure.Middleware.ExceptionHandling.Handlers;

public sealed class FallbackExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => true; // catches everything

    public (int StatusCode, string ErrorCode, string Message) Handle(Exception exception)
        => (500, "SERVER_ERROR",
            "Something went wrong on our end. Please try again in a moment.");
}