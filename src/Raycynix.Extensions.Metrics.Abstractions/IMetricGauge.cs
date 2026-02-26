namespace Raycynix.Extensions.Metrics.Abstractions;

//TODO: Create Documentation

/// <summary>
/// 
/// </summary>
public interface IMetricGauge
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="labelValues"></param>
    void Set(double value, params string[] labelValues);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="labelValues"></param>
    void Increment(double value = 1, params string[] labelValues);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="labelValues"></param>
    void Decrement(double value = 1, params string[] labelValues);
}