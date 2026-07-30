using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Contracts.AspNetCore.Extensions;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Results;

/// <summary>
/// Represents an ASP.NET Core result that emits Raycynix contract metadata headers.
/// </summary>
/// <typeparam name="TContract">The payload type.</typeparam>
public sealed class ContractHttpResult<TContract> : IResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContractHttpResult{TContract}"/> class.
    /// </summary>
    /// <param name="value">The payload value.</param>
    /// <param name="metadata">The associated contract metadata.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="useEnvelope">A value indicating whether to wrap the payload into a versioned envelope.</param>
    public ContractHttpResult(TContract? value, ContractMetadata metadata, int statusCode, bool useEnvelope)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (!metadata.HasIdentity)
        {
            throw new ArgumentException(
                "Contract metadata must contain a non-empty name and a valid semantic version.",
                nameof(metadata));
        }

        Value = value;
        Metadata = metadata;
        StatusCode = statusCode;
        UseEnvelope = useEnvelope;
    }

    /// <summary>
    /// Gets the payload value.
    /// </summary>
    public TContract? Value { get; }

    /// <summary>
    /// Gets the contract metadata.
    /// </summary>
    public ContractMetadata Metadata { get; }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets a value indicating whether the response body uses the versioned contract envelope.
    /// </summary>
    public bool UseEnvelope { get; }

    /// <inheritdoc />
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var logger = httpContext.RequestServices?.GetService<ILogger<ContractHttpResult<TContract>>>();
        logger?.LogDebug(
            "Executing contract HTTP result. Contract: {ContractName}, Version: {ContractVersion}, StatusCode: {StatusCode}, UsesEnvelope: {UsesEnvelope}.",
            Metadata.Name,
            Metadata.Version,
            StatusCode,
            UseEnvelope);

        httpContext.Response.StatusCode = StatusCode;
        httpContext.Response.WriteContractMetadata(Metadata);

        object? payload = UseEnvelope
            ? new VersionedContract<TContract>
            {
                Metadata = Metadata,
                Payload = Value
            }
            : Value;

        await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken: httpContext.RequestAborted);

        logger?.LogDebug(
            "Contract HTTP result completed. Contract: {ContractName}, Version: {ContractVersion}, StatusCode: {StatusCode}.",
            Metadata.Name,
            Metadata.Version,
            StatusCode);
    }
}
