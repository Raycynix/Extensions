namespace Raycynix.Extensions.Email.Abstractions.Exceptions;

/// <summary>
/// Represents an exception thrown when an email provider is missing required configuration.
/// </summary>
public sealed class EmailProviderConfigurationException(string message) : Exception(message)
{
}
