using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Security.Implementation;

public sealed class SecurityContext : ISecurityContext
{
    public bool IsAuthenticated { get; init; }
    public string SubjectId { get;  init;} = string.Empty;
    public SecuritySubjectType SubjectType { get;  init;}
    public IReadOnlyCollection<string> Roles { get;  init;} = [];
    public IReadOnlyCollection<string> Permissions { get;  init;} = [];
}