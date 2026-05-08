// SaaSPlatform.Infrastructure/Persistence/Configurations/Billing/PaymentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Billing.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Billing;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.StoreId });   // new index for store‑scoped queries

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        // Optional FK to Store (if StoreId is set)
        builder.HasOne<SaaSPlatform.Core.Tenant.Entities.Store>()
               .WithMany()
               .HasForeignKey(p => p.StoreId)
               .IsRequired(false);    // because StoreId is nullable
    }
}