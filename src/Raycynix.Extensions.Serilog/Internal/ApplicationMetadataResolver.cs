using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace Raycynix.Extensions.Serilog.Internal;

internal static class ApplicationMetadataResolver
{
    public static string ResolveServiceName(IHostEnvironment environment)
    {
        if (!string.IsNullOrWhiteSpace(environment.ApplicationName))
        {
            return environment.ApplicationName;
        }

        var assembly = ResolveAssembly();

        return assembly.GetName().Name
               ?? "Application";
    }

    public static string ResolveServiceVersion()
    {
        var assembly = ResolveAssembly();

        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (string.IsNullOrWhiteSpace(informationalVersion))
            return assembly.GetName().Version?.ToString()
                   ?? "Unknown";
        
        var metadataSeparatorIndex =
            informationalVersion.IndexOf('+', StringComparison.Ordinal);

        return metadataSeparatorIndex >= 0
            ? informationalVersion[..metadataSeparatorIndex]
            : informationalVersion;

    }

    private static Assembly ResolveAssembly()
    {
        return Assembly.GetEntryAssembly()
               ?? Assembly.GetExecutingAssembly();
    }
}