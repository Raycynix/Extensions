using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.AspNetCore.FeatureGate;

namespace Raycynix.Extensions.Configuration.AspNetCore.Middleware;

internal sealed class FeatureGateMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IFeatureFlagAccessor featureFlags)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is not null)
        {
            foreach (var gate in GetFeatureGates(endpoint))
            {
                if (!IsSatisfied(gate, featureFlags))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
            }
        }

        await next(context);
    }

    private static IReadOnlyCollection<FeatureGateMetadata> GetFeatureGates(Endpoint endpoint)
    {
        var metadata = new List<FeatureGateMetadata>();

        metadata.AddRange(endpoint.Metadata.GetOrderedMetadata<FeatureGateMetadata>());
        metadata.AddRange(endpoint.Metadata.GetOrderedMetadata<FeatureGateAttribute>().Select(static attribute => attribute.Metadata));

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
