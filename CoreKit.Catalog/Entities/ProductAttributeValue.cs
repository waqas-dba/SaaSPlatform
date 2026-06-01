namespace CoreKit.Catalog.Entities;

public class ProductAttributeValue
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid TemplateId { get; set; }
    public AttributeTemplate Template { get; set; } = default!;
    public string Value { get; set; } = default!;
}