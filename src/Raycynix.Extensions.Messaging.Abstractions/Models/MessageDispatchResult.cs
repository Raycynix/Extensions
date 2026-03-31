namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents the result of dispatching an incoming message to registered handlers.
/// </summary>
public sealed record MessageDispatchResult
{
    /// <summary>
    /// Gets the normalized dispatch context.
    /// </summary>
    public required MessageDispatchContext Context { get; init; }

    /// <summary>
    /// Gets the number of handlers that processed the message.
    /// </summary>
    public required int HandlerCount { get; init; }
}
