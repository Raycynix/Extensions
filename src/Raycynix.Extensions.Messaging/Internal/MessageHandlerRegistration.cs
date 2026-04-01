namespace Raycynix.Extensions.Messaging.Internal;

/// <summary>
/// Stores a registered message payload type for inbound type resolution.
/// </summary>
internal sealed record MessageHandlerRegistration(Type MessageType);
