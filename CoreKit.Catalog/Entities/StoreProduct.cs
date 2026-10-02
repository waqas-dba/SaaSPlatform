using CoreKit.SharedKernel.Common;

namespace CoreKit.Catalog.Entities;

/// <summary>
/// Says "this store sells this product". Moving or copying a menu between
/// stores of the same tenant means creating or moving these rows.
/// StoreId has no foreign key because stores live in the Tenant module.
/// </summary>
public class StoreProduct : BaseEntity, ITenantScoped, IStoreScoped
{
    public Guid TenantId { get; set; }
    public Guid StoreId { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    /// <summary>False hides the item at this store, for example when sold out.</summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>When set, replaces the product price (and variant prices are unchanged) at this store.</summary>
    public decimal? PriceOverride { get; set; }

    public int SortOrder { get; set; }
}