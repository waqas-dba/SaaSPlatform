using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Orders.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Orders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasIndex(x => new
        {
            x.TenantId,
            x.CreatedAt
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.OrderNumber
        }).IsUnique();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.Status
        });

        builder.HasIndex(x => x.StoreId);

        builder.Property(x => x.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(x => x.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.OrderType).IsRequired();
        builder.Property(x => x.DeliveryAddress).HasMaxLength(500);
    }
}