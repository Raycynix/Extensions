namespace Raycynix.Extensions.Contracts.Models;

/// <summary>
/// Represents a versioned contract envelope.
/// </summary>
/// <typeparam name="TContract">The payload type.</typeparam>
public class VersionedContract<TContract>
{
    /// <summary>
    /// Gets or sets the contract metadata.
    /// </summary>
    public ContractMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the contract payload.
    /// </summary>
    public TContract? Payload { get; set; }
}
