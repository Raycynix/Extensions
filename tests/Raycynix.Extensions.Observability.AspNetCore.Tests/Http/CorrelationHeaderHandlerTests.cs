using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Observability.AspNetCore.Http;

namespace Raycynix.Extensions.Observability.AspNetCore.Tests.Http;

/// <summary>
/// Covers outgoing correlation header propagation for HTTP client requests.
/// </summary>
public class CorrelationHeaderHandlerTests
{
    internal const string CorrelationHeader = "X-Correlation-ID";

    /// <summary>
    /// Verifies that the outgoing request keeps the correlation identifier
    /// from the incoming HTTP header when it is already present.
    /// </summary>
    [Fact]
    public async Task SendAsync_ShouldUseIncomingHeaderValue_WhenItExists()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        httpContextAccessor.HttpContext.Request.Headers[CorrelationHeader] = "incoming-correlation";

        using var handler = new TestCorrelationHeaderHandler(httpContextAccessor, new CaptureHandler());
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test");

        var response = await handler.SendAsyncPublic(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.GetValues(CorrelationHeader).Should().ContainSingle("incoming-correlation");
    }

    /// <summary>
    /// Verifies that the outgoing request falls back to the ambient operation context
    /// when no incoming correlation header is available.
    /// </summary>
    [Fact]
    public async Task SendAsync_ShouldUseOperationContextCorrelationId_WhenIncomingHeaderIsMissing()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var previous = OperationContext.Current;
        OperationContext.Current = new OperationContext
        {
            CorrelationId = "operation-correlation"
        };

        try
        {
            using var handler = new TestCorrelationHeaderHandler(httpContextAccessor, new CaptureHandler());
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test");

            await handler.SendAsyncPublic(request, TestContext.Current.CancellationToken);

            request.Headers.GetValues(CorrelationHeader).Should().ContainSingle("operation-correlation");
        }
        finally
        {
            OperationContext.Current = previous;
        }
    }

    [Fact]
    public async Task SendAsync_ShouldNotPropagateInvalidIncomingHeader()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "trusted-trace" }
        };
        httpContextAccessor.HttpContext.Request.Headers[CorrelationHeader] = "invalid correlation value";

        using var handler = new TestCorrelationHeaderHandler(httpContextAccessor, new CaptureHandler());
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test");

        await handler.SendAsyncPublic(request, TestContext.Current.CancellationToken);

        var propagated = request.Headers.GetValues(CorrelationHeader).Single();
        propagated.Should().NotBe("invalid correlation value");
        propagated.Should().HaveLength(32);
    }

    /// <summary>
    /// Exposes the protected handler method for direct testing.
    /// </summary>
    private sealed class TestCorrelationHeaderHandler(IHttpContextAccessor accessor, HttpMessageHandler innerHandler)
        : CorrelationHeaderHandler(accessor)
    {
        public Task<HttpResponseMessage> SendAsyncPublic(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            InnerHandler = innerHandler;
            return SendAsync(request, cancellationToken);
        }
    }

    /// <summary>
    /// Captures outgoing requests and returns a successful fake response.
    /// </summary>
    private sealed class CaptureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
