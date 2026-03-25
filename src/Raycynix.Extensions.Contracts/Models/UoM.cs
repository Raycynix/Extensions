namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a unit of measure in a contract-safe format.
/// </summary>
public class UoM
{
    /// <summary>
    /// Gets or sets the canonical unit code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the unit.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
