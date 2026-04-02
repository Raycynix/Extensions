namespace Raycynix.Extensions.Messaging.Internal;

/// <summary>
/// Stores a registered request/response type pair and destination for direct request dispatch.
/// </summary>
internal sealed record RequestHandlerRegistration(
    Type RequestType,
    Type ResponseType,
    Type HandlerType,
    string Destination);
