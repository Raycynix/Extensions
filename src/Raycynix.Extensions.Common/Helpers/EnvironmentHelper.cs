namespace Raycynix.Extensions.Common.Helpers;

/// <summary>
/// Provides helper methods for determining the current application environment.
/// </summary>
public static class EnvironmentHelper
{
    /// <returns><c>true</c> if the current environment is <b>Development</b></returns>
    public static bool IsDevelopment() =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    /// <returns><c>true</c> if the current environment is <b>Production</b></returns>
    public static bool IsProduction() =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

    /// <returns>the name of the current application environment, or <c>Unknown</c> if not set.</returns>
    public static string CurrentEnvironment()
        => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
}