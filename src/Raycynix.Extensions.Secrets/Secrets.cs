using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Secrets.Implementations;
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
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixSecrets(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISecretProvider, EnvironmentSecretProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISecretProvider, GitHubSecretProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISecretProvider, TeamCitySecretProvider>());
        services.TryAddSingleton<ISecretResolver, CompositeSecretResolver>();

        return services;
    }
}
