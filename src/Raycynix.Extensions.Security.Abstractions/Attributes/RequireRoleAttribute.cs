namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have a specific role before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequireRoleAttribute(string role) : Attribute
{
    /// <summary>
    /// Gets the required role.
    /// </summary>
    public string Role { get; } = role;
}
