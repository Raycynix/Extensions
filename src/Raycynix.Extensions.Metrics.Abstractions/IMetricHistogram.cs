namespace Raycynix.Extensions.Metrics.Abstractions;

//TODO: Create Documentation

/// <summary>
/// 
/// </summary>
public interface IMetricHistogram
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="labelValues"></param>
    void Observe(double value, params string[] labelValues);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="labelValues"></param>
    /// <returns></returns>
    IDisposable MeasureDuration(params string[] labelValues);
}