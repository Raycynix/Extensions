using System.Diagnostics.Metrics;

namespace Raycynix.Extensions.Metrics.Abstractions;

/// <summary>
/// Defines the shared meter identity used by Raycynix instrumentation.
/// </summary>
public static class RaycynixMetrics
{
    /// <summary>
    /// The name of the meter that publishes built-in Raycynix measurements.
    /// </summary>
    public const string MeterName = "Raycynix.Extensions";

    /// <summary>
    /// The instrumentation version emitted by this release.
    /// </summary>
    public const string MeterVersion = "3.0.0";

    /// <summary>
    /// Creates the shared Raycynix meter through the application meter factory.
    /// </summary>
    /// <param name="meterFactory">The application meter factory.</param>
    /// <returns>A meter managed by <paramref name="meterFactory"/>.</returns>
    public static Meter CreateMeter(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        return meterFactory.Create(new MeterOptions(MeterName)
        {
            Version = MeterVersion
        });
    }
}
