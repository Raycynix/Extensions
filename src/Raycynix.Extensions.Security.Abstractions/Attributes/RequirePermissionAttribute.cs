namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have a specific permission before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute : Attribute
{
    /// <summary>
    /// Initializes a permission authorization requirement.
    /// </summary>
    public RequirePermissionAttribute(string permission)
    {
        Permission = AuthorizationAttributeValues.Required(permission, nameof(permission));
    }

    /// <summary>
    /// Gets the required permission.
    /// </summary>
    public string Permission { get; }
}
