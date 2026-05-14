using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantKit.Entities;

namespace TenantKit.Configuration;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("TNT_Stores");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique();
        builder.HasIndex(x => x.TenantId);
        builder.Property(x => x.Latitude).HasPrecision(9, 6);
        builder.Property(x => x.Longitude).HasPrecision(9, 6);
        builder.HasOne(x => x.Tenant).WithMany(x => x.Stores).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}