using System.Net;
using System.Text.Json;
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
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = ApiResponse<object>.FailResponse(
            message: "An unexpected error occurred",
            code: "SERVER_ERROR",
            errors: new List<string> { ex.Message },
            traceId: context.TraceIdentifier);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}