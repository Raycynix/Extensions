using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Contracts.AspNetCore.Extensions;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Middleware;

/// <summary>
/// Writes endpoint contract metadata to the HTTP response headers when available.
/// </summary>
public sealed class ContractMetadataMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ContractMetadataMiddleware>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContractMetadataMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The optional logger.</param>
    public ContractMetadataMiddleware(
        RequestDelegate next,
        ILogger<ContractMetadataMiddleware>? logger = null)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes the current HTTP request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the pipeline finishes.</returns>
    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.TryGetEndpointContractMetadata(out var metadata))
        {
            _logger?.LogDebug(
                "Contract metadata found for endpoint {Endpoint}. Contract: {ContractName}, Version: {ContractVersion}.",
                context.GetEndpoint()?.DisplayName,
                metadata!.Name,
                metadata.Version);

            context.Response.OnStarting(
                static state =>
                {
                    var (httpContext, contractMetadata, logger) =
                        ((HttpContext, ContractMetadata, ILogger<ContractMetadataMiddleware>?))state;

                    httpContext.Response.WriteContractMetadata(contractMetadata);
                    logger?.LogDebug(
                        "Wrote contract metadata headers for endpoint {Endpoint}. Contract: {ContractName}, Version: {ContractVersion}.",
                        httpContext.GetEndpoint()?.DisplayName,
                        contractMetadata.Name,
                        contractMetadata.Version);

                    return Task.CompletedTask;
                },
                (context, metadata!, _logger));
        }
        else
        {
            _logger?.LogDebug(
                "No contract metadata found for endpoint {Endpoint}.",
                context.GetEndpoint()?.DisplayName);
        }

        return _next(context);
    }
}
