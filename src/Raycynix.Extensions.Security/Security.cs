using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Configurations;
using Raycynix.Extensions.Security.Implementations;
using Raycynix.Extensions.Security.Internal;

namespace Raycynix.Extensions.Security;

/// <summary>
/// Provides service registration extensions for the Raycynix security package.
/// </summary>
public static class Security
{
    /// <summary>
    /// Registers the core security services without configuration binding.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixSecurity(this IServiceCollection services)
    {
        services.TryAddScoped<ISecurityContext, SecurityContext>();

        return services;
    }

    /// <summary>
    /// Registers the core security services and binds <see cref="SecurityConfiguration"/> from application configuration.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration used to bind <see cref="SecurityConfiguration"/>.</param>
    /// <param name="setup">An optional callback for adjusting the bound security configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SecurityConfiguration>? setup = null)
    {
        services.AddRaycynixConfiguration<SecurityConfiguration>(
            configuration,
            configurePostBind: setup);
        services.AddRaycynixConfigurationValidator<SecurityConfiguration, SecurityConfigurationValidator>();
        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<SecurityConfiguration>>().Current);
        services.TryAddScoped<ISecurityContext, SecurityContext>();

        return services;
    }
}
