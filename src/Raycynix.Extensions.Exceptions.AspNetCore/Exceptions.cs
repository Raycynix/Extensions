using Microsoft.AspNetCore.Builder;
using Raycynix.Extensions.Exceptions.AspNetCore.Middleware;

namespace Raycynix.Extensions.Exceptions.AspNetCore;

/// <summary>
/// Provides middleware extensions for the Raycynix exception handling package.
/// </summary>
public static class Exceptions
{
    /// <summary>
    /// Adds the Raycynix exception handling middleware to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The configured application builder.</returns>
    public static IApplicationBuilder UseRaycynixExceptions(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RaycynixExceptionMiddleware>();
    }
}
