// CoreKit.Infrastructure/Middleware/ExceptionMiddleware.cs
using System.Text.Json;
using CoreKit.Infrastructure.Middleware.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreKit.Infrastructure.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly ExceptionHandlerRegistry _registry;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        ExceptionHandlerRegistry registry)
    {
        _next = next;
        _logger = logger;
        _registry = registry;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request cancelled: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted) return;

        var (statusCode, errorCode, message) = _registry.Resolve(ex);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(
            new { success = false, errorCode, message },
            JsonOptions));
    }
}