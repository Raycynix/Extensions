using System.Diagnostics;

namespace Raycynix.Extensions.Tracing.Abstractions;

/// <summary>
/// Defines the shared activity source used by Raycynix instrumentation.
/// </summary>
public static class RaycynixTracing
{
    /// <summary>
    /// The stable name of the Raycynix activity source.
    /// </summary>
    public const string SourceName = "Raycynix.Extensions";

    /// <summary>
    /// The instrumentation version emitted by this release.
    /// </summary>
    public const string SourceVersion = "3.0.0";

    /// <summary>
    /// Gets the process-wide source used to create Raycynix activities.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new(SourceName, SourceVersion);
}
