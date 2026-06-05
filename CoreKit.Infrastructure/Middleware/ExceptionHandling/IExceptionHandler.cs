// CoreKit.Infrastructure/Middleware/ExceptionHandling/IExceptionHandler.cs
namespace CoreKit.Infrastructure.Middleware.ExceptionHandling;

public interface IExceptionHandler
{
    bool CanHandle(Exception exception);
    (int StatusCode, string ErrorCode, string Message) Handle(Exception exception);
}