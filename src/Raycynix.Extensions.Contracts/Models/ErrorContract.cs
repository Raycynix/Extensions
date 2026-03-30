using System.ComponentModel.DataAnnotations;

namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a transport-safe error contract for cross-service communication.
/// </summary>
public class ErrorContract
{
    /// <summary>
    /// Gets or sets the machine-readable error code.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable error message.
    /// </summary>
    [Required]
    [MaxLength(2048)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the logical error target, such as a field, route, or operation.
    /// </summary>
    [MaxLength(256)]
    public string? Target { get; set; }

    /// <summary>
    /// Gets or sets the distributed tracing identifier associated with the failure.
    /// </summary>
    [MaxLength(128)]
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets additional machine-readable detail values.
    /// </summary>
    public IReadOnlyDictionary<string, string?> Details { get; set; } = new Dictionary<string, string?>();

    /// <summary>
    /// Gets or sets field-level validation errors when the failure is validation-related.
    /// </summary>
    public IReadOnlyCollection<ValidationError> ValidationErrors { get; set; } = Array.Empty<ValidationError>();

    /// <summary>
    /// Determines whether the error contract is structurally valid for transport.
    /// </summary>
    /// <returns><c>true</c> when the model is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Code) &&
               !string.IsNullOrWhiteSpace(Message) &&
               ValidationErrors.All(static error =>
                   !string.IsNullOrWhiteSpace(error.Field) &&
                   !string.IsNullOrWhiteSpace(error.Code) &&
                   !string.IsNullOrWhiteSpace(error.Message));
    }
}
