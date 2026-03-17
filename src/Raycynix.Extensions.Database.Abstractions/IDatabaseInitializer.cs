namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Defines the contract for preparing the database during application startup.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Initializes the database according to the configured strategy.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the initialization operation.</param>
    /// <returns>A task that completes when initialization finishes.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value indicating whether the database is ready for use.
    /// </summary>
    bool IsReady { get; }
}
