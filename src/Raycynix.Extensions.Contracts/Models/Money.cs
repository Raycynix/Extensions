namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a monetary value in a contract-safe format.
/// </summary>
public class Money
{
    /// <summary>
    /// Gets or sets the numeric amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the ISO currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
}
