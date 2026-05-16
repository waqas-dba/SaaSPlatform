using CoreKit.IAM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreKit.IAM.Configuration;

public class UserStoreAssignmentConfiguration : IEntityTypeConfiguration<UserStoreAssignment>
{
    public void Configure(EntityTypeBuilder<UserStoreAssignment> builder)
    {
        builder.ToTable("IAM_UserStoreAssignments");
        builder.HasKey(x => new { x.UserId, x.StoreId });
        builder.HasIndex(x => new { x.TenantId, x.StoreId });
        builder.HasOne(x => x.User)
               .WithMany(u => u.StoreAssignments)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.User.IsDeleted);
    }
}