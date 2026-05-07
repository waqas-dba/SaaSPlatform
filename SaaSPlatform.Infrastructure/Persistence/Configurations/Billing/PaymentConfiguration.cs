using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Billing.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Billing;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasIndex(x => new
        {
            x.TenantId,
            x.Status
        });

        builder.HasIndex(x => x.TransactionId);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);
    }
}