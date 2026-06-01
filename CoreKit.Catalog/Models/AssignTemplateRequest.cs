// CoreKit.Catalog/Models/AttributeTemplate/AssignTemplateRequest.cs
namespace CoreKit.Catalog.Models;

/// <summary>
/// Tenant admin assigns a template (platform or custom) to a store.
/// StoreId = null  → applies to all stores of that tenant for the store type.
/// StoreId = set   → applies only to that store.
/// </summary>
public class AssignTemplateRequest
{
    public Guid TenantId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid TemplateId { get; set; }
    public string StoreTypeCode { get; set; } = default!;
}