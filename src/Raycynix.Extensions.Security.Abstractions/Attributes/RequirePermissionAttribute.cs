namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have a specific permission before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute(string permission) : Attribute
{
    /// <summary>
    /// Gets the required permission.
    /// </summary>
    public string Permission { get; } = permission;
}
