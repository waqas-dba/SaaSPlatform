namespace CoreKit.Order.Models;

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; }

    /// <summary>IDs of the chosen add-ons. Names and prices are looked up on the server.</summary>
    public List<Guid>? AddonIds { get; set; }
}