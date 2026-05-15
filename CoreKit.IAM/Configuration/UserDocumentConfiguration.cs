using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreKit.IAM.Entities;

namespace CoreKit.IAM.Configuration;

public class UserDocumentConfiguration : IEntityTypeConfiguration<UserDocument>
{
    public void Configure(EntityTypeBuilder<UserDocument> builder)
    {
        builder.ToTable("IAM_UserDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(2000);
        builder.HasIndex(x => new { x.UserId, x.DocumentType }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Match User's soft‑delete filter
        builder.HasQueryFilter(ud => !ud.User.IsDeleted);
    }
}