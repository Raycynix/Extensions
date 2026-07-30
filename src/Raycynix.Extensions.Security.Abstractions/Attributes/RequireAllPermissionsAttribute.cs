namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have all of the specified permissions before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireAllPermissionsAttribute : Attribute
{
    /// <summary>
    /// Initializes an all-permissions authorization requirement.
    /// </summary>
    public RequireAllPermissionsAttribute(params string[] permissions)
    {
        Permissions = AuthorizationAttributeValues.RequiredMany(permissions, nameof(permissions));
    }

    /// <summary>
    /// Gets the permissions that must all be granted.
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; }
}
