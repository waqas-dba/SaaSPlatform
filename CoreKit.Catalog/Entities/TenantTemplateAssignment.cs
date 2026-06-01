// CoreKit.Catalog/Entities/TenantTemplateAssignment.cs
namespace CoreKit.Catalog.Entities;

/// <summary>
/// Records that a tenant has explicitly assigned a template (platform or their
/// own custom) to one of their stores.
/// When StoreId is null the assignment applies to all stores of that tenant
/// for the given store type (tenant-wide default).
/// When StoreId is set it applies only to that specific store.
/// </summary>
public class TenantTemplateAssignment
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>Null = tenant-wide default; set = store-specific.</summary>
    public Guid? StoreId { get; set; }

    public Guid TemplateId { get; set; }
    public ProductAttributeTemplate Template { get; set; } = default!;

    public string StoreTypeCode { get; set; } = default!;
}