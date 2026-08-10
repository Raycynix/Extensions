namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have a specific role before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireRoleAttribute : Attribute
{
    /// <summary>
    /// Initializes a role authorization requirement.
    /// </summary>
    public RequireRoleAttribute(string role)
    {
        Role = AuthorizationAttributeValues.Required(role, nameof(role));
    }

    /// <summary>
    /// Gets the required role.
    /// </summary>
    public string Role { get; }
}
