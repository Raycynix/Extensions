namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Defines a contract for masking sensitive exception data.
/// </summary>
public interface IExceptionDataMasker
{
    /// <summary>
    /// Masks sensitive values in the supplied data.
    /// </summary>
    /// <param name="data">The object containing the data to be masked. This can be null.</param>
    /// <returns>A sanitized representation of the input value.</returns>
    object? Mask(object? data);
}
