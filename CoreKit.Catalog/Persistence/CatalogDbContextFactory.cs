using CoreKit.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoreKit.Catalog.Persistence;

public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=127.0.0.1;Port=5432;Database=saas_db;Username=postgres;Password=123;");

        // Design-time: no HTTP context or tenant resolution available.
        // Passing null tenant ID makes query filters neutral (TenantId == null || ... evaluates true).
        return new CatalogDbContext(optionsBuilder.Options, new DesignTimeTenantContext());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
    }
}