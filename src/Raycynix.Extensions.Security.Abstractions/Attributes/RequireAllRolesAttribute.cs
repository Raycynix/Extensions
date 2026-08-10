namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have all of the specified roles before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireAllRolesAttribute : Attribute
{
    /// <summary>
    /// Initializes an all-roles authorization requirement.
    /// </summary>
    public RequireAllRolesAttribute(params string[] roles)
    {
        Roles = AuthorizationAttributeValues.RequiredMany(roles, nameof(roles));
    }

    /// <summary>
    /// Gets the roles that must all be assigned.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; }
}
