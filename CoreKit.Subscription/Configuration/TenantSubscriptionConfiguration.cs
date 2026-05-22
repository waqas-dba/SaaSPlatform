using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.Subscription.Entities;

namespace CoreKit.Subscription.Configuration;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("SUB_TenantSubscriptions");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasOne(x => x.Plan).WithMany(x => x.Subscriptions).HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}