namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a semantic contract version.
/// </summary>
public class ContractVersion
{
    /// <summary>
    /// Gets or sets the major version.
    /// </summary>
    public int Major { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minor version.
    /// </summary>
    public int Minor { get; set; }

    /// <summary>
    /// Gets or sets the patch version.
    /// </summary>
    public int Patch { get; set; }

    /// <summary>
    /// Returns the version as a semantic version string.
    /// </summary>
    /// <returns>The semantic version string.</returns>
    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}";
    }
}
