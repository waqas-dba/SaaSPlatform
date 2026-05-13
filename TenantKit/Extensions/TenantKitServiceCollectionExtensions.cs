using Microsoft.Extensions.DependencyInjection;
using TenantKit.Abstractions;
using TenantKit.Infrastructure;

namespace TenantKit.Extensions;

public static class TenantKitServiceCollectionExtensions
{
    public static IServiceCollection AddTenantKit(
        this IServiceCollection services)
    {
        services.AddScoped<TenantContext>();

        services.AddScoped<ITenantContext>(sp =>
            sp.GetRequiredService<TenantContext>());

        return services;
    }
}