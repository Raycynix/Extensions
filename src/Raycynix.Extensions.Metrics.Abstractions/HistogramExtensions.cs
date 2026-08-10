using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Raycynix.Extensions.Metrics.Abstractions;

/// <summary>
/// Provides duration recording helpers for standard .NET histograms.
/// </summary>
public static class HistogramExtensions
{
    /// <summary>
    /// Measures elapsed time and records it in seconds when the returned handle is disposed.
    /// </summary>
    /// <param name="histogram">A histogram whose unit is seconds.</param>
    /// <param name="tags">Tags attached to the recorded measurement.</param>
    /// <returns>A handle that records the elapsed duration once.</returns>
    public static IDisposable MeasureDuration(
        this Histogram<double> histogram,
        params KeyValuePair<string, object?>[] tags)
    {
        ArgumentNullException.ThrowIfNull(histogram);
        ArgumentNullException.ThrowIfNull(tags);

        return new HistogramTimer(histogram, tags);
    }

    private sealed class HistogramTimer(
        Histogram<double> histogram,
        KeyValuePair<string, object?>[] tags) : IDisposable
    {
        private readonly long _startedAt = Stopwatch.GetTimestamp();
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            histogram.Record(Stopwatch.GetElapsedTime(_startedAt).TotalSeconds, tags);
        }
    }
}
