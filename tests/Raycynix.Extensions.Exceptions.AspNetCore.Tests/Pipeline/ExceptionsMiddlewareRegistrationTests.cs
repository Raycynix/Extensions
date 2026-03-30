using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Exceptions.AspNetCore.Tests.Pipeline;

/// <summary>
/// Covers registration helpers for the ASP.NET Core exceptions package.
/// </summary>
public sealed class ExceptionsMiddlewareRegistrationTests
{
    /// <summary>
    /// Verifies that the middleware extension returns the same application builder.
    /// </summary>
    [Fact]
    public void UseRaycynixExceptions_ShouldReturnSameBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        builder.Services.AddRaycynixExceptions();
        builder.Services.AddSingleton<Common.Context.IOperationContext, Common.Context.OperationContext>();
        var app = builder.Build();

        var result = app.UseRaycynixExceptions();

        result.Should().BeSameAs(app);
    }
}
