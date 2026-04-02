using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Messaging.Database.Implementations;

/// <summary>
/// Initializes the messaging persistence schema during application startup.
/// </summary>
internal sealed class MessagingDatabasePersistenceInitializationService(
    IDatabaseInitializer databaseInitializer) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return databaseInitializer.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
