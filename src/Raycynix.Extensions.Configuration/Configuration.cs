using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Internal;

namespace Raycynix.Extensions.Configuration;

/// <summary>
/// Provides service registration extensions for the Raycynix configuration package.
/// </summary>
public static class Configuration
{
    /// <summary>
    /// Registers a typed configuration model using the standard Options pipeline with Raycynix conventions.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="sectionName">An optional configuration section name. Defaults to the model type name.</param>
    /// <param name="configureDefaults">An optional callback for applying default values before configuration binding.</param>
    /// <param name="configureBinder">An optional callback for binder behavior customization.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfiguration<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null,
        Action<TOptions>? configureDefaults = null,
        Action<BinderOptions>? configureBinder = null)
        where TOptions : class, new()
    {
        sectionName ??= typeof(TOptions).Name;
        var section = configuration.GetSection(sectionName);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationDefaults<TOptions>, EmptyConfigurationDefaults<TOptions>>());

        var optionsBuilder = services.AddOptions<TOptions>();

        optionsBuilder.Configure<IEnumerable<IConfigurationDefaults<TOptions>>>((options, defaultsProviders) =>
        {
            foreach (var defaultsProvider in defaultsProviders)
            {
                defaultsProvider.Apply(options);
            }

            configureDefaults?.Invoke(options);
        });

        optionsBuilder.Bind(section, configureBinder ?? (_ => { }));

        return services;
    }
}
