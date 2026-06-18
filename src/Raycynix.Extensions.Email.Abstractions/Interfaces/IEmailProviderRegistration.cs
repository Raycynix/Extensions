namespace Raycynix.Extensions.Email.Abstractions.Interfaces;

/// <summary>
/// Defines provider-specific email provider metadata and validation behavior.
/// </summary>
public interface IEmailProviderRegistration
{
    /// <summary>
    /// Gets the logical provider name handled by the registration.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Validates provider-specific email configuration.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve provider-specific options.</param>
    void Validate(IServiceProvider serviceProvider);
}
