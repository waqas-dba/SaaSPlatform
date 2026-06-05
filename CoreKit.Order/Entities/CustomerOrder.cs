// CoreKit.Order/Entities/CustomerOrder.cs
using CoreKit.SharedKernel.Common;

namespace CoreKit.Order.Entities;

public class CustomerOrder : AuditableEntity, ITenantScoped
{
    public string OrderNumber { get; set; } = default!;
    public Guid StoreId { get; set; }
    public Guid TenantId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public string? DeliveryAddress { get; set; }
    public decimal? DeliveryFee { get; set; }

    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public enum OrderType
{
    Collection = 1,
    Delivery = 2
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Preparing = 3,
    ReadyForPickup = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Cancelled = 7
}