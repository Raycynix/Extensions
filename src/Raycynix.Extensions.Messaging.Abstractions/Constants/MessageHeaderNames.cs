namespace Raycynix.Extensions.Messaging.Abstractions.Constants;

/// <summary>
/// Defines standard transport headers used by Raycynix messaging.
/// </summary>
public static class MessageHeaderNames
{
    /// <summary>
    /// Gets the correlation identifier header.
    /// </summary>
    public const string CorrelationId = "X-Correlation-Id";

    /// <summary>
    /// Gets the request identifier header.
    /// </summary>
    public const string RequestId = "X-Request-Id";

    /// <summary>
    /// Gets the W3C trace parent header.
    /// </summary>
    public const string TraceParent = "traceparent";

    /// <summary>
    /// Gets the W3C trace state header.
    /// </summary>
    public const string TraceState = "tracestate";

    /// <summary>
    /// Gets the payload format header.
    /// </summary>
    public const string Format = "X-Message-Format";

    /// <summary>
    /// Gets the authentication marker header.
    /// </summary>
    public const string Authenticated = "X-Subject-Authenticated";

    /// <summary>
    /// Gets the subject identifier header.
    /// </summary>
    public const string SubjectId = "X-Subject-Id";

    /// <summary>
    /// Gets the subject type header.
    /// </summary>
    public const string SubjectType = "X-Subject-Type";

    /// <summary>
    /// Gets the roles header.
    /// </summary>
    public const string SubjectRoles = "X-Subject-Roles";

    /// <summary>
    /// Gets the permissions header.
    /// </summary>
    public const string SubjectPermissions = "X-Subject-Permissions";

    /// <summary>
    /// Gets the logical source header identifying the publishing service or caller.
    /// </summary>
    public const string Source = "X-Message-Source";
}
