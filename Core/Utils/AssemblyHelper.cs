using System.Reflection;

namespace Raycynix.Extensions.Core.Utils;

/// <summary>
/// 
/// </summary>
public static class AssemblyHelper
{
    /// <returns>the version of the current executing application, or <c>Unknown</c> if not set.</returns>
    public static string CurrentVersion() => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    
    /// <returns>the name of the current executing application, or <c>Unknown</c> if not set.</returns>
    public static string CurrentName() =>  Assembly.GetExecutingAssembly().GetName().Name ?? "Unknown";
}