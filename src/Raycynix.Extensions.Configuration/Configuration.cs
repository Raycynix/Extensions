using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TOptions>, RaycynixValidateOptions<TOptions>>());

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
        optionsBuilder.ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Registers a Raycynix validator for a typed configuration model.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <typeparam name="TValidator">The validator type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfigurationValidator<TOptions, TValidator>(
        this IServiceCollection services)
        where TOptions : class, new()
        where TValidator : class, IConfigurationValidator<TOptions>
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TOptions>, RaycynixValidateOptions<TOptions>>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationValidator<TOptions>, TValidator>());

        return services;
    }

    /// <summary>
    /// Registers an inline Raycynix validator for a typed configuration model.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <param name="validate">The validation delegate.</param>
    /// <param name="failureMessage">The error message returned when validation fails.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfigurationValidator<TOptions>(
        this IServiceCollection services,
        Func<TOptions, bool> validate,
        string failureMessage)
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(validate);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TOptions>, RaycynixValidateOptions<TOptions>>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationValidator<TOptions>>(
                new DelegateConfigurationValidator<TOptions>(validate, failureMessage)));

        return services;
    }
}
