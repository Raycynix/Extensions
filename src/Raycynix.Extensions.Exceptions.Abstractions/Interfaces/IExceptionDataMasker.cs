namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Defines a contract for masking exception data to ensure sensitive or unnecessary information
/// is obscured or transformed in a safe and consistent manner. This can be used to sanitize
/// data before exposing it to external systems or logs.
/// Provides the capability to process various types of data, including collections, objects,
/// and primitive types, to enforce data masking rules defined by the implementation.
/// </summary>
public interface IExceptionDataMasker
{
    /// <summary>
    /// Masks the given data to obfuscate or hide sensitive information.
    /// </summary>
    /// <param name="data">The object containing the data to be masked. This can be null.</param>
    /// <return>
    /// A masked version of the input data, or null if the input data is null.
    /// </return>
    object? Mask(object? data);
}