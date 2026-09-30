// CoreKit.Catalog/Models/ProductFilterQuery.cs
using System.ComponentModel.DataAnnotations;

namespace CoreKit.Catalog.Models;

public class ProductFilterQuery
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MinPrice must be >= 0.")]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaxPrice must be >= 0.")]
    public decimal? MaxPrice { get; set; }

    public bool? IsActive { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be >= 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 20;

    public int Skip => (Page - 1) * PageSize;

    /// <summary>Only used for store menus: filters on the store's availability flag.</summary>
    public bool? IsAvailable { get; set; }

}