using SaaSPlatform.Core.SharedKernel.Common;
using SaaSPlatform.Core.SharedKernel.Enums;

namespace SaaSPlatform.Core.Orders.Entities;

public class Order : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem>? Items { get; set; }
}