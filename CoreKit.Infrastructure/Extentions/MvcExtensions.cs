using CoreKit.IAM.Validators;
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

        return services;
    }
}