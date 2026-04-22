using FluentAssertions;
using Raycynix.Extensions.Secrets.Exceptions;
using Raycynix.Extensions.Secrets.Extensions;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Abstractions.Records;

namespace Raycynix.Extensions.Secrets.Tests.Service;

/// <summary>
/// Covers extension-based convenience APIs for secret resolution.
/// </summary>
public sealed class SecretResolverExtensionsTests
{
    /// <summary>
    /// Verifies that a required secret is returned when available.
    /// </summary>
    [Fact]
    public async Task GetRequiredSecretAsync_ShouldReturnValue_WhenSecretExists()
    {
        ISecretResolver resolver = new StaticResolver("resolved-secret");

        var result = await resolver.GetRequiredSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        result.Should().Be("resolved-secret");
    }

    /// <summary>
    /// Verifies that a required secret throws a dedicated exception when missing.
    /// </summary>
    [Fact]
    public async Task GetRequiredSecretAsync_ShouldThrow_WhenSecretIsMissing()
    {
        ISecretResolver resolver = new StaticResolver(null);

        var action = async () => await resolver.GetRequiredSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowAsync<SecretNotFoundException>();
        exception.Which.Key.Should().Be("Api:Token");
    }

    /// <summary>
    /// Verifies that required-secret resolution uses a single diagnostics pass on missing secrets.
    /// </summary>
    [Fact]
    public async Task GetRequiredSecretAsync_ShouldUseSingleDiagnosticsPass_WhenSecretIsMissing()
    {
        var resolver = new CountingDiagnosticsResolver();

        var action = async () => await resolver.GetRequiredSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        await action.Should().ThrowAsync<SecretNotFoundException>();
        resolver.DiagnoseCallCount.Should().Be(1);
        resolver.ResolveCallCount.Should().Be(0);
        resolver.ExplainCallCount.Should().Be(0);
    }

    private sealed class StaticResolver(string? value) : ISecretResolver
    {
        public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(value);
        }
    }

    private sealed class CountingDiagnosticsResolver : ISecretDiagnosticsResolver
    {
        public int DiagnoseCallCount { get; private set; }

        public int ResolveCallCount { get; private set; }

        public int ExplainCallCount { get; private set; }

        public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<string?>(null);
        }

        public ValueTask<SecretResolutionDiagnostics> DiagnoseSecretResolutionAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            DiagnoseCallCount++;
            return ValueTask.FromResult(new SecretResolutionDiagnostics(
                new SecretResolutionResult(key, Value: null, ProviderName: null),
                [new SecretResolutionAttempt("TestProvider", false)]));
        }

        public ValueTask<SecretResolutionResult> ResolveSecretAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            ResolveCallCount++;
            return ValueTask.FromResult(new SecretResolutionResult(key, Value: null, ProviderName: null));
        }

        public ValueTask<IReadOnlyList<SecretResolutionAttempt>> ExplainSecretResolutionAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            ExplainCallCount++;
            return ValueTask.FromResult<IReadOnlyList<SecretResolutionAttempt>>(
                [new SecretResolutionAttempt("TestProvider", false)]);
        }
    }
}
