using System.ComponentModel.DataAnnotations;

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
    [Required]
    [MinLength(3)]
    [MaxLength(3)]
    [RegularExpression("^[A-Z]{3}$")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Determines whether the monetary value is structurally valid for transport.
    /// </summary>
    /// <returns><c>true</c> when the model is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        return Currency.Length == 3 && Currency.All(static c => char.IsAsciiLetterUpper(c));
    }
}
