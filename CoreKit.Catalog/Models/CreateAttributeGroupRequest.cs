// CoreKit.Catalog/Models/AttributeTemplate/CreateAttributeGroupRequest.cs
namespace CoreKit.Catalog.Models;

public class CreateAttributeGroupRequest
{
    public string Name { get; set; } = default!;
    public string StoreTypeCode { get; set; } = default!;
    public int SortOrder { get; set; }

    /// <summary>
    /// Null = platform group (requires catalog.templates.manage.platform).
    /// Set  = tenant group   (requires catalog.templates.manage.tenant).
    /// </summary>
    public Guid? TenantId { get; set; }
}