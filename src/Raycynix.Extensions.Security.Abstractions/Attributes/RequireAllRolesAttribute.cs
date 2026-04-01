namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have all of the specified roles before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequireAllRolesAttribute(params string[] roles) : Attribute
{
    /// <summary>
    /// Gets the roles that must all be assigned.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; } =
        roles.Where(static role => !string.IsNullOrWhiteSpace(role)).Select(static role => role.Trim()).ToArray();
}
