namespace CoreKit.Catalog.Entities;

public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<VariantAttributeValue> AttributeValues { get; set; } = new List<VariantAttributeValue>();
}