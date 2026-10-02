namespace CoreKit.Catalog.Entities;

/// <summary>
/// A simple sellable option of a product, for example Small / Medium / Large,
/// each with its own full price.
/// </summary>
public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}