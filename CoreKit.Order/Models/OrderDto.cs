namespace CoreKit.Order.Models;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? DeliveryAddress { get; set; }
    public decimal? DeliveryFee { get; set; }
    public string? TableNumber { get; set; }
    public string? RoomNumber { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}