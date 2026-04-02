using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Messaging.Abstractions.Constants;
using Raycynix.Extensions.Messaging.Abstractions.Exceptions;
using Raycynix.Extensions.Security.Abstractions.Attributes;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Internal;

/// <summary>
/// Evaluates declarative messaging authorization requirements declared on handler types.
/// </summary>
internal sealed class MessagingAuthorizationEvaluator(ISecurityContext securityContext)
{
    /// <summary>
    /// Evaluates messaging authorization requirements for the supplied handler type.
    /// </summary>
    /// <param name="handlerType">The handler implementation type.</param>
    /// <param name="headers">The inbound transport headers.</param>
    public void Authorize(Type handlerType, IReadOnlyDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(handlerType);
        ArgumentNullException.ThrowIfNull(headers);

        var requireAuthenticated = handlerType.IsDefined(typeof(RequireAuthenticatedSubjectAttribute), inherit: true) ||
            handlerType.IsDefined(typeof(RequireSubjectTypeAttribute), inherit: true) ||
            handlerType.IsDefined(typeof(RequirePermissionAttribute), inherit: true) ||
            handlerType.IsDefined(typeof(RequireAnyPermissionAttribute), inherit: true) ||
            handlerType.IsDefined(typeof(RequireAllPermissionsAttribute), inherit: true) ||
            handlerType.IsDefined(typeof(RequireRoleAttribute), inherit: true) ||
            handlerType.IsDefined(typeof(RequireAnyRoleAttribute), inherit: true) ||
            handlerType.IsDefined(typeof(RequireAllRolesAttribute), inherit: true);

        if (requireAuthenticated && !securityContext.IsAuthenticated)
        {
            throw new IncomingMessageAuthenticationException(
                $"Messaging handler '{handlerType.FullName}' requires an authenticated subject.");
        }

        foreach (var requirement in handlerType.GetCustomAttributes(typeof(RequireSubjectTypeAttribute), inherit: true).Cast<RequireSubjectTypeAttribute>())
        {
            if (securityContext.SubjectType != requirement.SubjectType)
            {
                throw new IncomingMessageAuthorizationException(
                    $"Messaging handler '{handlerType.FullName}' requires subject type '{requirement.SubjectType}'.");
            }
        }

        foreach (var requirement in handlerType.GetCustomAttributes(typeof(RequirePermissionAttribute), inherit: true).Cast<RequirePermissionAttribute>())
        {
            if (!securityContext.Permissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase))
            {
                throw new IncomingMessageAuthorizationException(
                    $"Messaging handler '{handlerType.FullName}' requires permission '{requirement.Permission}'.");
            }
        }

        foreach (var requirement in handlerType.GetCustomAttributes(typeof(RequireAnyPermissionAttribute), inherit: true).Cast<RequireAnyPermissionAttribute>())
        {
            if (requirement.Permissions.Count > 0 &&
                !requirement.Permissions.Any(permission => securityContext.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)))
            {
                throw new IncomingMessageAuthorizationException(
                    $"Messaging handler '{handlerType.FullName}' requires at least one configured permission.");
            }
        }

        foreach (var requirement in handlerType.GetCustomAttributes(typeof(RequireAllPermissionsAttribute), inherit: true).Cast<RequireAllPermissionsAttribute>())
        {
            if (requirement.Permissions.Any(permission => !securityContext.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)))
            {
                throw new IncomingMessageAuthorizationException(
                    $"Messaging handler '{handlerType.FullName}' requires all configured permissions.");
            }
        }

        foreach (var requirement in handlerType.GetCustomAttributes(typeof(RequireRoleAttribute), inherit: true).Cast<RequireRoleAttribute>())
        {
            if (!securityContext.Roles.Contains(requirement.Role, StringComparer.OrdinalIgnoreCase))
            {
                throw new IncomingMessageAuthorizationException(
                    $"Messaging handler '{handlerType.FullName}' requires role '{requirement.Role}'.");
            }
        }

        foreach (var requirement in handlerType.GetCustomAttributes(typeof(RequireAnyRoleAttribute), inherit: true).Cast<RequireAnyRoleAttribute>())
        {
            if (requirement.Roles.Count > 0 &&
                !requirement.Roles.Any(role => securityContext.Roles.Contains(role, StringComparer.OrdinalIgnoreCase)))
            {
                throw new IncomingMessageAuthorizationException(
                    $"Messaging handler '{handlerType.FullName}' requires at least one configured role.");
            }
        }

        foreach (var requirement in handlerType.GetCustomAttributes(typeof(RequireAllRolesAttribute), inherit: true).Cast<RequireAllRolesAttribute>())
        {
            if (requirement.Roles.Any(role => !securityContext.Roles.Contains(role, StringComparer.OrdinalIgnoreCase)))
            {
                throw new IncomingMessageAuthorizationException(
                    $"Messaging handler '{handlerType.FullName}' requires all configured roles.");
            }
        }

        var source = headers.TryGetValue(MessageHeaderNames.Source, out var sourceValue) ? sourceValue : null;
        foreach (var requirement in handlerType.GetCustomAttributes(typeof(RequireTrustedSourceAttribute), inherit: true).Cast<RequireTrustedSourceAttribute>())
        {
            if (string.IsNullOrWhiteSpace(source) ||
                !requirement.Sources.Contains(source, StringComparer.OrdinalIgnoreCase))
            {
                throw new IncomingMessageAuthenticationException(
                    $"Messaging handler '{handlerType.FullName}' requires a trusted source.");
            }
        }
    }
}
