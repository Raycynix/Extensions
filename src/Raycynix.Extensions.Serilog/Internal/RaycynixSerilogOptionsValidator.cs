using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Serilog.Configurations;

namespace Raycynix.Extensions.Serilog.Internal;

internal static class RaycynixSerilogOptionsValidator
{
    public static void ApplyDefaultsAndValidate(
        RaycynixSerilogOptions options,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);

        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            options.ServiceName =
                ApplicationMetadataResolver.ResolveServiceName(environment);
        }

        if (string.IsNullOrWhiteSpace(options.ServiceVersion))
        {
            options.ServiceVersion =
                ApplicationMetadataResolver.ResolveServiceVersion();
        }

        if (string.IsNullOrWhiteSpace(options.Environment))
        {
            options.Environment = environment.EnvironmentName;
        }

        options.ServiceName = options.ServiceName.Trim();
        options.ServiceVersion = options.ServiceVersion.Trim();
        options.Environment = options.Environment.Trim();
        options.SerilogSectionName = options.SerilogSectionName.Trim();

        Validate(options);
    }

    private static void Validate(RaycynixSerilogOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ServiceName))
        {
            throw new InvalidOperationException(
                "Raycynix Serilog service name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.ServiceVersion))
        {
            throw new InvalidOperationException(
                "Raycynix Serilog service version cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.Environment))
        {
            throw new InvalidOperationException(
                "Raycynix Serilog environment cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(options.SerilogSectionName))
        {
            throw new InvalidOperationException(
                "Native Serilog configuration section name cannot be empty.");
        }

        if (options.UseDefaultConsoleWhenNoSinksConfigured &&
            string.IsNullOrWhiteSpace(options.DefaultConsoleOutputTemplate))
        {
            throw new InvalidOperationException(
                "Default console output template cannot be empty " +
                "when the fallback console sink is enabled.");
        }
    }
}