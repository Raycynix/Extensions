using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Security.AspNetCore.Enums;
using Raycynix.Extensions.Security.AspNetCore.Options;

namespace Raycynix.Extensions.Security.AspNetCore.Tests.Registration;

/// <summary>
/// Covers configurable ASP.NET Core rate limiting registration.
/// </summary>
public class RateLimitRegistrationTests
{
    /// <summary>
    /// Verifies that rate limit settings and named policies are bound from configuration.
    /// </summary>
    [Fact]
    public void AddRaycynixRateLimiting_ShouldBindGlobalAndNamedPolicies()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimitOptions:RejectionStatusCode"] = "429",
                ["RateLimitOptions:IncludeRetryAfterHeader"] = "false",
                ["RateLimitOptions:GlobalPolicy:Algorithm"] = "SlidingWindow",
                ["RateLimitOptions:GlobalPolicy:PartitionStrategy"] = "Subject",
                ["RateLimitOptions:GlobalPolicy:PermitLimit"] = "20",
                ["RateLimitOptions:GlobalPolicy:Window"] = "00:00:30",
                ["RateLimitOptions:GlobalPolicy:SegmentsPerWindow"] = "3",
                ["RateLimitOptions:Policies:login:Algorithm"] = "TokenBucket",
                ["RateLimitOptions:Policies:login:PermitLimit"] = "5",
                ["RateLimitOptions:Policies:login:TokensPerPeriod"] = "1",
                ["RateLimitOptions:Policies:login:Window"] = "00:01:00"
            })
            .Build();

        services.AddRaycynixRateLimiting(configuration);
        using var provider = services.BuildServiceProvider();

        var settings = provider.GetRequiredService<RateLimitOptions>();
        var middlewareOptions = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

        settings.GlobalPolicy.Should().NotBeNull();
        settings.GlobalPolicy!.Algorithm.Should().Be(RateLimitAlgorithm.SlidingWindow);
        settings.GlobalPolicy.PartitionStrategy.Should().Be(RateLimitPartitionStrategy.Subject);
        settings.GlobalPolicy.PermitLimit.Should().Be(20);
        settings.Policies.Should().ContainKey("login");
        settings.Policies["login"].Algorithm.Should().Be(RateLimitAlgorithm.TokenBucket);
        middlewareOptions.RejectionStatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        middlewareOptions.GlobalLimiter.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the configured global limiter creates independent buckets by IP address.
    /// </summary>
    [Fact]
    public void AddRaycynixRateLimiting_ShouldApplyConfiguredIpPartition()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimitOptions:GlobalPolicy:Algorithm"] = "FixedWindow",
                ["RateLimitOptions:GlobalPolicy:PartitionStrategy"] = "IpAddress",
                ["RateLimitOptions:GlobalPolicy:PermitLimit"] = "1",
                ["RateLimitOptions:GlobalPolicy:Window"] = "01:00:00"
            })
            .Build();

        services.AddRaycynixRateLimiting(configuration);
        using var provider = services.BuildServiceProvider();
        var limiter = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value.GlobalLimiter!;

        var firstContext = CreateHttpContext("192.0.2.1");
        var secondContext = CreateHttpContext("192.0.2.2");

        using var firstLease = limiter.AttemptAcquire(firstContext);
        using var rejectedLease = limiter.AttemptAcquire(firstContext);
        using var otherPartitionLease = limiter.AttemptAcquire(secondContext);

        firstLease.IsAcquired.Should().BeTrue();
        rejectedLease.IsAcquired.Should().BeFalse();
        otherPartitionLease.IsAcquired.Should().BeTrue();
    }

    [Fact]
    public void AddRaycynixRateLimiting_ShouldIgnoreSubjectClaimsFromUnauthenticatedPrincipals()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddRaycynixRateLimiting(configuration, options =>
        {
            options.GlobalPolicy!.Algorithm = RateLimitAlgorithm.FixedWindow;
            options.GlobalPolicy.PartitionStrategy = RateLimitPartitionStrategy.Subject;
            options.GlobalPolicy.PermitLimit = 1;
            options.GlobalPolicy.Window = TimeSpan.FromHours(1);
        });
        using var provider = services.BuildServiceProvider();
        var limiter = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value.GlobalLimiter!;
        var firstContext = CreateHttpContext("192.0.2.1");
        firstContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(JwtRegisteredClaimNames.Sub, "attacker-a")]));
        var secondContext = CreateHttpContext("192.0.2.1");
        secondContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(JwtRegisteredClaimNames.Sub, "attacker-b")]));

        using var firstLease = limiter.AttemptAcquire(firstContext);
        using var secondLease = limiter.AttemptAcquire(secondContext);

        firstLease.IsAcquired.Should().BeTrue();
        secondLease.IsAcquired.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that every supported algorithm creates a usable global limiter.
    /// </summary>
    [Theory]
    [InlineData(RateLimitAlgorithm.FixedWindow)]
    [InlineData(RateLimitAlgorithm.SlidingWindow)]
    [InlineData(RateLimitAlgorithm.TokenBucket)]
    [InlineData(RateLimitAlgorithm.Concurrency)]
    public void AddRaycynixRateLimiting_ShouldCreateSupportedAlgorithm(RateLimitAlgorithm algorithm)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddRaycynixRateLimiting(configuration, options =>
        {
            options.GlobalPolicy!.Algorithm = algorithm;
            options.GlobalPolicy.PermitLimit = 1;
            options.GlobalPolicy.Window = TimeSpan.FromMinutes(1);
            options.GlobalPolicy.SegmentsPerWindow = 2;
            options.GlobalPolicy.TokensPerPeriod = 1;
        });
        using var provider = services.BuildServiceProvider();
        var limiter = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value.GlobalLimiter!;

        using var lease = limiter.AttemptAcquire(CreateHttpContext("192.0.2.1"));

        lease.IsAcquired.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that invalid limiter values are reported through options validation.
    /// </summary>
    [Fact]
    public void AddRaycynixRateLimiting_ShouldRejectInvalidConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimitOptions:GlobalPolicy:PermitLimit"] = "0",
                ["RateLimitOptions:GlobalPolicy:Window"] = "00:00:00"
            })
            .Build();

        services.AddRaycynixRateLimiting(configuration);
        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<RateLimitOptions>();

        action.Should().Throw<OptionsValidationException>()
            .WithMessage("*RateLimitOptions.GlobalPolicy.PermitLimit must be greater than zero*")
            .WithMessage("*RateLimitOptions.GlobalPolicy.Window must be greater than zero*");
    }

    private static DefaultHttpContext CreateHttpContext(string address)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(address);
        return context;
    }
}
