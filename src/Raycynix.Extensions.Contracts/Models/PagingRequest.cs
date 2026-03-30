using System.ComponentModel.DataAnnotations;

namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents paging input for list contracts.
/// </summary>
public class PagingRequest
{
    /// <summary>
    /// Gets or sets the 1-based page number.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the requested page size.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Determines whether the paging request is valid for transport.
    /// </summary>
    /// <returns><c>true</c> when the model is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        return Page >= 1 && PageSize >= 1;
    }
}
