using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;

namespace Raycynix.Extensions.Logging;

/// <summary>
/// Provides extension methods for integrating Serilog logging into an application.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures the application to use Raycynix logging with Serilog.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="WebApplicationBuilder"/> instance used to configure the application.
    /// </param>
    /// <param name="serviceName">
    /// The name of the service, which will be included in log entries as a property.
    /// </param>
    /// <param name="configure">
    /// An optional action to configure additional Serilog settings.
    /// </param>
    public static void AddRaycynixLogging(
        this WebApplicationBuilder builder,
        string serviceName,
        Action<LoggerConfiguration>? configure = null)
    {
        builder.Host.UseSerilog((_, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Service", serviceName)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);

            if (builder.Environment.IsDevelopment())
            {
                loggerConfig.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}");
            }
            else
            {
                loggerConfig.WriteTo.Console(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter());
            }
            
            configure?.Invoke(loggerConfig);
        });
    }
}