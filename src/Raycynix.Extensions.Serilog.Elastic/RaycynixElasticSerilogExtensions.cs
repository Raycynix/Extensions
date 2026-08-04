using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Serilog.Elastic.Configurations;
using Raycynix.Extensions.Serilog.Elastic.Internal;

namespace Raycynix.Extensions.Serilog.Elastic;

/// <summary>
/// Provides Elastic integration extensions for Raycynix Serilog.
/// </summary>
public static class RaycynixElasticSerilogExtensions
{
    /// <summary>
    /// Adds the official Elastic Serilog sink to the Raycynix pipeline.
    /// </summary>
    /// <param name="builder">
    /// The Raycynix Serilog builder.
    /// </param>
    /// <param name="configure">
    /// Optional callback for overriding Elastic settings in code.
    /// </param>
    /// <returns>The same Raycynix Serilog builder.</returns>
    public static RaycynixSerilogBuilder AddElastic(
        this RaycynixSerilogBuilder builder,
        Action<ElasticSerilogOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        EnsureNotRegistered(builder.Services);

        var options = new ElasticSerilogOptions();

        builder.Configuration
            .GetSection(ElasticSerilogOptions.SectionName)
            .Bind(options);

        configure?.Invoke(options);

        ElasticSerilogOptionsValidator.Validate(options);

        builder.Services.AddSingleton(options);

        builder.Services.AddSingleton<ElasticSerilogRegistrationMarker>();

        builder.AddSinkConfigurator<ElasticSerilogConfigurator>();

        return builder;
    }

    private static void EnsureNotRegistered(IServiceCollection services)
    {
        var alreadyRegistered = services.Any(descriptor =>
            descriptor.ServiceType ==
            typeof(ElasticSerilogRegistrationMarker));

        if (alreadyRegistered)
            throw new InvalidOperationException("Raycynix Elastic Serilog integration has already been registered.");
    }
}
