using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.Tenant.Entities;

namespace CoreKit.Tenant.Configuration;

public class TenantLegalInfoConfiguration : IEntityTypeConfiguration<TenantLegalInfo>
{
    public void Configure(EntityTypeBuilder<TenantLegalInfo> builder)
    {
        builder.ToTable("TNT_TenantLegalInfos");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId).IsUnique();
        builder.HasOne(x => x.Tenant).WithOne().HasForeignKey<TenantLegalInfo>(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
    }
}