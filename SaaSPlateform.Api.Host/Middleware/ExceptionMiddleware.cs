using System.Net;
using System.Text.Json;
using FluentValidation;
using SaaSPlatform.Api.Host.Responses;

namespace SaaSPlatform.Api.Host.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex,
                "Unhandled exception | TraceId: {TraceId}",
                context.TraceIdentifier);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, code, errors) = ex switch
        {
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                ex.Message,
                "UNAUTHORIZED",
                null as List<string>
            ),

            // FluentValidation throws ValidationException
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                "Validation failed",
                "VALIDATION_ERROR",
                validationEx.Errors.Select(e => e.ErrorMessage).ToList()
            ),

            ArgumentException => (
                (int)HttpStatusCode.BadRequest,
                ex.Message,
                "BAD_REQUEST",
                null
            ),

            InvalidOperationException => (
                (int)HttpStatusCode.BadRequest,
                ex.Message,
                "INVALID_OPERATION",
                null
            ),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred",
                "SERVER_ERROR",
                new List<string> { ex.Message }  // only for non‑production; remove later
            )
        };

        var response = ApiResponse<object>.FailResponse(
            message: message,
            code: code,
            errors: errors,
            traceId: context.TraceIdentifier
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}