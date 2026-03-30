using System.ComponentModel.DataAnnotations;

namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a validation error within a contract response.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Gets or sets the field or member name that failed validation.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the machine-readable validation code.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable validation message.
    /// </summary>
    [Required]
    [MaxLength(2048)]
    public string Message { get; set; } = string.Empty;
}
