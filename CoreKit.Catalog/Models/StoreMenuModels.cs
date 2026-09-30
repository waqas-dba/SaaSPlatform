namespace CoreKit.Catalog.Models;

public class StoreMenuItemDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? PrimaryImageUrl { get; set; }
    public bool IsVegetarian { get; set; }

    public decimal BasePrice { get; set; }
    public decimal? PriceOverride { get; set; }

    /// <summary>PriceOverride if set, otherwise BasePrice.</summary>
    public decimal EffectivePrice { get; set; }

    /// <summary>The product itself is active (menu-level switch).</summary>
    public bool IsActive { get; set; }

    /// <summary>This store currently offers it (store-level switch, e.g. sold out).</summary>
    public bool IsAvailable { get; set; }

    public int SortOrder { get; set; }
}

public class AssignProductsRequest
{
    public List<Guid> ProductIds { get; set; } = new();
}

public class UpdateStoreProductRequest
{
    public bool? IsAvailable { get; set; }
    public decimal? PriceOverride { get; set; }

    /// <summary>Set true to remove the store price and use the base price again.</summary>
    public bool ClearPriceOverride { get; set; }

    public int? SortOrder { get; set; }
}

public class MenuTransferRequest
{
    public Guid SourceStoreId { get; set; }

    /// <summary>When true, the target store's current menu is cleared first.</summary>
    public bool ReplaceExisting { get; set; }
}

public class MenuTransferResultDto
{
    public int Copied { get; set; }
    public int Skipped { get; set; }
}