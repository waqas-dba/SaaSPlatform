// CoreKit.Infrastructure/Middleware/ExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreKit.Infrastructure.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await WriteErrorAsync(context, ex);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        var (statusCode, clientMessage) = ex switch
        {
            UnauthorizedAccessException =>
                ((int)HttpStatusCode.Unauthorized, ex.Message),

            KeyNotFoundException =>
                ((int)HttpStatusCode.NotFound, ex.Message),

            InvalidOperationException =>
                ((int)HttpStatusCode.BadRequest, ex.Message),

            ArgumentException =>
                ((int)HttpStatusCode.BadRequest, ex.Message),

            // FIX: For unexpected exceptions return a generic message so
            // internal details (stack traces, connection strings, schema info)
            // are never sent to clients. The full exception is already logged.
            _ =>
                ((int)HttpStatusCode.InternalServerError,
                 "An unexpected error occurred. Please try again later.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new { success = false, message = clientMessage };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}