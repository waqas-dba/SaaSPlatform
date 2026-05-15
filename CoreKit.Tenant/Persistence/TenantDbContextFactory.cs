using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoreKit.Tenant.Persistence;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=saas_db;Username=postgres;Password=123");

        return new TenantDbContext(optionsBuilder.Options);
    }
}