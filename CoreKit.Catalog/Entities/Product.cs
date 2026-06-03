// CoreKit.Catalog/Entities/Product.cs
using CoreKit.SharedKernel.Common;

namespace CoreKit.Catalog.Entities;

public class Product : AuditableEntity, ITenantScoped, IStoreScoped
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public Guid StoreId { get; set; }
    public Guid TenantId { get; set; }
    public bool TrackInventory { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Addon group reference
    public Guid? AddonGroupId { get; set; }
    public AddonGroup? AddonGroup { get; set; }

    // Variant group reference — defines which variant dimensions
    // this product supports and what values are available
    public Guid? VariantGroupId { get; set; }
    public VariantGroup? VariantGroup { get; set; }

    public ICollection<ProductImage> Images { get; set; }
        = new List<ProductImage>();
    public ICollection<ProductAttributeValue> AttributeValues { get; set; }
        = new List<ProductAttributeValue>();
    public ICollection<ProductVariant> Variants { get; set; }
        = new List<ProductVariant>();
}