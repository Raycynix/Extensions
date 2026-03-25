namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents paging input for list contracts.
/// </summary>
public class PagingRequest
{
    /// <summary>
    /// Gets or sets the 1-based page number.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the requested page size.
    /// </summary>
    public int PageSize { get; set; } = 20;
}
