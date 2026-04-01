namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents the current processing state of an incoming message in the inbox store.
/// </summary>
public enum IncomingMessageInboxStatus
{
    /// <summary>
    /// Indicates that the message is currently being processed.
    /// </summary>
    Processing = 0,

    /// <summary>
    /// Indicates that the message has already been processed successfully.
    /// </summary>
    Processed = 1,

    /// <summary>
    /// Indicates that the last processing attempt failed.
    /// </summary>
    Failed = 2
}
