using CoreKit.SharedKernel.Common;
using NpgsqlTypes;

namespace CoreKit.Catalog.Entities;

/// <summary>
/// A menu item owned by the tenant. Which stores sell it is decided by
/// <see cref="StoreProducts"/>.
/// </summary>
public class Product : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsVegetarian { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public int SortOrder { get; set; }

    // Order-type availability. Hotel room service uses AvailableForDineIn
    // (the room is treated as the place the guest dines in).
    public bool AvailableForDineIn { get; set; } = true;
    public bool AvailableForCollection { get; set; } = true;
    public bool AvailableForDelivery { get; set; } = true;

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductAddonGroup> AddonGroupLinks { get; set; } = new List<ProductAddonGroup>();
    public ICollection<StoreProduct> StoreProducts { get; set; } = new List<StoreProduct>();

    public NpgsqlTsVector SearchVector { get; set; } = null!;
}