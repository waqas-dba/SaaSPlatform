using CoreKit.SharedKernel.Interfaces;
using CoreKit.Subscription.Interfaces;
using CoreKit.Subscription.Persistence;
using CoreKit.Subscription.Repositories;
using CoreKit.Subscription.Services;
using CoreKit.Tenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreKit.Subscription.Extensions;

public static class ServiceCollectionExtensions
{
    // CoreKit.Subscription/Extensions/ServiceCollectionExtensions.cs
    public static IServiceCollection AddSubscriptionModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<SubscriptionDbContext>((sp, options) =>
            options.UseNpgsql(connectionString));

        // Ensure memory cache is available (safe to call multiple times)
        services.AddMemoryCache();

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<IPlanLimitProvider, PlanLimitProvider>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();

        return services;
    }
}