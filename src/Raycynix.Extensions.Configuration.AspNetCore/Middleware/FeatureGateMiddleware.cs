using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.AspNetCore.FeatureGate;

namespace Raycynix.Extensions.Configuration.AspNetCore.Middleware;

/// <summary>
/// Enforces endpoint feature-gate metadata during request execution.
/// </summary>
internal sealed class FeatureGateMiddleware(RequestDelegate next, IOptions<FeatureGateOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await next(context);
            return;
        }

        var gates = GetFeatureGates(endpoint);
        if (gates.Count == 0)
        {
            await next(context);
            return;
        }

        var featureFlags = context.RequestServices.GetService<IFeatureFlagAccessor>();
        if (featureFlags is null)
        {
            context.Response.StatusCode = options.Value.MissingFeatureFlagAccessorStatusCode;
            return;
        }

        if (gates.Any(gate => !IsSatisfied(gate, featureFlags)))
        {
            context.Response.StatusCode = options.Value.DisabledStatusCode;
            return;
        }

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