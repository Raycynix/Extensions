using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.AspNetCore.Tests;

/// <summary>
/// Covers public database initialization extensions for ASP.NET Core applications.
/// </summary>
public sealed class DatabaseInitializationTests
{
    /// <summary>
    /// Verifies that web initialization invokes the registered database initializer.
    /// </summary>
    [Fact]
    public async Task InitializeRaycynixDatabaseAsync_ShouldInvokeInitializer()
    {
        var initializer = new FakeDatabaseInitializer();
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<IDatabaseInitializer>(_ => initializer);

        await using var app = builder.Build();

        var returnedApp = await app.InitializeRaycynixDatabaseAsync(TestContext.Current.CancellationToken);

        returnedApp.Should().BeSameAs(app);
        initializer.CallCount.Should().Be(1);
        initializer.LastCancellationToken.Should().Be(TestContext.Current.CancellationToken);
    }

    private sealed class FakeDatabaseInitializer : IDatabaseInitializer
    {
        public int CallCount { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public bool IsReady => CallCount > 0;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }
}
