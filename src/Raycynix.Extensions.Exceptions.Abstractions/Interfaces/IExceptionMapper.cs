namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Defines a contract for converting exceptions to <see cref="RaycynixException"/> instances.
/// </summary>
public interface IExceptionMapper
{
    /// <summary>
    /// Maps an exception to a Raycynix exception.
    /// </summary>
    /// <param name="exception">The exception to be mapped.</param>
    /// <returns>The mapped exception.</returns>
    RaycynixException Map(Exception exception);
}
