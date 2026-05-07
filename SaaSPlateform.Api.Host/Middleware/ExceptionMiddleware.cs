using System.Net;
using System.Text.Json;
using SaaSPlatform.Core.Common.Responses;

namespace SaaSPlateform.Api.Host.Middleware;

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
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType = "application/json";

            var response =
                ApiResponse<object>.FailResponse(
                    "Internal server error",
                    "SERVER_ERROR",
                    traceId: context.TraceIdentifier);

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}