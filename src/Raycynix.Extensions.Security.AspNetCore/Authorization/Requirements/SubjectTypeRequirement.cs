using Microsoft.AspNetCore.Authorization;
using Raycynix.Extensions.Security.Abstractions.Enums;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

/// <summary>
/// Represents a requirement that the current subject must match a specific subject type.
/// </summary>
public sealed class SubjectTypeRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubjectTypeRequirement"/> class.
    /// </summary>
    /// <param name="subjectType">The subject type required to satisfy the policy.</param>
    public SubjectTypeRequirement(SecuritySubjectType subjectType)
    {
        SubjectType = subjectType;
    }

    /// <summary>
    /// Gets the subject type required to satisfy the policy.
    /// </summary>
    public SecuritySubjectType SubjectType { get; }
}
