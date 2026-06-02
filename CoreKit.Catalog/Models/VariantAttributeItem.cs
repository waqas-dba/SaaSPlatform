// CoreKit.Catalog/Models/VariantAttributeItem.cs

namespace CoreKit.Catalog.Models;

public class VariantAttributeItem
{
    /// <summary>
    /// Preferred: resolve by ID. Falls back to Name+StoreTypeCode if null.
    /// </summary>
    public Guid? TemplateId { get; set; }   // ← new

    public string Name { get; set; } = default!;
    public string Value { get; set; } = default!;
}