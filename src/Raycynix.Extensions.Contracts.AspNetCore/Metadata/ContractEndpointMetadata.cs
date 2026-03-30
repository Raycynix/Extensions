using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Metadata;

/// <summary>
/// Represents endpoint-level contract metadata for ASP.NET Core routing.
/// </summary>
public sealed class ContractEndpointMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContractEndpointMetadata"/> class.
    /// </summary>
    /// <param name="metadata">The contract metadata.</param>
    public ContractEndpointMetadata(ContractMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        Metadata = metadata;
    }

    /// <summary>
    /// Gets the contract metadata associated with the endpoint.
    /// </summary>
    public ContractMetadata Metadata { get; }
}
