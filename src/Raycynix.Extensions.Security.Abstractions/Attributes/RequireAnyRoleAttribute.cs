namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires the current subject to have at least one of the specified roles before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequireAnyRoleAttribute(params string[] roles) : Attribute
{
    /// <summary>
    /// Gets the roles of which at least one must be assigned.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; } =
        roles.Where(static role => !string.IsNullOrWhiteSpace(role)).Select(static role => role.Trim()).ToArray();
}
