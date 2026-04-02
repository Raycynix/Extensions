namespace Raycynix.Extensions.Messaging.Abstractions.Exceptions;

/// <summary>
/// Represents an exception indicating that inbound messaging authentication or trust validation failed.
/// </summary>
public sealed class IncomingMessageAuthenticationException(string message) : Exception(message)
{
}
