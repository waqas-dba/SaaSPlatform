using System.ComponentModel.DataAnnotations;

namespace CoreKit.Catalog.Models;

public class CreateVariantRequest
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateVariantRequest
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public Guid CategoryId { get; set; }

    public bool IsVegetarian { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public int SortOrder { get; set; }

    public bool AvailableForDineIn { get; set; } = true;
    public bool AvailableForCollection { get; set; } = true;
    public bool AvailableForDelivery { get; set; } = true;

    public List<CreateVariantRequest> Variants { get; set; } = new();
    public List<Guid> AddonGroupIds { get; set; } = new();
    public List<CreateImageItem> Images { get; set; } = new();

    /// <summary>Stores that should sell this product immediately. Optional.</summary>
    public List<Guid> StoreIds { get; set; } = new();
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVegetarian { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public int? SortOrder { get; set; }

    public bool? AvailableForDineIn { get; set; }
    public bool? AvailableForCollection { get; set; }
    public bool? AvailableForDelivery { get; set; }

    /// <summary>When not null, replaces the product's modifier groups (in this order).</summary>
    public List<Guid>? AddonGroupIds { get; set; }

    /// <summary>
    /// When not null, replaces the product's variants, matched by name:
    /// existing names are updated, new names are added, missing names are removed.
    /// </summary>
    public List<CreateVariantRequest>? Variants { get; set; }
}

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; }
    public bool IsVegetarian { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public int SortOrder { get; set; }

    public bool AvailableForDineIn { get; set; }
    public bool AvailableForCollection { get; set; }
    public bool AvailableForDelivery { get; set; }

    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
    public List<AddonGroupDto> AddonGroups { get; set; } = new();

    /// <summary>Stores currently selling this product.</summary>
    public List<Guid> StoreIds { get; set; } = new();
}

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? PrimaryImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsVegetarian { get; set; }
}

public class ProductFilterQuery
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MinPrice must be >= 0.")]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaxPrice must be >= 0.")]
    public decimal? MaxPrice { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>Only used for store menus: filters on the store's availability flag.</summary>
    public bool? IsAvailable { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be >= 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 20;

    public int Skip => (Page - 1) * PageSize;
}