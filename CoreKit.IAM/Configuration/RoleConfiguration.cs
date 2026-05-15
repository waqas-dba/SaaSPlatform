using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Configuration;

/// <summary>
/// EF Core configuration for the Role entity.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("IAM_Roles");
        builder.HasKey(x => x.Id);

        // Unique role name per tenant (NULL tenant treated as distinct)
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}