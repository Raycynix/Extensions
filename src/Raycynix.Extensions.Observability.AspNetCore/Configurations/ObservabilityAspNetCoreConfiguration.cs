namespace Raycynix.Extensions.Observability.AspNetCore.Configurations;

/// <summary>
/// Configures ASP.NET Core-specific observability behavior.
/// </summary>
public sealed class ObservabilityAspNetCoreConfiguration
{
    /// <summary>
    /// Gets or sets whether resolved user and subject identifiers are added to the request logging scope.
    /// </summary>
    public bool IncludeIdentityInLoggingScope { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum accepted length of an incoming correlation identifier.
    /// </summary>
    public int MaxCorrelationIdLength { get; set; } = 128;
}
