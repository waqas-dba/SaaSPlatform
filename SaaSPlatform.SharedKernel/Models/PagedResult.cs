// FILE: CoreKit.SharedKernel/Models/PagedResult.cs  (NEW FILE)
// Lightweight pagination envelope used across all list endpoints.

namespace CoreKit.SharedKernel.Models;

/// <summary>
/// Cursor-free, page-number-based pagination result.
/// Keep it simple: offset pagination is fine for admin/tenant volumes.
/// </summary>
public sealed class PagedResult<T>
{
    /// <summary>The items on this page.</summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>Total number of items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Current 1-based page number.</summary>
    public int Page { get; init; }

    /// <summary>Maximum items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> From(
        IReadOnlyList<T> items,
        int totalCount,
        int page,
        int pageSize) =>
        new()
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
}