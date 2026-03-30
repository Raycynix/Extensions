namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents metadata that identifies a shared contract.
/// </summary>
public class ContractMetadata
{
    /// <summary>
    /// Gets or sets the canonical contract name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contract version.
    /// </summary>
    public ContractVersion Version { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether the metadata contains a canonical name and a valid version.
    /// </summary>
    public bool HasIdentity => !string.IsNullOrWhiteSpace(Name) && Version.IsValid();

    /// <inheritdoc />
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Name) ? Version.ToString() : $"{Name}:{Version}";
    }
}
