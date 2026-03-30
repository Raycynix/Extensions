using System.ComponentModel.DataAnnotations;

namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a paged collection response in a contract-safe format.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
public class PagedResult<TItem>
{
    /// <summary>
    /// Gets or sets the current page items.
    /// </summary>
    public IReadOnlyCollection<TItem> Items { get; set; } = Array.Empty<TItem>();

    /// <summary>
    /// Gets or sets the paging metadata.
    /// </summary>
    [Required]
    public PageInfo PageInfo { get; set; } = new();

    /// <summary>
    /// Determines whether the paged result is structurally valid for transport.
    /// </summary>
    /// <returns><c>true</c> when the model is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        return PageInfo.IsValid();
    }
}
