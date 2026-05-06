using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSPlatform.Core.Orders.Entities;

namespace SaaSPlatform.Infrastructure.Persistence.Configurations.Orders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt });
        builder.HasIndex(x => x.Status);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasIndex(x => x.OrderId);
    }
}

public class OrderItemAddonConfiguration : IEntityTypeConfiguration<OrderItemAddon>
{
    public void Configure(EntityTypeBuilder<OrderItemAddon> builder)
    {
        builder.HasIndex(x => x.OrderItemId);
    }
}