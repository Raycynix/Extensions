namespace Core.Utils
{
    /// <summary>
    /// Provides helper methods for determining the current application environment.
    /// </summary>
    public static class EnvironmentHelper
    {
        /// <summary>
        /// Returns <c>true</c> if the current environment is Development.
        /// </summary>
        public static bool IsDevelopment() =>
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        /// <summary>
        /// Returns <c>true</c> if the current environment is Production.
        /// </summary>
        public static bool IsProduction() =>
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

        /// <summary>
        /// Returns the name of the current ASP.NET Core environment, or "Unknown" if not set.
        /// </summary>
        public static string CurrentEnvironment()
            => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
    }
}
