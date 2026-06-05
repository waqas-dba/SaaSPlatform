// CoreKit.Infrastructure/Extensions/MvcExtensions.cs — register handlers
using CoreKit.Infrastructure.Filters;
using CoreKit.Infrastructure.Middleware.ExceptionHandling;
using CoreKit.Infrastructure.Middleware.ExceptionHandling.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace CoreKit.Infrastructure.Extensions;

public static class MvcExtensions
{
    public static IServiceCollection AddCoreKitControllers(
        this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });

        services.AddScoped<ValidationFilter>();

        // OCP: register all handlers — add new ones here without
        // touching ExceptionMiddleware
        services.AddSingleton<IExceptionHandler, UnauthorizedExceptionHandler>();
        services.AddSingleton<IExceptionHandler, ForbiddenExceptionHandler>();
        services.AddSingleton<IExceptionHandler, KeyNotFoundExceptionHandler>();
        services.AddSingleton<IExceptionHandler, InvalidOperationExceptionHandler>();
        services.AddSingleton<IExceptionHandler, ArgumentExceptionHandler>();
        services.AddSingleton<IExceptionHandler, DbConcurrencyExceptionHandler>();
        services.AddSingleton<IExceptionHandler, DbUpdateExceptionHandler>();
        services.AddSingleton<IExceptionHandler, FallbackExceptionHandler>(); // must be last

        services.AddSingleton<ExceptionHandlerRegistry>();

        return services;
    }
}