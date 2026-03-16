using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides helper methods to initialize the database during application startup.
/// </summary>
public static class DatabaseInitialization
{
    /// <summary>
    /// Initializes the configured database before the application starts serving requests.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
    public static async Task<WebApplication> UseRaycynixDatabaseInitializationAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

        await initializer.InitializeAsync(cancellationToken);

        return app;
    }
}