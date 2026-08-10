namespace Raycynix.Extensions.Serilog;

/// <summary>
/// Provides standard structured property names used by Raycynix logging.
/// </summary>
public static class RaycynixSerilogPropertyNames
{
    /// <summary>
    /// Name of the service producing the event.
    /// </summary>
    public const string ServiceName = "ServiceName";

    /// <summary>
    /// Version of the service producing the event.
    /// </summary>
    public const string ServiceVersion = "ServiceVersion";

    /// <summary>
    /// Deployment environment of the service.
    /// </summary>
    public const string Environment = "Environment";
}