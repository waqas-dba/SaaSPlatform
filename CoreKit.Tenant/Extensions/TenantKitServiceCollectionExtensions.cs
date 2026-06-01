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

        // Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ITenantLegalInfoRepository, TenantLegalInfoRepository>();
        services.AddScoped<IStoreTypeRepository, StoreTypeRepository>();

        // Services
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ITenantLegalInfoService, TenantLegalInfoService>();
        services.AddScoped<IStoreTypeService, StoreTypeService>();

        services.AddScoped<IMutableTenantContext, TenantContext>();
        services.AddScoped<ITenantContext>(sp =>
            sp.GetRequiredService<IMutableTenantContext>());

        // TenantContext is the single concrete that satisfies both ITenantContext
        // and IMutableTenantContext. Register the concrete first, then resolve
        // the interfaces from it so all callers share the same scoped instance.
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // BUG FIX: IMutableTenantContext was previously registered to resolve
        // TenantContext via CoreKit.Tenant.Services.IMutableTenantContext, which is
        // correct, but the using alias for ITenantContext was ambiguous between
        // CoreKit.Tenant.Services.ITenantContext and the one re-exported in Abstractions.
        // Using the fully-qualified concrete avoids any namespace ambiguity.
        services.AddScoped<IMutableTenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // BUG FIX: TenantResolutionMiddleware is registered as IMiddleware which means
        // ASP.NET Core's middleware factory resolves it from DI per-request.
        // Registering it as AddScoped here is correct and sufficient.
        // Do NOT also call services.AddTransient<TenantResolutionMiddleware>() elsewhere —
        // that would create a second, separate instance that bypasses the scoped lifetime.
        services.AddScoped<TenantResolutionMiddleware>();

        services.AddValidatorsFromAssemblyContaining<CreateStoreRequestValidator>();

        return services;
    }
}