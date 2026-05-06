using Microsoft.EntityFrameworkCore;

namespace SaaSPlatform.Infrastructure.Persistence;

public class SaaSPlatformDbContext : DbContext
{
    public SaaSPlatformDbContext(DbContextOptions<SaaSPlatformDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaaSPlatformDbContext).Assembly);
    }
}