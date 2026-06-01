// CoreKit.Catalog/Models/AttributeTemplate/ResolvedAttributeDto.cs
using CoreKit.Catalog.Entities;

namespace CoreKit.Catalog.Models;

/// <summary>
/// The fully-resolved view of an attribute for a specific store,
/// with all three precedence layers applied:
///   1. Platform base template
///   2. Tenant-scoped override / custom attribute
///   3. Store-level toggle
/// This is what the product creation form should render.
/// </summary>
public class ResolvedAttributeDto
{
    public Guid TemplateId { get; set; }
    public string Name { get; set; } = default!;
    public string StoreTypeCode { get; set; } = default!;
    public AttributeFieldType FieldType { get; set; }
    public string? OptionsJson { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisible { get; set; }
    public int SortOrder { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupName { get; set; }

    /// <summary>
    /// "platform" | "tenant-override" | "tenant-custom"
    /// </summary>
    public string Source { get; set; } = default!;
}