// CoreKit.Order/Entities/OrderItem.cs
namespace CoreKit.Order.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public CustomerOrder Order { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? AddonsJson { get; set; }
}