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

    /// <summary>Required for DineIn orders.</summary>
    public string? TableNumber { get; set; }

    /// <summary>Required for RoomService orders (hotels).</summary>
    public string? RoomNumber { get; set; }

    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public enum OrderType
{
    Collection = 1,
    Delivery = 2,
    DineIn = 3,
    RoomService = 4
}

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Preparing = 3,

    /// <summary>Ready for the customer to collect (Collection) or ready to serve (DineIn).</summary>
    ReadyForPickup = 4,

    /// <summary>On the way to the customer (Delivery) or to the room (RoomService).</summary>
    OutForDelivery = 5,

    Delivered = 6,
    Cancelled = 7
}