namespace Raycynix.Extensions.Metrics.Abstractions;

//TODO: Create Documentation

/// <summary>
/// 
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="help"></param>
    /// <param name="labelNames"></param>
    /// <returns></returns>
    IMetricCounter CreateCounter(string name, string help, params string[] labelNames);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="help"></param>
    /// <param name="labelNames"></param>
    /// <returns></returns>
    IMetricGauge CreateGauge(string name, string help, params string[] labelNames);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="help"></param>
    /// <param name="labelNames"></param>
    /// <returns></returns>
    IMetricHistogram CreateHistogram(string name, string help, params string[] labelNames);
}