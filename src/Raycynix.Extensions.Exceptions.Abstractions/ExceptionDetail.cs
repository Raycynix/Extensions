using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Abstractions;

/// <summary>
/// Represents a concrete machine-readable detail item attached to an exception.
/// </summary>
public record ExceptionDetail(
    string Code,
    string Message,
    string? Target = null) : IExceptionDetail;