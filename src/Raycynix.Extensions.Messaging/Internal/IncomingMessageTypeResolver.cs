using Raycynix.Extensions.Contracts.Constants;
using Raycynix.Extensions.Contracts.Models;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Internal;

/// <summary>
/// Resolves incoming transport messages to registered payload types.
/// </summary>
internal sealed class IncomingMessageTypeResolver(
    IEnumerable<MessageHandlerRegistration> registrations,
    IMessageContractResolver contractResolver)
{
    private readonly Lazy<IReadOnlyList<MessageTypeMapping>> _mappings = new(() => registrations
        .Select(registration => registration.MessageType)
        .Distinct()
        .Select(messageType => new MessageTypeMapping(messageType, contractResolver.Resolve(messageType)))
        .ToArray());

    /// <summary>
    /// Resolves the payload type for an incoming message.
    /// </summary>
    /// <param name="message">The incoming transport message.</param>
    /// <returns>The resolved payload type.</returns>
    public Type Resolve(IncomingTransportMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.Headers.TryGetValue(ContractHeaders.ContractName, out var contractName))
        {
            var contractVersion = ResolveContractVersion(message.Headers);
            var match = _mappings.Value.FirstOrDefault(mapping =>
                string.Equals(mapping.Contract.Name, contractName, StringComparison.OrdinalIgnoreCase) &&
                Equals(mapping.Contract.Version, contractVersion));

            if (match is not null)
            {
                return match.MessageType;
            }
        }

        var destinationMatches = _mappings.Value
            .Where(mapping => string.Equals(mapping.Contract.Name, message.Destination, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (destinationMatches.Length == 1)
        {
            return destinationMatches[0].MessageType;
        }

        throw new InvalidOperationException(
            $"No registered message handler payload type matches incoming destination '{message.Destination}'.");
    }

    /// <summary>
    /// Resolves contract metadata from incoming headers when available.
    /// </summary>
    /// <param name="message">The incoming transport message.</param>
    /// <returns>The resolved contract metadata, or <see langword="null"/> when unavailable.</returns>
    public ContractMetadata? ResolveContract(IncomingTransportMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!message.Headers.TryGetValue(ContractHeaders.ContractName, out var contractName))
        {
            return null;
        }

        return new ContractMetadata
        {
            Name = contractName,
            Version = ResolveContractVersion(message.Headers)
        };
    }

    private static ContractVersion ResolveContractVersion(IReadOnlyDictionary<string, string> headers)
    {
        if (!headers.TryGetValue(ContractHeaders.ContractVersion, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return new ContractVersion();
        }

        return ContractVersion.Parse(value);
    }

    private sealed record MessageTypeMapping(Type MessageType, ContractMetadata Contract);
}
