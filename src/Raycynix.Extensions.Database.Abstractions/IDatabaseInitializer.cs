namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Defines the contract for initializing a database.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Asynchronously initializes the database by creating and/or migrating it based on the configuration.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete. Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. Upon completion, indicates whether the database initialization is complete.
    /// </returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicates whether the database initialization process has been successfully completed.
    /// </summary>
    /// <remarks>
    /// This property returns true if the database has been initialized and is ready for use; otherwise, it returns false.
    /// The value is typically updated as part of the completion of the initialization process in
    /// an implementation of <see cref="IDatabaseInitializer.InitializeAsync"/>.
    /// </remarks>
    bool IsReady { get; }
}