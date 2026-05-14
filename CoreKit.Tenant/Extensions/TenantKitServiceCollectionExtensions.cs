using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreKit.Tenant.Abstractions;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Repositories;
using CoreKit.Tenant.Services;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;

namespace CoreKit.Tenant.Extensions;

public static class TenantKitServiceCollectionExtensions
{
    public static IServiceCollection AddCoreKitTenant(
        this IServiceCollection services,
        string connectionString,
        Action<TenantKitOptions>? configureOptions = null)
    {
        services.AddDbContext<TenantDbContext>(options =>
            options.UseNpgsql(connectionString));

        var options = new TenantKitOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IStoreService, StoreService>();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        if (options.EnableLegalInfo)
        {
            services.AddScoped<ITenantLegalInfoRepository, TenantLegalInfoRepository>();
            services.AddScoped<ITenantLegalInfoService, TenantLegalInfoService>();
        }

        return services;
    }
}