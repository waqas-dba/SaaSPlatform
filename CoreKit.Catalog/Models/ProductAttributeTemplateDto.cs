// CoreKit.Catalog/Models/AttributeTemplate/ProductAttributeTemplateDto.cs
using CoreKit.Catalog.Entities;

namespace CoreKit.Catalog.Models;

public class ProductAttributeTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string StoreTypeCode { get; set; } = default!;
    public AttributeFieldType FieldType { get; set; }
    public string? OptionsJson { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisible { get; set; }
    public int SortOrder { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupName { get; set; }
    public Guid? OverridesTemplateId { get; set; }

    /// <summary>
    /// Indicates the source of this attribute as seen by the current tenant:
    /// "platform"  → unmodified base template
    /// "override"  → tenant has a scoped override of a platform template
    /// "custom"    → tenant-created, no platform equivalent
    /// </summary>
    public string Source { get; set; } = "platform";
}