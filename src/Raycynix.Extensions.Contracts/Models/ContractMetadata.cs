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
}
