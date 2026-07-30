namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have at least one of the specified permissions before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireAnyPermissionAttribute : Attribute
{
    /// <summary>
    /// Initializes an any-permission authorization requirement.
    /// </summary>
    public RequireAnyPermissionAttribute(params string[] permissions)
    {
        Permissions = AuthorizationAttributeValues.RequiredMany(permissions, nameof(permissions));
    }

    /// <summary>
    /// Gets the permissions of which at least one must be granted.
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; }
}
