// SaaSPlatform.Infrastructure/Persistence/Configurations/TenantConfig/TenantLegalInfoConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Tenant.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.TenantConfig;

public class TenantLegalInfoConfiguration : IEntityTypeConfiguration<TenantLegalInfo>
{
    public void Configure(EntityTypeBuilder<TenantLegalInfo> builder)
    {
        builder.HasIndex(x => x.TenantId).IsUnique();
    }
}