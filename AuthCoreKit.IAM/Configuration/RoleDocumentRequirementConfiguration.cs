using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AuthCoreKit.IAM.Entities;

namespace AuthCoreKit.IAM.Configuration;

public class RoleDocumentRequirementConfiguration : IEntityTypeConfiguration<RoleDocumentRequirement>
{
    public void Configure(EntityTypeBuilder<RoleDocumentRequirement> builder)
    {
        builder.ToTable("IAM_RoleDocumentRequirements");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.RoleId, x.DocumentType }).IsUnique();
        builder.Property(x => x.DocumentType).IsRequired().HasMaxLength(50);
        builder.HasOne(x => x.Role)
               .WithMany()
               .HasForeignKey(x => x.RoleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}