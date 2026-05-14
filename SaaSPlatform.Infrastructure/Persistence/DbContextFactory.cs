using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TenantKit.Abstractions;

namespace SaaSPlatform.Infrastructure.Persistence;

public class DbContextFactory : IDesignTimeDbContextFactory<SaaSPlatformDbContext>
{
    public SaaSPlatformDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SaaSPlatformDbContext>()
            .UseNpgsql("Host=localhost;Database=saas_db;Username=postgres;Password=123")
            .Options;

        // Design‑time dummy tenant context (no tenant)
        var dummyTenantContext = new DesignTimeTenantContext();

        return new SaaSPlatformDbContext(options, dummyTenantContext, null);
    }

    private class DesignTimeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
    }
}