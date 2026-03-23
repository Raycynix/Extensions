using Raycynix.Extensions.Security.Abstractions.Enums;

namespace Raycynix.Extensions.Security.Abstractions.Interfaces;

/// <summary>
/// Represents the authenticated security context available during request processing.
/// </summary>
public interface ISecurityContext
{
    /// <summary>
    /// Gets a value indicating whether the current subject has been authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the unique identifier of the authenticated subject.
    /// </summary>
    string SubjectId { get; }

    /// <summary>
    /// Gets the type of authenticated subject.
    /// </summary>
    SecuritySubjectType SubjectType { get; }

    /// <summary>
    /// Gets the roles assigned to the authenticated subject.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// Gets the permissions granted to the authenticated subject.
    /// </summary>
    IReadOnlyCollection<string> Permissions { get; }
}
