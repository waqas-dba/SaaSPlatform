using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Tenancy;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Middleware;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;
using CoreKit.Tenant.Repositories;
using CoreKit.Tenant.Services;
using CoreKit.Tenant.Validators;
using FluentValidation;
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

        services.AddDbContext<TenantDbContext>((sp, opt) =>
        {
            opt.UseNpgsql(connectionString);
        });

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ITenantLegalInfoRepository, TenantLegalInfoRepository>();
        services.AddScoped<IStoreTypeRepository, StoreTypeRepository>();

        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ITenantLegalInfoService, TenantLegalInfoService>();
        services.AddScoped<IStoreTypeService, StoreTypeService>();

        // Rule 1: IStoreInfoProvider lives in SharedKernel; implementation lives in Tenant
        services.AddScoped<IStoreInfoProvider, TenantStoreInfoProvider>();

        // Single TenantContext instance shared by both interfaces
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<IMutableTenantContext>(sp => sp.GetRequiredService<TenantContext>());

        services.AddValidatorsFromAssemblyContaining < CreateStoreRequestValidator > ();

        return services;
    }
}