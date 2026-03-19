using Microsoft.AspNetCore.Builder;
using Raycynix.Extensions.Database.Hosting;

namespace Raycynix.Extensions.Database.Web;

/// <summary>
/// Provides extensions for running database initialization during ASP.NET Core startup.
/// </summary>
public static class DatabaseInitialization
{
    /// <summary>
    /// Runs database initialization using the application's service provider.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="cancellationToken">The cancellation token for the initialization operation.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
    public static async Task<WebApplication> InitializeRaycynixDatabaseAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        await app.Services.InitializeRaycynixDatabaseAsync(cancellationToken);

        return app;
    }
}
