using System.ComponentModel.DataAnnotations;

namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a unit of measure in a contract-safe format.
/// </summary>
public class UoM
{
    /// <summary>
    /// Gets or sets the canonical unit code.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the unit.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Determines whether the unit of measure is structurally valid for transport.
    /// </summary>
    /// <returns><c>true</c> when the model is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(Name);
    }
}
