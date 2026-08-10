using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Security.AspNetCore.Enums;
using Raycynix.Extensions.Security.AspNetCore.Options;
using Raycynix.Extensions.Security.AspNetCore.RateLimiting.Models;

namespace Raycynix.Extensions.Security.AspNetCore.Internal;

internal static class RaycynixRateLimiterOptions
{
    public static void Configure(
        Microsoft.AspNetCore.RateLimiting.RateLimiterOptions options,
        RateLimitOptions settings)
    {
        options.RejectionStatusCode = settings.RejectionStatusCode;
        options.OnRejected = async (context, cancellationToken) =>
        {
            var httpContext = context.HttpContext;
            var logger = httpContext.RequestServices.GetService<ILoggerFactory>()
                ?.CreateLogger("Raycynix.Extensions.Security.AspNetCore.RateLimiting");

            logger?.LogWarning(
                "Request rejected by rate limiting. Path={Path}, TraceId={TraceId}.",
                httpContext.Request.Path.Value,
                httpContext.TraceIdentifier);

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            if (settings.IncludeRetryAfterHeader &&
                context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
                httpContext.Response.Headers.RetryAfter =
                    retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
            }

            httpContext.Response.StatusCode = settings.RejectionStatusCode;
            httpContext.Response.ContentType = "application/json; charset=utf-8";

            var response = new RateLimitErrorResponse(
                Status: settings.RejectionStatusCode,
                Code: "rate_limit_exceeded",
                Message: "Too many requests.",
                TraceId: httpContext.TraceIdentifier);

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        };

        if (settings.GlobalPolicy is not null)
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context => CreatePartition(context, settings.GlobalPolicy));
        }

        foreach (var (policyName, policy) in settings.Policies)
        {
            options.AddPolicy(
                policyName,
                context => CreatePartition(context, policy));
        }
    }

    private static RateLimitPartition<string> CreatePartition(
        HttpContext context,
        RateLimitPolicyOptions policy)
    {
        var partitionKey = ResolvePartitionKey(context, policy.PartitionStrategy);

        return policy.Algorithm switch
        {
            RateLimitAlgorithm.FixedWindow => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    Window = policy.Window,
                    QueueLimit = policy.QueueLimit,
                    QueueProcessingOrder = policy.QueueProcessingOrder,
                    AutoReplenishment = policy.AutoReplenishment
                }),
            RateLimitAlgorithm.SlidingWindow => RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey,
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    Window = policy.Window,
                    SegmentsPerWindow = policy.SegmentsPerWindow,
                    QueueLimit = policy.QueueLimit,
                    QueueProcessingOrder = policy.QueueProcessingOrder,
                    AutoReplenishment = policy.AutoReplenishment
                }),
            RateLimitAlgorithm.TokenBucket => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey,
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = policy.PermitLimit,
                    ReplenishmentPeriod = policy.Window,
                    TokensPerPeriod = policy.TokensPerPeriod,
                    QueueLimit = policy.QueueLimit,
                    QueueProcessingOrder = policy.QueueProcessingOrder,
                    AutoReplenishment = policy.AutoReplenishment
                }),
            RateLimitAlgorithm.Concurrency => RateLimitPartition.GetConcurrencyLimiter(
                partitionKey,
                _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    QueueLimit = policy.QueueLimit,
                    QueueProcessingOrder = policy.QueueProcessingOrder
                }),
            _ => throw new InvalidOperationException($"Unsupported rate limit algorithm '{policy.Algorithm}'.")
        };
    }

    private static string ResolvePartitionKey(
        HttpContext context,
        RateLimitPartitionStrategy strategy)
    {
        return strategy switch
        {
            RateLimitPartitionStrategy.Global => "global",
            RateLimitPartitionStrategy.Subject => ResolveSubjectOrIpPartition(context),
            RateLimitPartitionStrategy.IpAddress => ResolveIpPartition(context),
            _ => throw new InvalidOperationException($"Unsupported rate limit partition strategy '{strategy}'.")
        };
    }

    private static string ResolveSubjectOrIpPartition(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return ResolveIpPartition(context);
        }

        var subject = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return string.IsNullOrWhiteSpace(subject)
            ? ResolveIpPartition(context)
            : $"subject:{subject}";
    }

    private static string ResolveIpPartition(HttpContext context)
    {
        return $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
    }
}
