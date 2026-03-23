using Raycynix.Extensions.Security.Abstractions.Enums;

namespace Raycynix.Extensions.Security.Abstractions.Interfaces;

public interface ISecurityContext
{
    bool IsAuthenticated { get; }
    
    string SubjectId { get; }
    
    SecuritySubjectType SubjectType { get; }
    
    IReadOnlyCollection<string> Roles { get; }
    
    IReadOnlyCollection<string> Permissions { get; }
}