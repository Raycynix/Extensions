using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Contracts.AspNetCore.Extensions;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Middleware;

/// <summary>
/// Writes endpoint contract metadata to the HTTP response headers when available.
/// </summary>
public sealed class ContractMetadataMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContractMetadataMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public ContractMetadataMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes the current HTTP request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that completes when the pipeline finishes.</returns>
    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.TryGetEndpointContractMetadata(out ContractMetadata? metadata))
        {
            context.Response.OnStarting(
                static state =>
                {
                    (HttpContext httpContext, ContractMetadata contractMetadata) = ((HttpContext, ContractMetadata))state;
                    httpContext.Response.WriteContractMetadata(contractMetadata);
                    return Task.CompletedTask;
                },
                (context, metadata!));
        }

        return _next(context);
    }
}
