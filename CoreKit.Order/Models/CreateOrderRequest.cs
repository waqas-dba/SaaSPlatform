namespace CoreKit.Order.Models;

public class CreateOrderRequest
{
    public Guid StoreId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;

    /// <summary>Collection, Delivery, DineIn or RoomService.</summary>
    public string OrderType { get; set; } = default!;

    public string? DeliveryAddress { get; set; }
    public decimal? DeliveryFee { get; set; }
    public string? TableNumber { get; set; }
    public string? RoomNumber { get; set; }

    public List<OrderItemRequest> Items { get; set; } = new();
    public string? Notes { get; set; }
}