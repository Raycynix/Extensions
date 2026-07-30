using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Internal;
using Raycynix.Extensions.Security.AspNetCore.Options;

namespace Raycynix.Extensions.Security.AspNetCore;

/// <summary>
/// Provides configurable ASP.NET Core rate limiting registration and middleware extensions.
/// </summary>
public static class RateLimitExtensions
{
    /// <summary>
    /// Registers global and named rate limit policies from the <c>RateLimitOptions</c> configuration section.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration used to bind rate limit settings.</param>
    /// <param name="setup">An optional callback for adjusting the bound rate limit settings.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RateLimitOptions>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddRaycynixConfiguration<RateLimitOptions>(
            configuration,
            configurePostBind: setup);
        services.AddRaycynixConfigurationValidator<RateLimitOptions, RateLimitOptionsValidator>();
        services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<RateLimitOptions>>().Current);

        services.AddRateLimiter();
        services.AddOptions<RateLimiterOptions>()
            .Configure<RateLimitOptions>(RaycynixRateLimiterOptions.Configure);

        return services;
    }

    /// <summary>
    /// Adds the configured Raycynix rate limiting middleware to the request pipeline.
    /// Place it after authentication and before authorization when subject partitioning is used.
    /// </summary>
    /// <param name="app">The application builder to update.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseRaycynixRateLimiting(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseRateLimiter();
    }
}
