// FILE: CoreKit.SharedKernel/Models/PagedQuery.cs  (NEW FILE)
// Shared query-string parameters for paginated list endpoints.

using System.ComponentModel.DataAnnotations;

namespace CoreKit.SharedKernel.Models;

/// <summary>
/// Bind this from [FromQuery] on any list endpoint to get page + page-size.
/// </summary>
public sealed class PagedQuery
{
    private int _page = 1;
    private int _pageSize = 20;

    /// <summary>1-based page number. Defaults to 1.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be ≥ 1.")]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Items per page. Defaults to 20, max 100.</summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 1,
            > 100 => 100,
            _ => value
        };
    }

    public int Skip => (Page - 1) * PageSize;
}