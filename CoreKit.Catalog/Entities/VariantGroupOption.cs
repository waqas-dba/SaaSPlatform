// CoreKit.Catalog/Entities/VariantGroupOption.cs
namespace CoreKit.Catalog.Entities;

/// <summary>
/// One variant dimension inside a group.
/// e.g.  Template = "Size",  AllowedValuesJson = ["S","M","L","XL"]
///       Template = "Color", AllowedValuesJson = ["Red","Blue"]
/// AllowedValuesJson narrows the template's full option list to only
/// what this merchant actually sells — can be null to allow all values.
/// </summary>
public class VariantGroupOption
{
    public Guid Id { get; set; }
    public Guid VariantGroupId { get; set; }
    public VariantGroup VariantGroup { get; set; } = default!;

    public Guid TemplateId { get; set; }
    public VariantAttributeTemplate Template { get; set; } = default!;

    /// <summary>JSON array of allowed values, subset of Template.OptionsJson.
    /// Null means all values from the template are allowed.</summary>
    public string? AllowedValuesJson { get; set; }

    public int SortOrder { get; set; }
}