namespace Raycynix.Extensions.Serilog.Abstractions;

/// <summary>
/// Identifies a Serilog configurator that contributes an output sink.
/// </summary>
/// <remarks>
/// Sink configurators participate in fallback-console detection so the
/// fallback is only added when the pipeline has no other output destination.
/// </remarks>
public interface IRaycynixSerilogSinkConfigurator :
    IRaycynixSerilogConfigurator
{
    /// <summary>
    /// Gets whether this configurator currently contributes a sink.
    /// </summary>
    bool IsEnabled => true;
}
