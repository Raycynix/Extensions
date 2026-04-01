using Raycynix.Extensions.Security.Abstractions.Enums;

namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to match a specific subject type before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequireSubjectTypeAttribute(SecuritySubjectType subjectType) : Attribute
{
    /// <summary>
    /// Gets the required subject type.
    /// </summary>
    public SecuritySubjectType SubjectType { get; } = subjectType;
}
