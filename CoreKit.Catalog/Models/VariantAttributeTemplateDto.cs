namespace CoreKit.Catalog.Models;

public class VariantAttributeTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string StoreTypeCode { get; set; } = default!;
    public string? OptionsJson { get; set; }
}