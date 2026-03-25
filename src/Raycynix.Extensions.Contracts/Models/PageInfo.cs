namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents paging metadata returned with list contracts.
/// </summary>
public class PageInfo
{
    /// <summary>
    /// Gets or sets the 1-based current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the current page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of items.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage { get; set; }
}
