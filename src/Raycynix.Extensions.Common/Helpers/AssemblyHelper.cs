using System.Reflection;

namespace Raycynix.Extensions.Common.Helpers;

/// <summary>
/// 
/// </summary>
public static class AssemblyHelper
{
    /// <returns>the version of the current executing application, or <c>Unknown</c> if not set.</returns>
    public static string CurrentVersion() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown Version";
    
    /// <returns>the name of the current executing application, or <c>Unknown Version</c> if not set.</returns>
    public static string CurrentName() =>  Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown Service";
}