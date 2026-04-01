namespace Raycynix.Extensions.Messaging.Database.Models;

/// <summary>
/// Represents a persisted inbox entry for the incoming message processing state.
/// </summary>
public sealed class MessagingInboxEntryEntity
{
    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message destination.
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the inbox status.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the latest processing error.
    /// </summary>
    public string? Error { get; set; }
}
