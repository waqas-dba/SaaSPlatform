using Microsoft.Extensions.DependencyInjection;
using TenantKit.Abstractions;
using TenantKit.Interfaces;
using TenantKit.Repositories;
using TenantKit.Services;
using TenantKit.Models;

namespace TenantKit.Extensions;

public static class TenantKitServiceCollectionExtensions
{
    public static IServiceCollection AddTenantKit<TDbContext>(
        this IServiceCollection services,
        Action<TenantKitOptions>? configureOptions = null)
        where TDbContext : class, ITenantKitDbContext
    {
        services.AddScoped<ITenantKitDbContext>(sp => sp.GetRequiredService<TDbContext>());
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