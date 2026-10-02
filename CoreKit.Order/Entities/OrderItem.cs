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

    /// <summary>Product or variant price per unit, without add-ons.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Sum of the selected add-on prices, per unit.</summary>
    public decimal AddonsTotal { get; set; }

    public string? AddonsJson { get; set; }
}