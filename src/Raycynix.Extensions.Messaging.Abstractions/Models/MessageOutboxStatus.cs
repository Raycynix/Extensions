namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents the current state of an outgoing message stored in the outbox.
/// </summary>
public enum MessageOutboxStatus
{
    /// <summary>
    /// Indicates that the message is waiting to be published.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Indicates that the message is currently leased by a dispatcher or recovery worker.
    /// </summary>
    Dispatching = 1,

    /// <summary>
    /// Indicates that the message has already been published successfully.
    /// </summary>
    Dispatched = 2,

    /// <summary>
    /// Indicates that the latest publish attempt failed.
    /// </summary>
    Failed = 3
}
