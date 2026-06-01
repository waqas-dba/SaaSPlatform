// CoreKit.Catalog/Entities/ProductAttributeTemplate.cs
namespace CoreKit.Catalog.Entities;

/// <summary>
/// Defines a single attribute field that can appear on a product of a given
/// store type (e.g. "Spice Level" for restaurants, "Size" for clothing).
///
/// Scope rules
/// ───────────
/// TenantId = null  → platform base template  (read-only for tenants)
/// TenantId = set   → tenant-scoped template  (override or custom addition)
///
/// Override rules
/// ──────────────
/// OverridesTemplateId = null  → original attribute (platform or tenant custom)
/// OverridesTemplateId = set   → this row is a tenant-scoped override of a
///                               platform attribute; the platform row is
///                               untouched and still used for all other tenants.
/// </summary>
public class ProductAttributeTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    /// <summary>
    /// The store type this attribute belongs to, e.g. "restaurant", "clothing".
    /// Matches StoreType.Code from CoreKit.Tenant.
    /// </summary>
    public string StoreTypeCode { get; set; } = default!;

    public AttributeFieldType FieldType { get; set; }

    /// <summary>
    /// JSON array of allowed values for Select fields, null for others.
    /// e.g. ["Small","Medium","Large"]
    /// </summary>
    public string? OptionsJson { get; set; }

    /// <summary>Platform default: is this attribute required?</summary>
    public bool IsRequired { get; set; }

    /// <summary>Platform default: is this attribute visible?</summary>
    public bool IsVisible { get; set; } = true;

    public int SortOrder { get; set; }

    // ── Scope ──────────────────────────────────────────────────────────────
    /// <summary>
    /// Null = platform base template visible to all tenants.
    /// Set  = tenant-scoped (custom addition or override).
    /// </summary>
    public Guid? TenantId { get; set; }

    // ── Override link ──────────────────────────────────────────────────────
    /// <summary>
    /// When a tenant overrides a platform attribute, we create a new row with
    /// TenantId set and OverridesTemplateId pointing to the platform row.
    /// The platform row is never modified.
    /// </summary>
    public Guid? OverridesTemplateId { get; set; }
    public ProductAttributeTemplate? OverridesTemplate { get; set; }

    // ── Group ──────────────────────────────────────────────────────────────
    public Guid? GroupId { get; set; }
    public ProductAttributeGroup? Group { get; set; }

    // ── Store-level toggle (applied via StoreAttributeOverride) ────────────
    public ICollection<StoreAttributeOverride> StoreOverrides { get; set; }
        = new List<StoreAttributeOverride>();

    public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; }
        = new List<ProductAttributeValue>();
}

public enum AttributeFieldType
{
    Text = 1,
    Number = 2,
    Boolean = 3,
    Select = 4
}