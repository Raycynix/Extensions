using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration;

/// <summary>
/// Provides helper extensions for working with standard Raycynix environment names.
/// </summary>
public static class EnvironmentExtensions
{
    /// <param name="environment">The environment abstraction to evaluate.</param>
    extension(IApplicationEnvironment environment)
    {
        /// <summary>
        /// Determines whether the current environment is <c>Development</c>.
        /// </summary>
        /// <returns><c>true</c> when the environment is <c>Development</c>.</returns>
        public bool IsDevelopment()
        {
            ArgumentNullException.ThrowIfNull(environment);
            return environment.IsDevelopment;
        }

        /// <summary>
        /// Determines whether the current environment is <c>Testing</c>.
        /// </summary>
        /// <returns><c>true</c> when the environment is <c>Testing</c>.</returns>
        public bool IsTesting()
        {
            ArgumentNullException.ThrowIfNull(environment);
            return environment.IsTesting;
        }

        /// <summary>
        /// Determines whether the current environment is <c>Staging</c>.
        /// </summary>
        /// <returns><c>true</c> when the environment is <c>Staging</c>.</returns>
        public bool IsStaging()
        {
            ArgumentNullException.ThrowIfNull(environment);
            return environment.IsStaging;
        }

        /// <summary>
        /// Determines whether the current environment is <c>Production</c>.
        /// </summary>
        /// <returns><c>true</c> when the environment is <c>Production</c>.</returns>
        public bool IsProduction()
        {
            ArgumentNullException.ThrowIfNull(environment);
            return environment.IsProduction;
        }
    }

    /// <summary>
    /// Determines whether the host environment is <c>Testing</c>.
    /// </summary>
    /// <param name="environment">The host environment abstraction to evaluate.</param>
    /// <returns><c>true</c> when the environment is <c>Testing</c>.</returns>
    public static bool IsTesting(this IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        return string.Equals(environment.EnvironmentName, EnvironmentNames.Testing, StringComparison.OrdinalIgnoreCase);
    }
}
