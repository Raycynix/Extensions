using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Enums;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Tests.Context;

/// <summary>
/// Covers public runtime behavior of <see cref="DatabaseContext"/>.
/// </summary>
public sealed class DatabaseContextTests
{
    /// <summary>
    /// Verifies that the context applies tracking-related settings from the configuration.
    /// </summary>
    [Fact]
    public void Constructor_ShouldApplyChangeTrackerSettingsFromConfiguration()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddRaycynixDatabase(BuildConfiguration());

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        context.ChangeTracker.LazyLoadingEnabled.Should().BeTrue();
        context.ChangeTracker.AutoDetectChangesEnabled.Should().BeFalse();
        context.ChangeTracker.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.NoTracking);
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:Provider"] = nameof(DatabaseProvider.Sqlite),
                ["DatabaseConfiguration:ConnectionString"] = "Data Source=test.db",
                ["DatabaseConfiguration:EnsureCreated"] = "true",
                ["DatabaseConfiguration:EnableLazyLoading"] = "true",
                ["DatabaseConfiguration:EnableAutoDetectChanges"] = "false",
                ["DatabaseConfiguration:UseQueryTrackingByDefault"] = "false",
                ["DatabaseConfiguration:EnableSeed"] = "false"
            })
            .Build();
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        public void Log(Microsoft.Extensions.Logging.LogLevel logLevel, string message, Exception? exception = null,
            object? metadata = null)
        {
        }
    }
}
