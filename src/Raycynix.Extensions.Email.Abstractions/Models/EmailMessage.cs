namespace Raycynix.Extensions.Email.Abstractions.Models;

/// <summary>
/// Represents an outgoing email message.
/// </summary>
public sealed class EmailMessage
{
    /// <summary>
    /// Gets or initializes the sender address override.
    /// </summary>
    public EmailAddress? From { get; init; }

    /// <summary>
    /// Gets or initializes the reply-to address.
    /// </summary>
    public EmailAddress? ReplyTo { get; init; }

    /// <summary>
    /// Gets or initializes the primary recipients.
    /// </summary>
    public IReadOnlyCollection<EmailAddress> To { get; init; } = [];

    /// <summary>
    /// Gets or initializes the carbon-copy recipients.
    /// </summary>
    public IReadOnlyCollection<EmailAddress> Cc { get; init; } = [];

    /// <summary>
    /// Gets or initializes the blind carbon-copy recipients.
    /// </summary>
    public IReadOnlyCollection<EmailAddress> Bcc { get; init; } = [];

    /// <summary>
    /// Gets or initializes the message subject.
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// Gets or initializes the message body.
    /// </summary>
    public required EmailBody Body { get; init; }

    /// <summary>
    /// Gets or initializes the attachments.
    /// </summary>
    public IReadOnlyCollection<EmailAttachment> Attachments { get; init; } = [];

    /// <summary>
    /// Gets or initializes custom provider headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or initializes provider-specific metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets a value indicating whether the message has at least one recipient.
    /// </summary>
    public bool HasRecipients =>
        (To?.Count ?? 0) > 0 ||
        (Cc?.Count ?? 0) > 0 ||
        (Bcc?.Count ?? 0) > 0;

    /// <summary>
    /// Validates the message before it is sent.
    /// </summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Subject);
        ArgumentNullException.ThrowIfNull(Body);
        ArgumentNullException.ThrowIfNull(To);
        ArgumentNullException.ThrowIfNull(Cc);
        ArgumentNullException.ThrowIfNull(Bcc);
        ArgumentNullException.ThrowIfNull(Attachments);
        ArgumentNullException.ThrowIfNull(Headers);
        ArgumentNullException.ThrowIfNull(Metadata);

        if (!HasRecipients)
        {
            throw new InvalidOperationException("Email message requires at least one recipient.");
        }

        ThrowIfContainsNull(To, nameof(To));
        ThrowIfContainsNull(Cc, nameof(Cc));
        ThrowIfContainsNull(Bcc, nameof(Bcc));
        ThrowIfContainsNull(Attachments, nameof(Attachments));
        ValidateAttachments();
        ValidateDictionary(Headers, nameof(Headers));
        ValidateDictionary(Metadata, nameof(Metadata));
    }

    private static void ThrowIfContainsNull<T>(IEnumerable<T> values, string parameterName)
    {
        if (values.Any(static value => value is null))
        {
            throw new InvalidOperationException($"{parameterName} cannot contain null values.");
        }
    }

    private void ValidateAttachments()
    {
        foreach (var attachment in Attachments)
        {
            attachment.Validate();
        }
    }

    private static void ValidateDictionary(
        IReadOnlyDictionary<string, string> values,
        string parameterName)
    {
        foreach (var (key, value) in values)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException($"{parameterName} cannot contain null or whitespace keys.");
            }

            if (value is null)
            {
                throw new InvalidOperationException($"{parameterName} cannot contain null values.");
            }
        }
    }
}
