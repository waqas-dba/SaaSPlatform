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

        // Status stored as int for DB clarity and query performance
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.StartDate).IsRequired();

        builder.Property(x => x.PaymentReference)
            .HasMaxLength(500);

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Subscriptions)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent accidental plan deletion with active subs

        // BUG FIX: the original had HasQueryFilter(x => !x.IsDeleted) but
        // TenantSubscription inherits AuditableEntity (via AuditableDbContext base),
        // which already registers a global soft-delete filter through
        // AuditableDbContext.OnModelCreating. Defining it again here causes EF to
        // apply two conflicting query filters on the same entity, which throws at
        // model-building time in EF Core 7+. Remove the manual filter entirely and
        // let AuditableDbContext handle it.
        //
        // If you need to suppress the soft-delete filter in specific queries use:
        //   .IgnoreQueryFilters()
    }
}