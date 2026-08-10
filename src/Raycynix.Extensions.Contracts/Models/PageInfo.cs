using System.ComponentModel.DataAnnotations;

namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents paging metadata returned with list contracts.
/// </summary>
public class PageInfo
{
    /// <summary>
    /// Gets or sets the 1-based current page number.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the current page size.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of items.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Determines whether the page info is structurally valid for transport.
    /// </summary>
    /// <returns><c>true</c> when the model is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        if (Page < 1 || PageSize < 1 || TotalCount < 0 || TotalPages < 0)
        {
            return false;
        }

        if (TotalPages == 0)
        {
            return TotalCount == 0 && !HasPreviousPage && !HasNextPage;
        }

        return Page <= TotalPages &&
               HasPreviousPage == (Page > 1) &&
               HasNextPage == (Page < TotalPages);
    }
}
