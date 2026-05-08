using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Orders.Entities;

public class OrderItemAddon : BaseEntity
{
    public Guid OrderItemId { get; set; }

    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}