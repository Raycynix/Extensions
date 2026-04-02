using Raycynix.Extensions.Messaging.Abstractions.Constants;
using Raycynix.Extensions.Messaging.Abstractions.Exceptions;
using Raycynix.Extensions.Security.Abstractions.Enums;

namespace Raycynix.Extensions.Messaging.Internal;

/// <summary>
/// Validates inbound security headers carried by broker and direct messaging transports.
/// </summary>
internal sealed class IncomingSecurityHeadersValidator(Configurations.MessagingConfiguration configuration)
{
    /// <summary>
    /// Validates the supplied inbound headers.
    /// </summary>
    /// <param name="headers">The inbound headers to validate.</param>
    public void Validate(IReadOnlyDictionary<string, string> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        ValidateTrustedSource(headers);

        if (!headers.TryGetValue(MessageHeaderNames.Authenticated, out var authenticatedValue))
        {
            return;
        }

        if (!bool.TryParse(authenticatedValue, out var isAuthenticated))
        {
            throw new IncomingSecurityHeadersValidationException(
                $"Inbound security header '{MessageHeaderNames.Authenticated}' must contain a boolean value.");
        }

        var hasSubjectId = headers.TryGetValue(MessageHeaderNames.SubjectId, out var subjectId) &&
            !string.IsNullOrWhiteSpace(subjectId);
        var hasSubjectType = headers.TryGetValue(MessageHeaderNames.SubjectType, out var subjectTypeValue) &&
            !string.IsNullOrWhiteSpace(subjectTypeValue);
        var hasRoles = headers.TryGetValue(MessageHeaderNames.SubjectRoles, out var rolesValue) &&
            !string.IsNullOrWhiteSpace(rolesValue);
        var hasPermissions = headers.TryGetValue(MessageHeaderNames.SubjectPermissions, out var permissionsValue) &&
            !string.IsNullOrWhiteSpace(permissionsValue);

        if (!isAuthenticated)
        {
            if (hasSubjectId || hasSubjectType || hasRoles || hasPermissions)
            {
                throw new IncomingSecurityHeadersValidationException(
                    "Anonymous inbound messages cannot include authenticated subject headers.");
            }

            return;
        }

        if (!hasSubjectId)
        {
            throw new IncomingSecurityHeadersValidationException(
                $"Authenticated inbound messages must include '{MessageHeaderNames.SubjectId}'.");
        }

        if (!hasSubjectType || !Enum.TryParse<SecuritySubjectType>(subjectTypeValue, ignoreCase: true, out _))
        {
            throw new IncomingSecurityHeadersValidationException(
                $"Authenticated inbound messages must include a valid '{MessageHeaderNames.SubjectType}'.");
        }
    }

    private void ValidateTrustedSource(IReadOnlyDictionary<string, string> headers)
    {
        if (configuration.IncomingProcessing.TrustedSources.Count == 0)
        {
            return;
        }

        if (!headers.TryGetValue(MessageHeaderNames.Source, out var source) || string.IsNullOrWhiteSpace(source))
        {
            throw new IncomingMessageAuthenticationException(
                $"Inbound messages must include '{MessageHeaderNames.Source}' when trusted sources are configured.");
        }

        if (!configuration.IncomingProcessing.TrustedSources.Contains(source, StringComparer.OrdinalIgnoreCase))
        {
            throw new IncomingMessageAuthenticationException(
                $"Inbound source '{source}' is not trusted.");
        }
    }
}
