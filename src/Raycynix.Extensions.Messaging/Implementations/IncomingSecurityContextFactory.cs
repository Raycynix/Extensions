using Raycynix.Extensions.Messaging.Abstractions.Constants;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Builds immutable inbound <see cref="ISecurityContext"/> instances from transport headers.
/// </summary>
internal sealed class IncomingSecurityContextFactory
{
    /// <summary>
    /// Creates a security context from the supplied inbound transport headers.
    /// </summary>
    /// <param name="headers">The inbound transport headers.</param>
    /// <returns>The created security context.</returns>
    public ISecurityContext Create(IReadOnlyDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (!headers.TryGetValue(MessageHeaderNames.Authenticated, out var authenticatedValue) ||
            !bool.TryParse(authenticatedValue, out var isAuthenticated) ||
            !isAuthenticated)
        {
            return new MessagingSecurityContext();
        }

        var subjectType = SecuritySubjectType.User;
        if (headers.TryGetValue(MessageHeaderNames.SubjectType, out var subjectTypeValue) &&
            Enum.TryParse<SecuritySubjectType>(subjectTypeValue, ignoreCase: true, out var parsedSubjectType))
        {
            subjectType = parsedSubjectType;
        }

        return new MessagingSecurityContext
        {
            IsAuthenticated = true,
            SubjectId = headers.TryGetValue(MessageHeaderNames.SubjectId, out var subjectId) ? subjectId : string.Empty,
            SubjectType = subjectType,
            Roles = SplitHeaderValues(headers, MessageHeaderNames.SubjectRoles),
            Permissions = SplitHeaderValues(headers, MessageHeaderNames.SubjectPermissions)
        };
    }

    private static IReadOnlyCollection<string> SplitHeaderValues(
        IReadOnlyDictionary<string, string> headers,
        string key)
    {
        if (!headers.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
