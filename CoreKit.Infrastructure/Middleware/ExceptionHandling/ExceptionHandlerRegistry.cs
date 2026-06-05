// CoreKit.Infrastructure/Middleware/ExceptionHandling/ExceptionHandlerRegistry.cs
namespace CoreKit.Infrastructure.Middleware.ExceptionHandling;

public sealed class ExceptionHandlerRegistry
{
    private readonly IReadOnlyList<IExceptionHandler> _handlers;

    public ExceptionHandlerRegistry(IEnumerable<IExceptionHandler> handlers)
    {
        _handlers = handlers.ToList();
    }

    public (int StatusCode, string ErrorCode, string Message) Resolve(Exception exception)
    {
        foreach (var handler in _handlers)
            if (handler.CanHandle(exception))
                return handler.Handle(exception);

        // FallbackExceptionHandler always matches so this is unreachable
        // but satisfies the compiler
        return (500, "SERVER_ERROR", "An unexpected error occurred.");
    }
}