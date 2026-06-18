using Raycynix.Extensions.Email.Abstractions.Models;

namespace Raycynix.Extensions.Email.Abstractions.Interfaces;

/// <summary>
/// Sends email messages through the configured provider.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The provider send result.</returns>
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
