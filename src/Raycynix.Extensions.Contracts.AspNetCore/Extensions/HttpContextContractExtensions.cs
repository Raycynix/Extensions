using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Contracts.AspNetCore.Attributes;
using Raycynix.Extensions.Contracts.AspNetCore.Metadata;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Extensions;

/// <summary>
/// Provides helpers for resolving contract metadata from the current HTTP context.
/// </summary>
public static class HttpContextContractExtensions
{
    internal const string EndpointContractMetadataItemKey = "__Raycynix.Contracts.EndpointMetadata";

    /// <param name="context">The HTTP context.</param>
    extension(HttpContext context)
    {
        /// <summary>
        /// Attempts to resolve the incoming contract metadata from request headers.
        /// </summary>
        /// <param name="metadata">The parsed request contract metadata.</param>
        /// <returns><c>true</c> when the request specifies a valid contract identity; otherwise, <c>false</c>.</returns>
        public bool TryGetRequestContractMetadata(out ContractMetadata? metadata)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Request.TryGetContractMetadata(out metadata);
        }

        /// <summary>
        /// Attempts to resolve the declared endpoint contract metadata.
        /// </summary>
        /// <param name="metadata">The endpoint contract metadata.</param>
        /// <returns><c>true</c> when endpoint contract metadata is available; otherwise, <c>false</c>.</returns>
        public bool TryGetEndpointContractMetadata(out ContractMetadata? metadata)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Items.TryGetValue(EndpointContractMetadataItemKey, out var value) &&
                value is ContractMetadata cachedMetadata)
            {
                metadata = cachedMetadata;
                return true;
            }

            var endpoint = context.GetEndpoint();
            if (endpoint is null)
            {
                metadata = null;
                return false;
            }

            metadata = endpoint.Metadata.GetMetadata<ContractEndpointMetadata>()?.Metadata
                       ?? endpoint.Metadata.GetMetadata<ContractAttribute>()?.Metadata;

            if (metadata?.HasIdentity == true)
            {
                context.Items[EndpointContractMetadataItemKey] = metadata;
                return true;
            }

            metadata = null;
            return false;
        }

        /// <summary>
        /// Resolves the declared endpoint contract metadata or throws when the current endpoint is not contract-aware.
        /// </summary>
        /// <returns>The endpoint contract metadata.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the current endpoint does not declare contract metadata.</exception>
        public ContractMetadata GetRequiredEndpointContractMetadata()
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.TryGetEndpointContractMetadata(out var metadata))
            {
                return metadata!;
            }

            throw new InvalidOperationException(
                "The current endpoint does not declare contract metadata. Add [Contract(...)] or .WithContract(...).");
        }
    }
}
