using Raycynix.Extensions.Email.Abstractions.Enums;

namespace Raycynix.Extensions.Email.Abstractions.Models;

/// <summary>
/// Represents the text and HTML body content of an email message.
/// </summary>
public sealed class EmailBody
{
    private EmailBody(
        string? plainText,
        string? html,
        EmailBodyFormat preferredFormat)
    {
        PlainText = plainText;
        Html = html;
        PreferredFormat = preferredFormat;
    }

    /// <summary>
    /// Gets the plain text body content.
    /// </summary>
    public string? PlainText { get; }

    /// <summary>
    /// Gets the HTML body content.
    /// </summary>
    public string? Html { get; }

    /// <summary>
    /// Gets the preferred body format when both text and HTML content are available.
    /// </summary>
    public EmailBodyFormat PreferredFormat { get; }

    /// <summary>
    /// Creates a plain text email body.
    /// </summary>
    /// <param name="plainText">The plain text body content.</param>
    /// <returns>The email body.</returns>
    public static EmailBody FromPlainText(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

        return new EmailBody(
            plainText: plainText,
            html: null,
            preferredFormat: EmailBodyFormat.PlainText);
    }

    /// <summary>
    /// Creates an HTML email body.
    /// </summary>
    /// <param name="html">The HTML body content.</param>
    /// <param name="plainText">The optional plain text alternative.</param>
    /// <returns>The email body.</returns>
    public static EmailBody FromHtml(string html, string? plainText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(html);

        return new EmailBody(
            plainText: plainText,
            html: html,
            preferredFormat: EmailBodyFormat.Html);
    }
}
