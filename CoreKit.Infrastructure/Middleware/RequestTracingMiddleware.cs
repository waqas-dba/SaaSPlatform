using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreKit.Infrastructure.Middleware;

public class RequestTracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTracingMiddleware> _logger;

    public RequestTracingMiddleware(
        RequestDelegate next,
        ILogger<RequestTracingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Guid.NewGuid().ToString()[..8].ToUpper();
        context.Items["TraceId"] = traceId;

        var method = context.Request.Method;
        var path = context.Request.Path;
        var hasTenant = context.Request.Headers.ContainsKey("x-tenant-id");
        var hasAuth = context.Request.Headers.ContainsKey("Authorization");

        using var scope = _logger.BeginScope(
            new Dictionary<string, object> { ["TraceId"] = traceId });

        _logger.LogInformation(
            "Incoming {Method} {Path} | tenant={HasTenant} auth={HasAuth}",
            method, path, hasTenant, hasAuth);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _next(context);
            sw.Stop();

            _logger.LogInformation(
                "Completed {Method} {Path} → {Status} in {ElapsedMs}ms",
                method, path, context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Unhandled exception in {Method} {Path} after {ElapsedMs}ms",
                method, path, sw.ElapsedMilliseconds);
            throw;
        }
    }
}