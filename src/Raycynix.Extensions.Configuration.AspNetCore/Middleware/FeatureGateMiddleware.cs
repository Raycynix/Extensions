using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.AspNetCore.FeatureGate;

namespace Raycynix.Extensions.Configuration.AspNetCore.Middleware;

/// <summary>
/// Enforces endpoint feature-gate metadata during request execution.
/// </summary>
internal sealed class FeatureGateMiddleware(
    RequestDelegate next,
    IOptions<FeatureGateOptions> options,
    ILogger<FeatureGateMiddleware>? logger = null)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger?.LogDebug("Checking feature gates for request.");
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            logger?.LogDebug("No endpoint found for request. Skipping feature gate evaluation.");
            await next(context);
            return;
        }

        logger?.LogDebug("Checking feature gates for endpoint {Endpoint}.", endpoint.DisplayName);
        var gates = GetFeatureGates(endpoint);
        if (gates.Count == 0)
        {
            logger?.LogDebug("No feature gates found for endpoint {Endpoint}.", endpoint.DisplayName);
            await next(context);
            return;
        }

        logger?.LogDebug("Found {Count} feature gates for endpoint {Endpoint}.", gates.Count, endpoint.DisplayName);
        logger?.LogDebug("Checking feature flags accessors.");

        var featureFlags = context.RequestServices.GetService<IFeatureFlagAccessor>();
        if (featureFlags is null)
        {
            logger?.LogWarning(
                "Feature-gated endpoint {Endpoint} cannot be evaluated because no feature flag accessor is registered.",
                endpoint.DisplayName);
            context.Response.StatusCode = options.Value.MissingFeatureFlagAccessorStatusCode;
            return;
        }

        logger?.LogDebug("Found feature flag accessor.");
        logger?.LogDebug("Checking feature gates for enabling.");

        if (gates.Any(gate => !IsSatisfied(gate, featureFlags)))
        {
            logger?.LogInformation(
                "Feature-gated endpoint {Endpoint} was blocked because one or more required feature flags are disabled.",
                endpoint.DisplayName);
            context.Response.StatusCode = options.Value.DisabledStatusCode;
            return;
        }

        logger?.LogDebug("All feature gates are enabled.");

        await next(context);
    }

    private static IReadOnlyCollection<FeatureGateMetadata> GetFeatureGates(Endpoint endpoint)
    {
        var metadata = new List<FeatureGateMetadata>();

        metadata.AddRange(endpoint.Metadata.GetOrderedMetadata<FeatureGateMetadata>());
        metadata.AddRange(endpoint.Metadata.GetOrderedMetadata<FeatureGateAttribute>()
            .Select(static attribute => attribute.Metadata));

        return metadata;
    }

    private static bool IsSatisfied(FeatureGateMetadata gate, IFeatureFlagAccessor featureFlags)
    {
        return gate.Mode switch
        {
            FeatureGateMode.All => gate.FeatureFlags.All(featureFlags.IsEnabled),
            FeatureGateMode.Any => gate.FeatureFlags.Any(featureFlags.IsEnabled),
            _ => false
        };
    }
}
