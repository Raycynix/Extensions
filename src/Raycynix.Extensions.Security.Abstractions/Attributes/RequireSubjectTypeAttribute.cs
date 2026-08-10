using Raycynix.Extensions.Security.Abstractions.Enums;

namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to match a specific subject type before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireSubjectTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a subject type authorization requirement.
    /// </summary>
    public RequireSubjectTypeAttribute(SecuritySubjectType subjectType)
    {
        if (!Enum.IsDefined(subjectType))
        {
            throw new ArgumentOutOfRangeException(nameof(subjectType), subjectType, "Unknown security subject type.");
        }

        SubjectType = subjectType;
    }

    /// <summary>
    /// Gets the required subject type.
    /// </summary>
    public SecuritySubjectType SubjectType { get; }
}
