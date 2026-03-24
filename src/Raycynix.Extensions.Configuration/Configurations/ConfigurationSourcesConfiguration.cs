using System.Reflection;

namespace Raycynix.Extensions.Configuration.Configurations;

/// <summary>
/// Describes the standard Raycynix configuration sources and their registration behavior.
/// </summary>
public sealed class ConfigurationSourcesConfiguration
{
    /// <summary>
    /// Gets or sets the base path used for JSON configuration files.
    /// Defaults to <see cref="AppContext.BaseDirectory"/>.
    /// </summary>
    public string BasePath { get; set; } = AppContext.BaseDirectory;

    /// <summary>
    /// Gets or sets the application environment name used for environment-specific JSON files.
    /// Defaults to <c>Production</c>.
    /// </summary>
    public string EnvironmentName { get; set; } = "Production";

    /// <summary>
    /// Gets or sets the base configuration name without file extension.
    /// Defaults to <c>appsettings</c>.
    /// </summary>
    public string BaseFileName { get; set; } = "appsettings";

    /// <summary>
    /// Gets or sets a value indicating whether the base JSON file is optional.
    /// </summary>
    public bool BaseJsonOptional { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether configuration files should reload on change.
    /// </summary>
    public bool ReloadOnChange { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether user secrets should be included.
    /// </summary>
    public bool IncludeUserSecrets { get; set; }

    /// <summary>
    /// Gets or sets the assembly used for resolving user secrets metadata.
    /// </summary>
    public Assembly? UserSecretsAssembly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user secrets source is optional.
    /// </summary>
    public bool UserSecretsOptional { get; set; } = true;

    /// <summary>
    /// Gets or sets the command-line arguments to include as a configuration source.
    /// </summary>
    public string[] CommandLineArguments { get; set; } = [];
}
