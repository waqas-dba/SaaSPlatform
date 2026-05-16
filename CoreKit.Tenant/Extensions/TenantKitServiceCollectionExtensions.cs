// CoreKit.Tenant/Extensions/TenantKitServiceCollectionExtensions.cs
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;
using CoreKit.Tenant.Persistence.Seeders;
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

        // FIX: Register TenantContext as the concrete type and expose it via
        // both interfaces so middleware resolves IMutableTenantContext and
        // controllers resolve ITenantContext — both point to the same instance.
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<IMutableTenantContext>(sp => sp.GetRequiredService<TenantContext>());

        return services;
    }
}