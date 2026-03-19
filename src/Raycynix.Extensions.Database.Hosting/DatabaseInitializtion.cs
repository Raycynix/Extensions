using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Hosting;

/// <summary>
/// Provides extensions for running database initialization in generic-host based applications.
/// </summary>
public static class DatabaseInitialization
{
    /// <summary>
     /// Resolves <see cref="IDatabaseInitializer"/> and runs database initialization.
     /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the initializer.</param>
    /// <param name="cancellationToken">The cancellation token for the initialization operation.</param>
    public static async Task InitializeRaycynixDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

        await initializer.InitializeAsync(cancellationToken);
    }

    /// <summary>
    /// Resolves <see cref="IDatabaseInitializer"/> from the host service provider and runs database initialization.
    /// </summary>
    /// <param name="host">The host whose services should be used.</param>
    /// <param name="cancellationToken">The cancellation token for the initialization operation.</param>
    public static Task InitializeRaycynixDatabaseAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        return host.Services.InitializeRaycynixDatabaseAsync(cancellationToken);
    }
}
