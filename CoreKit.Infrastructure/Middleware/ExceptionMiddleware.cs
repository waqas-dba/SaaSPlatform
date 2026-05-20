using System.Text.Json;
using CoreKit.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreKit.Infrastructure.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request cancelled by client: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorAsync(context, ex);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted) return;

        var (statusCode, message, errorCode) = Classify(ex);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new ApiErrorResponse
        {
            Success = false,
            ErrorCode = errorCode,
            Message = message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, JsonOptions));
    }

    private static (int StatusCode, string Message, string ErrorCode) Classify(Exception ex)
        => ex switch
        {
            // 401 — not authenticated
            UnauthorizedAccessException =>
                (401,
                 "You are not authorised to perform this action. Please log in and try again.",
                 "UNAUTHORIZED"),

            // 403 — authenticated but not permitted
            ForbiddenException =>
                (403,
                 ex.Message,
                 "FORBIDDEN"),

            KeyNotFoundException =>
                (404,
                 "The requested resource was not found.",
                 "NOT_FOUND"),

            InvalidOperationException =>
                (400,
                 ex.Message,
                 "INVALID_OPERATION"),

            ArgumentException =>
                (400,
                 ex.Message,
                 "BAD_REQUEST"),

            DbUpdateConcurrencyException =>
                (409,
                 "The record was modified by another user. Please refresh and try again.",
                 "CONFLICT"),

            DbUpdateException =>
                (422,
                 "The request could not be saved. Please check your input and try again.",
                 "UNPROCESSABLE"),

            TimeoutException =>
                (504,
                 "The request took too long. Please try again.",
                 "TIMEOUT"),

            _ =>
                (500,
                 "Something went wrong on our end. Please try again in a moment.",
                 "SERVER_ERROR")
        };

    private sealed class ApiErrorResponse
    {
        public bool Success { get; init; }
        public string ErrorCode { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }
}