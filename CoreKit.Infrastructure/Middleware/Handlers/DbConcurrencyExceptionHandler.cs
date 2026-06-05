// CoreKit.Infrastructure/Middleware/ExceptionHandling/Handlers/DbExceptionHandlers.cs
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Infrastructure.Middleware.ExceptionHandling.Handlers;

public sealed class DbConcurrencyExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception)
        => exception is DbUpdateConcurrencyException;

    public (int StatusCode, string ErrorCode, string Message) Handle(Exception exception)
        => (409, "CONFLICT",
            "The record was modified by another user. Please refresh and try again.");
}

public sealed class DbUpdateExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception)
        => exception is DbUpdateException;

    public (int StatusCode, string ErrorCode, string Message) Handle(Exception exception)
        => (422, "UNPROCESSABLE",
            "The request could not be saved. Please check your input and try again.");
}