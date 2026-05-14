using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("IAM_Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(256);

        builder.Property(x => x.Phone)
            .HasMaxLength(30);

        // ✅ SAFE UNIQUE INDEX (NO FILTER)
        builder.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.Phone })
            .IsUnique();
    }
}