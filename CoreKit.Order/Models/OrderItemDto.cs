namespace CoreKit.Order.Models;

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal AddonsTotal { get; set; }

    /// <summary>Quantity x (UnitPrice + AddonsTotal).</summary>
    public decimal LineTotal { get; set; }

    public List<AddonSnapshotDto>? Addons { get; set; }
}