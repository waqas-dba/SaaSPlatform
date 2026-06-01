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
    public static IServiceCollection AddSubscriptionModule(
        this IServiceCollection services,
        string connectionString)
    {
        // BUG FIX: AuditableDbContext now requires ICurrentUser to stamp CreatedBy/UpdatedBy.
        // Use AddDbContext overload that resolves ICurrentUser from the DI scope so audit
        // fields are populated with the authenticated user's ID on every SaveChanges call.
        services.AddDbContext<SubscriptionDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<IPlanLimitProvider, PlanLimitProvider>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();

        return services;
    }
}