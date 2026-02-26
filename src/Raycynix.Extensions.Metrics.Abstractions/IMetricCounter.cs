namespace Raycynix.Extensions.Metrics.Abstractions;

//TODO: Create Documentation

/// <summary>
/// 
/// </summary>
public interface IMetricCounter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="labelValues"></param>
    void Increment(double value = 1, params string[] labelValues);
}