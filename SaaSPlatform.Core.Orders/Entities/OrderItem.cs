using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Orders.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? ProductVariantId { get; set; }

    public string ProductNameSnapshot { get; set; } = default!;

    public string? VariantNameSnapshot { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }

    public ICollection<OrderItemAddon>? Addons { get; set; }
}