using System.Reflection;

namespace Raycynix.Extensions.Common.Helpers;

/// <summary>
/// Provides helper methods for resolving the current application assembly metadata.
/// </summary>
public static class AssemblyHelper
{
    /// <summary>
    /// Returns the version of the entry assembly, or the executing assembly when
    /// the entry assembly is unavailable.
    /// </summary>
    /// <returns>The assembly version, or <c>Unknown Version</c> when it is not defined.</returns>
    public static string CurrentVersion() => ResolveAssembly().GetName().Version?.ToString() ?? "Unknown Version";
    
    /// <summary>
    /// Returns the name of the entry assembly, or the executing assembly when
    /// the entry assembly is unavailable.
    /// </summary>
    /// <returns>The assembly name, or <c>Unknown Service</c> when it is not defined.</returns>
    public static string CurrentName() =>  ResolveAssembly().GetName().Name ?? "Unknown Service";
    
    private static Assembly ResolveAssembly() => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
}
