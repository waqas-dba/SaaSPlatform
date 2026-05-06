using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Orders.Entities;

public class Order : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;

    public string Status { get; set; } = "Pending";

    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem>? Items { get; set; }
}