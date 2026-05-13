using AuthCoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthCoreKit.IAM.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.FamilyId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.JwtId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.IsRevoked)
            .HasDefaultValue(false);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => x.UserId);

        // ❌ REMOVE THIS (causes your error):
        // builder.HasOne(x => x.User) ...
    }
}