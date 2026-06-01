using Microsoft.EntityFrameworkCore;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Configuration;

namespace CoreKit.Subscription.Persistence;

// BUG FIX: was inheriting plain DbContext — Plan and TenantSubscription both extend
// AuditableEntity which expects CreatedAt/UpdatedAt/CreatedBy/UpdatedBy to be stamped
// automatically. Without AuditableDbContext those fields are never set.
public class SubscriptionDbContext : AuditableDbContext
{
    public SubscriptionDbContext(
        DbContextOptions<SubscriptionDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // AuditableDbContext.OnModelCreating registers soft-delete query filters
        // and the xmin row-version column — must call base first.
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PlanConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSubscriptionConfiguration());
    }
}