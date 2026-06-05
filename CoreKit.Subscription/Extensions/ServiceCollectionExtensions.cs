// CoreKit.Subscription/Extensions/ServiceCollectionExtensions.cs
using CoreKit.SharedKernel.Interfaces;
using CoreKit.Subscription.Interfaces;
using CoreKit.Subscription.Persistence;
using CoreKit.Subscription.Repositories;
using CoreKit.Subscription.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreKit.Subscription.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSubscriptionModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<SubscriptionDbContext>((sp, options) =>
            options.UseNpgsql(connectionString));

        services.AddMemoryCache();

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();

        // OCP: register feature evaluators — add new plan features here only
        services.AddSingleton<IFeatureEvaluator, CustomDomainEvaluator>();
        services.AddSingleton<IFeatureEvaluator, ThemeCustomizationEvaluator>();

        services.AddScoped<IPlanLimitProvider, PlanLimitProvider>();
        services.AddHostedService<SubscriptionExpiryService>();

        return services;
    }
}