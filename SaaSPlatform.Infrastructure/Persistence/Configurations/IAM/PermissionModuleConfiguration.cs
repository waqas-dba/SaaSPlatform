using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.IAM.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.IAM;

public class PermissionModuleConfiguration : IEntityTypeConfiguration<PermissionModule>
{
    public void Configure(EntityTypeBuilder<PermissionModule> builder)
    {
        builder.HasIndex(x => x.Code).IsUnique();
    }
}