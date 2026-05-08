using Microsoft.EntityFrameworkCore;
using SaaSPlatform.SharedKernel.Common;
using SaaSPlatform.Core.Tenant.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence.Extensions;

public static class QueryFilterExtensions
{
    public static void ApplyTenantFilters(
        this ModelBuilder modelBuilder,
        ITenantContext tenantContext)
    {
        var entityTypes =
            modelBuilder.Model.GetEntityTypes()
                .Where(x =>
                    typeof(ITenantScoped)
                        .IsAssignableFrom(x.ClrType));

        foreach (var entityType in entityTypes)
        {
            var method =
                typeof(QueryFilterExtensions)
                    .GetMethod(
                        nameof(SetTenantFilter),
                        System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.NonPublic)!
                    .MakeGenericMethod(entityType.ClrType);

            method.Invoke(
                null,
                new object[]
                {
                    modelBuilder,
                    tenantContext
                });
        }
    }

    private static void SetTenantFilter<TEntity>(
        ModelBuilder builder,
        ITenantContext tenantContext)
        where TEntity : class, ITenantScoped
    {
        builder.Entity<TEntity>()
            .HasQueryFilter(
                e => e.TenantId ==
                     tenantContext.TenantId);
    }
}