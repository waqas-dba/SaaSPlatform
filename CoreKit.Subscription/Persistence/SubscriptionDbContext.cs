using Microsoft.EntityFrameworkCore;
using CoreKit.Subscription.Entities;
using CoreKit.Subscription.Configuration;

namespace CoreKit.Subscription.Persistence;

public class SubscriptionDbContext : DbContext
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options) : base(options) { }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PlanConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSubscriptionConfiguration());
    }
}