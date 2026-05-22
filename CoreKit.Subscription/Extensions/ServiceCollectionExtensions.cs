using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreKit.Tenant.Abstractions;
using CoreKit.Subscription.Interfaces;
using CoreKit.Subscription.Persistence;
using CoreKit.Subscription.Repositories;
using CoreKit.Subscription.Services;

namespace CoreKit.Subscription.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSubscriptionModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SubscriptionDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<IPlanLimitProvider, PlanLimitProvider>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();

        return services;
    }
}