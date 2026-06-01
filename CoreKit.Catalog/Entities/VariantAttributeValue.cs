namespace CoreKit.Catalog.Entities;

public class VariantAttributeValue
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public ProductVariant Variant { get; set; } = default!;
    public Guid TemplateId { get; set; }
    public VariantAttributeTemplate Template { get; set; } = default!;
    public string Value { get; set; } = default!;
}