// CoreKit.Infrastructure/Middleware/ExceptionHandling/Handlers/ForbiddenExceptionHandler.cs
using CoreKit.SharedKernel.Exceptions;

namespace CoreKit.Infrastructure.Middleware.ExceptionHandling.Handlers;

public sealed class ForbiddenExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception)
        => exception is ForbiddenException;

    public (int StatusCode, string ErrorCode, string Message) Handle(Exception exception)
        => (403, "FORBIDDEN", exception.Message);
}