using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.TenantConfig;

public class StoreCuisineConfiguration : IEntityTypeConfiguration<StoreCuisine>
{
    public void Configure(EntityTypeBuilder<StoreCuisine> builder)
    {
        builder.HasKey(sc => new { sc.StoreId, sc.CuisineId });

        builder.HasOne(sc => sc.Store)
               .WithMany(s => s.StoreCuisines)
               .HasForeignKey(sc => sc.StoreId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sc => sc.Cuisine)
               .WithMany()
               .HasForeignKey(sc => sc.CuisineId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}