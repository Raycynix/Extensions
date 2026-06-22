using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Email.Abstractions.Interfaces;

namespace Raycynix.Extensions.Email.Implementations;

/// <summary>
/// Captures the single resolved email provider registration used by the shared email infrastructure.
/// </summary>
public sealed class EmailProviderDescriptor
{
    /// <summary>
    /// Gets the normalized logical name of the active provider.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Gets the provider-specific registration implementation.
    /// </summary>
    public required IEmailProviderRegistration Registration { get; init; }

    /// <summary>
    /// Resolves the single active email provider registration from the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider containing email provider registrations.</param>
    /// <returns>A descriptor for the active email provider.</returns>
    public static EmailProviderDescriptor Resolve(IServiceProvider serviceProvider)
    {
        var registrations = serviceProvider.GetServices<IEmailProviderRegistration>().ToArray();
        var logger = serviceProvider.GetService<ILogger<EmailProviderDescriptor>>();

        logger?.LogDebug(
            "Found {ProviderCount} Raycynix email provider registration(s).",
            registrations.Length);

        return registrations.Length switch
        {
            1 => new EmailProviderDescriptor
            {
                ProviderName = registrations[0].ProviderName,
                Registration = registrations[0]
            },
            0 => throw new NotSupportedException(
                "No email provider is registered. Add exactly one matching provider package, for example AddSmtp()."),
            _ => throw new InvalidOperationException(
                $"Multiple email providers are registered ({string.Join(", ", registrations.Select(static registration => registration.ProviderName))}). Register exactly one email provider package.")
        };
    }
}
