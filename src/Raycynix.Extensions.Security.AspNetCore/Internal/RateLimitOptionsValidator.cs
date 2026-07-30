using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Security.AspNetCore.Enums;
using Raycynix.Extensions.Security.AspNetCore.Options;

namespace Raycynix.Extensions.Security.AspNetCore.Internal;

internal sealed class RateLimitOptionsValidator : IConfigurationValidator<RateLimitOptions>
{
    public ConfigurationValidationResult Validate(RateLimitOptions options)
    {
        var errors = new List<string>();

        if (options.RejectionStatusCode is < 400 or > 599)
        {
            errors.Add("RateLimitOptions.RejectionStatusCode must be between 400 and 599.");
        }

        ValidatePolicy(options.GlobalPolicy, "RateLimitOptions.GlobalPolicy", errors);

        if (options.Policies is null)
        {
            errors.Add("RateLimitOptions.Policies must be provided.");
        }
        else
        {
            foreach (var (name, policy) in options.Policies)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add("RateLimitOptions.Policies cannot contain an empty policy name.");
                    continue;
                }

                ValidatePolicy(policy, $"RateLimitOptions.Policies:{name}", errors);
            }
        }

        return errors.Count == 0
            ? ConfigurationValidationResult.Success()
            : ConfigurationValidationResult.Failure(errors);
    }

    private static void ValidatePolicy(
        RateLimitPolicyOptions? policy,
        string path,
        ICollection<string> errors)
    {
        if (policy is null)
        {
            return;
        }

        if (!Enum.IsDefined(policy.Algorithm))
        {
            errors.Add($"{path}.Algorithm is invalid.");
        }

        if (!Enum.IsDefined(policy.PartitionStrategy))
        {
            errors.Add($"{path}.PartitionStrategy is invalid.");
        }

        if (policy.PermitLimit <= 0)
        {
            errors.Add($"{path}.PermitLimit must be greater than zero.");
        }

        if (policy.QueueLimit < 0)
        {
            errors.Add($"{path}.QueueLimit cannot be negative.");
        }

        if (!Enum.IsDefined(policy.QueueProcessingOrder))
        {
            errors.Add($"{path}.QueueProcessingOrder is invalid.");
        }

        if (policy.Algorithm is not RateLimitAlgorithm.Concurrency && policy.Window <= TimeSpan.Zero)
        {
            errors.Add($"{path}.Window must be greater than zero.");
        }

        if (policy.Algorithm is RateLimitAlgorithm.SlidingWindow && policy.SegmentsPerWindow <= 0)
        {
            errors.Add($"{path}.SegmentsPerWindow must be greater than zero.");
        }

        if (policy.Algorithm is RateLimitAlgorithm.TokenBucket && policy.TokensPerPeriod <= 0)
        {
            errors.Add($"{path}.TokensPerPeriod must be greater than zero.");
        }
    }
}
