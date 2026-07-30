using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Contracts.Constants;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Extensions;

/// <summary>
/// Provides helpers for reading and writing transport-level contract headers.
/// </summary>
public static class ContractHeaderExtensions
{
    /// <summary>
    /// Writes contract metadata headers to the response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="metadata">The contract metadata to write.</param>
    public static void WriteContractMetadata(this HttpResponse response, ContractMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(metadata);

        if (!metadata.HasIdentity)
        {
            return;
        }

        response.Headers[ContractHeaders.ContractName] = metadata.Name;
        response.Headers[ContractHeaders.ContractVersion] = metadata.Version.ToString();
    }

    /// <summary>
    /// Attempts to read contract metadata from the request headers.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="metadata">The parsed contract metadata.</param>
    /// <returns><c>true</c> when the headers contain a valid contract identity; otherwise, <c>false</c>.</returns>
    public static bool TryGetContractMetadata(this HttpRequest request, out ContractMetadata? metadata)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Headers.TryGetContractMetadata(out metadata);
    }

    /// <summary>
    /// Attempts to read contract metadata from an HTTP header collection.
    /// </summary>
    /// <param name="headers">The header collection.</param>
    /// <param name="metadata">The parsed contract metadata.</param>
    /// <returns><c>true</c> when the headers contain a valid contract identity; otherwise, <c>false</c>.</returns>
    public static bool TryGetContractMetadata(this IHeaderDictionary headers, out ContractMetadata? metadata)
    {
        ArgumentNullException.ThrowIfNull(headers);

        metadata = null;

        var name = headers[ContractHeaders.ContractName].FirstOrDefault();
        var version = headers[ContractHeaders.ContractVersion].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(name) || !ContractVersion.TryParse(version, out var parsedVersion))
        {
            return false;
        }

        metadata = new ContractMetadata
        {
            Name = name.Trim(),
            Version = parsedVersion!
        };

        return metadata.HasIdentity;
    }
}
