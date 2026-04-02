namespace Raycynix.Extensions.Messaging.Abstractions.Exceptions;

/// <summary>
/// Represents an inbound messaging security header validation failure.
/// </summary>
public sealed class IncomingSecurityHeadersValidationException(string message) : InvalidOperationException(message);
