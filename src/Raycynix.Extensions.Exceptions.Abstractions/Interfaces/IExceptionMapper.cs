namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Defines a contract for mapping generic exceptions to instances of <see cref="RaycynixException"/>.
/// Provides an abstraction to enable configurable and consistent exception handling
/// across the application by converting various exceptions into a standard format.
/// </summary>
public interface IExceptionMapper
{
    /// Maps a given exception to a corresponding RaycynixException based on predefined mappings.
    /// <param name="exception">The exception to be mapped.</param>
    /// <returns>A RaycynixException instance that represents the mapped exception.</returns>
    RaycynixException Map(Exception exception);
}