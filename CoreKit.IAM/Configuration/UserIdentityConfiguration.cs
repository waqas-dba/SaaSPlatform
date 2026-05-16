
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Configuration;

public class UserIdentityConfiguration
    : IEntityTypeConfiguration<UserIdentity>
{
    public void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        builder.ToTable("IAM_UserIdentities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EncryptedCnic)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<UserIdentity>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}