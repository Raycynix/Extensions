using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Security.Implementation;

/// <summary>
/// Provides the default immutable implementation of <see cref="ISecurityContext"/>.
/// </summary>
public sealed class SecurityContext : ISecurityContext
{
    /// <inheritdoc />
    public bool IsAuthenticated { get; init; }

    /// <inheritdoc />
    public string SubjectId { get; init; } = string.Empty;

    /// <inheritdoc />
    public SecuritySubjectType SubjectType { get; init; }

    /// <inheritdoc />
    public IReadOnlyCollection<string> Roles { get; init; } = [];

    /// <inheritdoc />
    public IReadOnlyCollection<string> Permissions { get; init; } = [];
}
