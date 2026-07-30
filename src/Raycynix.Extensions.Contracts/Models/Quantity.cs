using System.ComponentModel.DataAnnotations;

namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a measurable quantity in a contract-safe format.
/// </summary>
public class Quantity
{
    /// <summary>
    /// Gets or sets the numeric value.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Gets or sets the unit of measure.
    /// </summary>
    [Required]
    public UoM UoM { get; set; } = new();

    /// <summary>
    /// Determines whether the quantity is structurally valid for transport.
    /// </summary>
    /// <returns><c>true</c> when the model is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        return UoM?.IsValid() == true;
    }
}
