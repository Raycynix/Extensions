using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Contracts.AspNetCore.Extensions;
using Raycynix.Extensions.Contracts.Constants;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Tests.Extensions;

/// <summary>
/// Covers request and response contract header helpers.
/// </summary>
public sealed class ContractHeaderExtensionsTests
{
    /// <summary>
    /// Verifies that response headers are written for valid contract identities.
    /// </summary>
    [Fact]
    public void WriteContractMetadata_ShouldWriteExpectedHeaders()
    {
        var context = new DefaultHttpContext();
        var metadata = new ContractMetadata
        {
            Name = "catalog.prices",
            Version = ContractVersion.Parse("1.2.0")
        };

        context.Response.WriteContractMetadata(metadata);

        context.Response.Headers[ContractHeaders.ContractName].ToString().Should().Be("catalog.prices");
        context.Response.Headers[ContractHeaders.ContractVersion].ToString().Should().Be("1.2.0");
    }

    /// <summary>
    /// Verifies that request headers can be parsed back into contract metadata.
    /// </summary>
    [Fact]
    public void TryGetContractMetadata_ShouldParseExpectedHeaders()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[ContractHeaders.ContractName] = " catalog.prices ";
        context.Request.Headers[ContractHeaders.ContractVersion] = "1.2.0";

        var parsed = context.Request.TryGetContractMetadata(out var metadata);

        parsed.Should().BeTrue();
        metadata.Should().NotBeNull();
        metadata!.Name.Should().Be("catalog.prices");
        metadata.Version.Should().BeEquivalentTo(new ContractVersion { Major = 1, Minor = 2, Patch = 0 });
    }
}
