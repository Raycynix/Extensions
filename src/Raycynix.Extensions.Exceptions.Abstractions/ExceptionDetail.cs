using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Abstractions;

/// <summary>
/// Represents a concrete machine-readable detail item attached to an exception.
/// </summary>
public sealed record ExceptionDetail : IExceptionDetail
{
    /// <summary>
    /// Initializes a new exception detail.
    /// </summary>
    public ExceptionDetail(string code, string message, string? target = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (target is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(target);
        }

        Code = code;
        Message = message;
        Target = target;
    }

    /// <inheritdoc />
    public string Code { get; }

    /// <inheritdoc />
    public string Message { get; }

    /// <inheritdoc />
    public string? Target { get; }
}
