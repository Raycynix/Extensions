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
    public UoM UoM { get; set; } = new();
}
