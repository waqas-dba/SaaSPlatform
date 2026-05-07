using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Orders.Enums;
using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Orders.Entities;

public class Order : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid StoreId { get; set; }

    public Guid? TableId { get; set; }

    public long OrderNumber { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public OrderChannel Channel { get; set; } = OrderChannel.QRMenu;

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public ICollection<OrderItem>? Items { get; set; }
}