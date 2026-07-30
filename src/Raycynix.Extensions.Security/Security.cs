using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Implementations;
using Raycynix.Extensions.Security.Internal;
using Raycynix.Extensions.Security.Options;

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
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ISecurityContext, SecurityContext>();

        return services;
    }

    /// <summary>
    /// Registers the core security services and binds <see cref="SecurityOptions"/> from application configuration.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration used to bind <see cref="SecurityOptions"/>.</param>
    /// <param name="setup">An optional callback for adjusting the bound security configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SecurityOptions>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddRaycynixConfiguration<SecurityOptions>(
            configuration,
            configurePostBind: setup);
        services.AddRaycynixConfigurationValidator<SecurityOptions, SecurityOptionsValidator>();
        services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<SecurityOptions>>().Current);
        services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<SecurityOptions>().JwtOptions);
        services.TryAddScoped<ISecurityContext, SecurityContext>();

        return services;
    }
}
