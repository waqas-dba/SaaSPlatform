// SubscriptionDbContextFactory.cs

using CoreKit.Subscription.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoreKit.Subscription;

public class SubscriptionDbContextFactory
    : IDesignTimeDbContextFactory<SubscriptionDbContext>
{
    public SubscriptionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<SubscriptionDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=127.0.0.1;Port=5432;Database=saas_db;Username=postgres;Password=123;");

        return new SubscriptionDbContext(optionsBuilder.Options);
    }
}