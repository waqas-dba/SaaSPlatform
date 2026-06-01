// CoreKit.Catalog/Models/AttributeTemplate/StoreAttributeOverrideRequest.cs
namespace CoreKit.Catalog.Models;

/// <summary>
/// Store manager toggles IsRequired / IsVisible for an attribute
/// on their specific store without touching the template definition.
/// </summary>
public class StoreAttributeOverrideRequest
{
    public Guid StoreId { get; set; }
    public Guid TemplateId { get; set; }
    public bool? IsRequired { get; set; }
    public bool? IsVisible { get; set; }
}