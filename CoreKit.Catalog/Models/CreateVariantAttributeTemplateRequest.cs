namespace CoreKit.Catalog.Models;

public class CreateVariantAttributeTemplateRequest
{
    public string Name { get; set; } = default!;
    public string StoreTypeCode { get; set; } = default!;
    public string? OptionsJson { get; set; }
}