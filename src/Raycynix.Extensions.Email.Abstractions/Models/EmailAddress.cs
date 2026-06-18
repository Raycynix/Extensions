namespace Raycynix.Extensions.Email.Abstractions.Models;

/// <summary>
/// Represents an email mailbox address and optional display name.
/// </summary>
public sealed record EmailAddress
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddress"/> record.
    /// </summary>
    /// <param name="address">The mailbox address.</param>
    /// <param name="displayName">The optional display name.</param>
    public EmailAddress(string address, string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        if (!System.Net.Mail.MailAddress.TryCreate(address, out var parsedAddress) ||
            !string.Equals(parsedAddress.Address, address, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Email address must be valid.", nameof(address));
        }

        if (displayName?.Length > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(displayName), "Display name cannot exceed 255 characters.");
        }

        Address = address;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the mailbox address.
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// Gets the optional display name.
    /// </summary>
    public string? DisplayName { get; }
}
