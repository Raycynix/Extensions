namespace Raycynix.Extensions.Messaging.Abstractions.Attributes;

/// <summary>
/// Requires the inbound message source header to match one of the supplied values before a messaging handler can execute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequireTrustedSourceAttribute(params string[] sources) : Attribute
{
    /// <summary>
    /// Gets the allowed inbound sources.
    /// </summary>
    public IReadOnlyCollection<string> Sources { get; } =
        sources.Where(static source => !string.IsNullOrWhiteSpace(source)).Select(static source => source.Trim()).ToArray();
}
