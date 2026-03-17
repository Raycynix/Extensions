namespace Raycynix.Extensions.Common.Helpers;

/// <summary>
/// Provides helper methods for reading the current ASP.NET Core environment.
/// </summary>
public static class EnvironmentHelper
{
    /// <summary>
    /// Determines whether the current environment is <c>Development</c>.
    /// </summary>
    /// <returns><c>true</c> when the current environment is <c>Development</c>.</returns>
    public static bool IsDevelopment() =>
        string.Equals(CurrentEnvironment(), "Development", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the current environment is <c>Production</c>.
    /// </summary>
    /// <returns><c>true</c> when the current environment is <c>Production</c>.</returns>
    public static bool IsProduction() =>
        string.Equals(CurrentEnvironment(), "Production", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns the current environment name.
    /// </summary>
    /// <returns>
    /// The value of <c>ASPNETCORE_ENVIRONMENT</c> or <c>DOTNET_ENVIRONMENT</c>,
    /// or <c>Unknown</c> when neither variable is set.
    /// </returns>
    public static string CurrentEnvironment()
        => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
           ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
           ?? "Unknown";
}
