namespace Raycynix.Extensions.Messaging.Abstractions.Exceptions;

/// <summary>
/// Represents an exception indicating that an inbound subject is authenticated but is not authorized to execute a messaging handler.
/// </summary>
public sealed class IncomingMessageAuthorizationException(string message) : Exception(message)
{
}
