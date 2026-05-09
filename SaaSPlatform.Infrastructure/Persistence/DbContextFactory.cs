using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Services;
using SaaSPlatform.Infrastructure.Services.TenantServices;

public class DbContextFactory : IDesignTimeDbContextFactory<SaaSPlatformDbContext>
{
    public SaaSPlatformDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SaaSPlatformDbContext>()
            .UseNpgsql("Host=localhost;Database=saas_db;Username=postgres;Password=123")
            .Options;

        return new SaaSPlatformDbContext(
            options,
            new NullTenantContext(), // ✅ FIX
            null);
    }
}