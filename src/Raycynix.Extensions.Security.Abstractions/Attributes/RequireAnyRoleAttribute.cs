namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have at least one of the specified roles before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireAnyRoleAttribute : Attribute
{
    /// <summary>
    /// Initializes an any-role authorization requirement.
    /// </summary>
    public RequireAnyRoleAttribute(params string[] roles)
    {
        Roles = AuthorizationAttributeValues.RequiredMany(roles, nameof(roles));
    }

    /// <summary>
    /// Gets the roles of which at least one must be assigned.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; }
}
