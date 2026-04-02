using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Resolves canonical contract metadata for messaging payload types.
/// </summary>
public interface IMessageContractResolver
{
    /// <summary>
    /// Resolves contract metadata for the specified payload type.
    /// </summary>
    /// <param name="messageType">The payload type.</param>
    /// <returns>The resolved contract metadata.</returns>
    ContractMetadata Resolve(Type messageType);
}
