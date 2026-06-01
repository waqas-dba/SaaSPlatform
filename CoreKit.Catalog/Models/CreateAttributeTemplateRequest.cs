// CoreKit.Catalog/Models/AttributeTemplate/CreateAttributeTemplateRequest.cs
using CoreKit.Catalog.Entities;

namespace CoreKit.Catalog.Models;

public class CreateAttributeTemplateRequest
{
    public string Name { get; set; } = default!;
    public string StoreTypeCode { get; set; } = default!;
    public AttributeFieldType FieldType { get; set; }
    public string? OptionsJson { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? GroupId { get; set; }
}