using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.IAM;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.Name });
    }
}