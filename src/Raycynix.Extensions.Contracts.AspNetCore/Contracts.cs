using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Contracts.AspNetCore.Extensions;
using Raycynix.Extensions.Contracts.AspNetCore.Metadata;
using Raycynix.Extensions.Contracts.AspNetCore.Middleware;
using Raycynix.Extensions.Contracts.AspNetCore.Results;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore;

/// <summary>
/// Provides ASP.NET Core integration helpers for Raycynix contracts.
/// </summary>
public static class Contracts
{
    /// <summary>
    /// Adds Raycynix contract services required by ASP.NET Core integrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddRaycynixContractsAspNetCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }

    /// <summary>
    /// Adds the Raycynix contract metadata middleware to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The configured application builder.</returns>
    public static IApplicationBuilder UseRaycynixContracts(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<ContractMetadataMiddleware>();
    }

    /// <summary>
    /// Attaches contract metadata to an endpoint using a semantic version string.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint builder.</param>
    /// <param name="contractName">The canonical contract name.</param>
    /// <param name="contractVersion">The contract version string.</param>
    /// <returns>The same endpoint builder for chaining.</returns>
    public static TBuilder WithContract<TBuilder>(this TBuilder builder, string contractName, string contractVersion)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithContract(
            new ContractMetadata
            {
                Name = contractName,
                Version = ContractVersion.Parse(contractVersion)
            });
    }

    /// <summary>
    /// Attaches contract metadata to an endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint builder.</param>
    /// <param name="contractName">The canonical contract name.</param>
    /// <param name="contractVersion">The contract version.</param>
    /// <returns>The same endpoint builder for chaining.</returns>
    public static TBuilder WithContract<TBuilder>(
        this TBuilder builder,
        string contractName,
        ContractVersion contractVersion)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(contractVersion);

        return builder.WithContract(
            new ContractMetadata
            {
                Name = contractName,
                Version = contractVersion
            });
    }

    /// <summary>
    /// Attaches contract metadata to an endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint builder.</param>
    /// <param name="metadata">The contract metadata.</param>
    /// <returns>The same endpoint builder for chaining.</returns>
    public static TBuilder WithContract<TBuilder>(this TBuilder builder, ContractMetadata metadata)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(metadata);

        builder.Add(endpointBuilder => endpointBuilder.Metadata.Add(new ContractEndpointMetadata(metadata)));

        return builder;
    }

    /// <summary>
    /// Creates an HTTP result that writes contract headers and returns the payload as-is.
    /// </summary>
    /// <typeparam name="TContract">The payload type.</typeparam>
    /// <param name="payload">The payload value.</param>
    /// <param name="metadata">The contract metadata.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The configured contract result.</returns>
    public static ContractHttpResult<TContract> Contract<TContract>(
        TContract? payload,
        ContractMetadata metadata,
        int statusCode = StatusCodes.Status200OK)
    {
        return new ContractHttpResult<TContract>(payload, metadata, statusCode, useEnvelope: false);
    }

    /// <summary>
    /// Creates an HTTP result that uses the current endpoint contract metadata and returns the payload as-is.
    /// </summary>
    /// <typeparam name="TContract">The payload type.</typeparam>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="payload">The payload value.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The configured contract result.</returns>
    public static ContractHttpResult<TContract> Contract<TContract>(
        this HttpContext context,
        TContract? payload,
        int statusCode = StatusCodes.Status200OK)
    {
        ArgumentNullException.ThrowIfNull(context);

        return Contract(payload, context.GetRequiredEndpointContractMetadata(), statusCode);
    }

    /// <summary>
    /// Creates an HTTP result that uses the current endpoint contract metadata and returns the payload as-is.
    /// </summary>
    /// <typeparam name="TContract">The payload type.</typeparam>
    /// <param name="controller">The controller instance.</param>
    /// <param name="payload">The payload value.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The configured contract result.</returns>
    public static ContractHttpResult<TContract> Contract<TContract>(
        this ControllerBase controller,
        TContract? payload,
        int statusCode = StatusCodes.Status200OK)
    {
        ArgumentNullException.ThrowIfNull(controller);

        return controller.HttpContext.Contract(payload, statusCode);
    }

    /// <summary>
    /// Creates an HTTP result that writes contract headers and returns a versioned contract envelope.
    /// </summary>
    /// <typeparam name="TContract">The payload type.</typeparam>
    /// <param name="payload">The payload value.</param>
    /// <param name="metadata">The contract metadata.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The configured versioned contract result.</returns>
    public static ContractHttpResult<TContract> VersionedContract<TContract>(
        TContract? payload,
        ContractMetadata metadata,
        int statusCode = StatusCodes.Status200OK)
    {
        return new ContractHttpResult<TContract>(payload, metadata, statusCode, useEnvelope: true);
    }

    /// <summary>
    /// Creates an HTTP result that uses the current endpoint contract metadata and returns a versioned contract envelope.
    /// </summary>
    /// <typeparam name="TContract">The payload type.</typeparam>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="payload">The payload value.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The configured versioned contract result.</returns>
    public static ContractHttpResult<TContract> VersionedContract<TContract>(
        this HttpContext context,
        TContract? payload,
        int statusCode = StatusCodes.Status200OK)
    {
        ArgumentNullException.ThrowIfNull(context);

        return VersionedContract(payload, context.GetRequiredEndpointContractMetadata(), statusCode);
    }

    /// <summary>
    /// Creates an HTTP result that uses the current endpoint contract metadata and returns a versioned contract envelope.
    /// </summary>
    /// <typeparam name="TContract">The payload type.</typeparam>
    /// <param name="controller">The controller instance.</param>
    /// <param name="payload">The payload value.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The configured versioned contract result.</returns>
    public static ContractHttpResult<TContract> VersionedContract<TContract>(
        this ControllerBase controller,
        TContract? payload,
        int statusCode = StatusCodes.Status200OK)
    {
        ArgumentNullException.ThrowIfNull(controller);

        return controller.HttpContext.VersionedContract(payload, statusCode);
    }
}
