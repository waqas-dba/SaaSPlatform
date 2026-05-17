using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Middleware;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;
using CoreKit.Tenant.Repositories;
using CoreKit.Tenant.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreKit.Tenant.Extensions;

public static class TenantKitServiceCollectionExtensions
{
    public static IServiceCollection AddTenantKit(
        this IServiceCollection services,
        string connectionString,
        Action<TenantKitOptions>? configure = null)
    {
        services.Configure<TenantKitOptions>(options =>
        {
            configure?.Invoke(options);
        });

        services.AddDbContext<TenantDbContext>(opt =>
        {
            opt.UseNpgsql(connectionString);
        });

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ITenantLegalInfoRepository, TenantLegalInfoRepository>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ITenantLegalInfoService, TenantLegalInfoService>();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp =>
            sp.GetRequiredService<TenantContext>());
        services.AddScoped<IMutableTenantContext>(sp =>
            sp.GetRequiredService<TenantContext>());

        // Required: IMiddleware implementations must be registered in DI
        // Without this line UseMiddleware<TenantResolutionMiddleware>() stalls silently
        services.AddScoped<TenantResolutionMiddleware>();

        return services;
    }
}