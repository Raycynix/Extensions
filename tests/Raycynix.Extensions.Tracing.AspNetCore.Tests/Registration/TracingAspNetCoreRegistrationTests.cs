using FluentAssertions;
using Microsoft.AspNetCore.Builder;

namespace Raycynix.Extensions.Tracing.AspNetCore.Tests.Registration;

/// <summary>
/// Covers registration-style behavior for the ASP.NET Core tracing package.
/// </summary>
public sealed class TracingAspNetCoreRegistrationTests
{
    /// <summary>
    /// Verifies that the middleware extension returns the same application builder for chaining.
    /// </summary>
    [Fact]
    public void UseRaycynixTracing_ShouldReturnApplicationBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        var result = app.UseRaycynixTracing();

        result.Should().BeSameAs(app);
    }
}
