namespace Raycynix.Extensions.Security.Abstractions.Attributes;

/// <summary>
/// Requires an authenticated subject before an operation can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RequireAuthenticatedSubjectAttribute : Attribute
{
}
