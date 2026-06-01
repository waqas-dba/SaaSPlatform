namespace CoreKit.Catalog.Entities;

public class VariantAttributeTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? OptionsJson { get; set; }
    public string StoreTypeCode { get; set; } = default!;
    public Guid? TenantId { get; set; }
}