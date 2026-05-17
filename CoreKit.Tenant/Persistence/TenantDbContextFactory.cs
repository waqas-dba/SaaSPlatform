using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoreKit.Tenant.Persistence;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=127.0.0.1;Port=5432;Database=saas_db;Username=postgres;Password=123;Timeout=10;CommandTimeout=10;SSL Mode=Disable");

        return new TenantDbContext(optionsBuilder.Options);
    }
}