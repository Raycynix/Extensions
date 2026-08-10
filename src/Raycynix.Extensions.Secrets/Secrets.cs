using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Secrets.Implementations;
using Raycynix.Extensions.Secrets.Internal;
using Raycynix.Extensions.Secrets.Options;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets;

/// <summary>
/// Provides service registration extensions for the Raycynix secrets package.
/// </summary>
public static class Secrets
{
    /// <summary>
    /// Registers the default secret providers and the composite secret resolver.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration used to bind <see cref="SecretOptions"/>.</param>
    /// <param name="setup">An optional callback for adjusting secret resolution behavior.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixSecrets(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SecretOptions>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton(configuration);
        services.AddRaycynixConfiguration<SecretOptions>(
            configuration,
            configurePostBind: setup);
        services.AddRaycynixConfigurationValidator<SecretOptions, SecretOptionsValidator>();
        services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<SecretOptions>>().Current);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISecretProvider, ConfigurationSecretProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISecretProvider, EnvironmentSecretProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISecretProvider, GitHubSecretProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISecretProvider, TeamCitySecretProvider>());
        services.TryAddSingleton<CompositeSecretResolver>();
        services.TryAddSingleton<ISecretResolver>(serviceProvider =>
            serviceProvider.GetRequiredService<CompositeSecretResolver>());
        services.TryAddSingleton<ISecretDiagnosticsResolver>(serviceProvider =>
            serviceProvider.GetRequiredService<CompositeSecretResolver>());

        return services;
    }
}
