using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreKit.Infrastructure.Middleware;

// Convention-based (NOT IMiddleware) — no DI registration needed
// ASP.NET Core instantiates this directly from the constructor
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

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("[BOOT] RequestTracingMiddleware instantiated OK");
        Console.ResetColor();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Guid.NewGuid().ToString()[..8].ToUpper();
        context.Items["TraceId"] = traceId;

        var method = context.Request.Method;
        var path = context.Request.Path;

        var hasTenant = context.Request.Headers
            .TryGetValue("x-tenant-id", out var tenantHeader);
        var hasAuth = context.Request.Headers
            .ContainsKey("Authorization");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[{traceId}] ======= NEW REQUEST =======");
        Console.WriteLine($"[{traceId}] {method} {path}");
        Console.WriteLine($"[{traceId}] x-tenant-id : {(hasTenant ? tenantHeader.ToString() : "NOT PROVIDED")}");
        Console.WriteLine($"[{traceId}] Authorization: {(hasAuth ? "PROVIDED" : "NOT PROVIDED")}");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[{traceId}] >>> PASSING TO ExceptionMiddleware");
        Console.ResetColor();

        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _next(context);
            sw.Stop();

            var color = context.Response.StatusCode switch
            {
                >= 500 => ConsoleColor.Red,
                >= 400 => ConsoleColor.Yellow,
                _ => ConsoleColor.Green
            };

            Console.ForegroundColor = color;
            Console.WriteLine($"[{traceId}] <<< FINAL STATUS {context.Response.StatusCode} — {sw.ElapsedMilliseconds}ms");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{traceId}] <<< UNHANDLED EXCEPTION after {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"[{traceId}]     {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();
            throw;
        }
    }
}