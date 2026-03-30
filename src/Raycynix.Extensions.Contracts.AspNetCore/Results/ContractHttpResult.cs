using Microsoft.AspNetCore.Http;
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
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCode;
        httpContext.Response.WriteContractMetadata(Metadata);

        object? payload = UseEnvelope
            ? new VersionedContract<TContract>
            {
                Metadata = Metadata,
                Payload = Value
            }
            : Value;

        return httpContext.Response.WriteAsJsonAsync(payload, cancellationToken: httpContext.RequestAborted);
    }
}
