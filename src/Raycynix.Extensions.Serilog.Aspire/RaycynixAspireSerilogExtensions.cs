using Aspire.Hosting;

namespace Raycynix.Extensions.Serilog.Aspire;

/// <summary>
/// Provides Raycynix Serilog registration for Aspire AppHost applications.
/// </summary>
public static class RaycynixAspireSerilogExtensions
{
    /// <summary>
    /// Adds Raycynix Serilog to the Aspire AppHost process.
    /// </summary>
    /// <remarks>
    /// This configures logging for the AppHost itself. Each orchestrated .NET
    /// project must register Raycynix Serilog in its own host process.
    /// </remarks>
    public static IDistributedApplicationBuilder AddRaycynixSerilog(
        this IDistributedApplicationBuilder builder,
        Action<RaycynixSerilogBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddRaycynixSerilog(
            builder.Configuration,
            builder.Environment,
            configure);

        return builder;
    }
}
