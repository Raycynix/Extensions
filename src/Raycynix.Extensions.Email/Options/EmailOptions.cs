using Raycynix.Extensions.Email.Abstractions.Models;

namespace Raycynix.Extensions.Email.Options;

/// <summary>
/// Represents shared email settings used by all providers.
/// </summary>
public sealed class EmailOptions
{
    /// <summary>
    /// Gets or sets the default sender email address used when a message does not specify <see cref="EmailMessage.From"/>.
    /// </summary>
    public string? DefaultFromAddress { get; set; }

    /// <summary>
    /// Gets or sets the default sender display name.
    /// </summary>
    public string? DefaultFromDisplayName { get; set; }

    /// <summary>
    /// Gets or sets the default reply-to email address used when a message does not specify <see cref="EmailMessage.ReplyTo"/>.
    /// </summary>
    public string? DefaultReplyToAddress { get; set; }

    /// <summary>
    /// Gets or sets the default reply-to display name.
    /// </summary>
    public string? DefaultReplyToDisplayName { get; set; }

    /// <summary>
    /// Resolves the default sender address.
    /// </summary>
    /// <returns>The configured default sender address, or <see langword="null"/> when it is not configured.</returns>
    public EmailAddress? ResolveDefaultFrom()
    {
        return string.IsNullOrWhiteSpace(DefaultFromAddress)
            ? null
            : new EmailAddress(DefaultFromAddress, DefaultFromDisplayName);
    }

    /// <summary>
    /// Resolves the default reply-to address.
    /// </summary>
    /// <returns>The configured default reply-to address, or <see langword="null"/> when it is not configured.</returns>
    public EmailAddress? ResolveDefaultReplyTo()
    {
        return string.IsNullOrWhiteSpace(DefaultReplyToAddress)
            ? null
            : new EmailAddress(DefaultReplyToAddress, DefaultReplyToDisplayName);
    }
}
