using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration.AspNetCore.FeatureGate;
using Raycynix.Extensions.Configuration.AspNetCore.Middleware;
using Raycynix.Extensions.Configuration.Configurations;

namespace Raycynix.Extensions.Configuration.AspNetCore;

/// <summary>
/// Provides ASP.NET Core-specific registration and pipeline extensions for Raycynix configuration.
/// </summary>
public static class Configuration
{
    /// <summary>
    /// Registers options used by the Raycynix ASP.NET Core feature gate middleware.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configure">An optional callback for customizing feature gate responses.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixFeatureGateOptions(
        this IServiceCollection services,
        Action<FeatureGateOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<FeatureGateOptions>();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        return services;
    }

    /// <summary>
    /// Applies the standard Raycynix ASP.NET Core configuration conventions to the web application builder.
    /// </summary>
    /// <param name="builder">The web application builder to update.</param>
    /// <param name="setup">An optional callback for adjusting source registration behavior.</param>
    /// <returns>The same <see cref="WebApplicationBuilder"/> instance for chaining.</returns>
    public static WebApplicationBuilder AddRaycynixAspNetCoreConfiguration(
        this WebApplicationBuilder builder,
        Action<ConfigurationSourcesOptions>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddRaycynixFeatureGateOptions();

        builder.Configuration.UseRaycynixConfigurationSources(options =>
        {
            options.EnvironmentName = builder.Environment.EnvironmentName;
            options.IncludeUserSecrets = builder.Environment.IsDevelopment();

            setup?.Invoke(options);
        });

        builder.Services.AddRaycynixEnvironment();

        return builder;
    }

    /// <summary>
    /// Adds the Raycynix ASP.NET Core feature gate middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The configured application builder.</returns>
    public static IApplicationBuilder UseRaycynixAspNetCoreConfiguration(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<FeatureGateMiddleware>();

        return app;
    }

    /// <summary>
    /// Requires all specified feature flags to be enabled for the endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint builder.</param>
    /// <param name="featureFlags">The feature flag names.</param>
    /// <returns>The same endpoint builder for chaining.</returns>
    public static TBuilder RequireFeature<TBuilder>(this TBuilder builder, params string[] featureFlags)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        var metadata = FeatureGateMetadata.CreateAll(featureFlags);
        builder.Add(endpointBuilder => endpointBuilder.Metadata.Add(metadata));

        return builder;
    }

    /// <summary>
    /// Requires at least one of the specified feature flags to be enabled for the endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint builder.</param>
    /// <param name="featureFlags">The feature flag names.</param>
    /// <returns>The same endpoint builder for chaining.</returns>
    public static TBuilder RequireAnyFeature<TBuilder>(this TBuilder builder, params string[] featureFlags)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        var metadata = FeatureGateMetadata.CreateAny(featureFlags);
        builder.Add(endpointBuilder => endpointBuilder.Metadata.Add(metadata));

        return builder;
    }
}
