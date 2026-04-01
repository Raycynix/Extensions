namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have at least one of the specified permissions before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequireAnyPermissionAttribute(params string[] permissions) : Attribute
{
    /// <summary>
    /// Gets the permissions of which at least one must be granted.
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; } =
        permissions.Where(static permission => !string.IsNullOrWhiteSpace(permission)).Select(static permission => permission.Trim()).ToArray();
}
