namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Represents an abstraction for providing detailed information about exceptions.
/// </summary>
public interface IExceptionDetail
{
    /// <summary>
    /// Gets the detail code.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the detail message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the target field or logical path associated with the detail.
    /// </summary>
    string? Target { get; }
}