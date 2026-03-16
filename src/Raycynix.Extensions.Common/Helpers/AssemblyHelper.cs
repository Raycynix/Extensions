using System.Reflection;

namespace Raycynix.Extensions.Common.Helpers;

/// <summary>
/// Provides helper methods for retrieving assembly-related information such as the name and version of the current application.
/// </summary>
public static class AssemblyHelper
{
    /// <returns>the version of the current executing application, or <c>Unknown</c> if not set.</returns>
    public static string CurrentVersion() => ResolveAssembly().GetName().Version?.ToString() ?? "Unknown Version";
    
    /// <returns>the name of the current executing application, or <c>Unknown Version</c> if not set.</returns>
    public static string CurrentName() =>  ResolveAssembly().GetName().Name ?? "Unknown Service";
    
    private static Assembly ResolveAssembly() => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
}