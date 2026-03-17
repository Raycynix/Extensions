using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides extensions for running database initialization during application startup.
/// </summary>
public static class DatabaseInitialization
{
    /// <summary>
    /// Resolves <see cref="IDatabaseInitializer"/> and runs database initialization.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="cancellationToken">The cancellation token for the initialization operation.</param>
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
