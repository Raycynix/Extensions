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
    public PageInfo PageInfo { get; set; } = new();
}
