using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Configuration;

public class PermissionModuleConfiguration : IEntityTypeConfiguration<PermissionModule>
{
    public void Configure(EntityTypeBuilder<PermissionModule> builder)
    {
        builder.ToTable("IAM_PermissionModules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
}