using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Billing.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Billing;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => x.NextBillingDate);
    }
}