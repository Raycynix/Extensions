using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Sqlite;

namespace Raycynix.Extensions.Database.Tests.Initialization;

/// <summary>
/// Covers initialization flow for <see cref="DatabaseInitializer"/>.
/// </summary>
public sealed class DatabaseInitializerTests
{
    /// <summary>
    /// Verifies that initialization marks the database as ready when ensure-created is enabled.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_ShouldMarkInitializerAsReady_WhenEnsureCreatedSucceeds()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));
        services.AddRaycynixDatabase(BuildSqliteConfiguration(databasePath), registerCallerAssembly: false)
            .AddSqlite();

        try
        {
            await using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
            await using var scope = serviceProvider.CreateAsyncScope();
            var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

            await initializer.InitializeAsync(TestContext.Current.CancellationToken);

            initializer.IsReady.Should().BeTrue();
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that repeated initialization returns early once the database is ready.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_ShouldBeIdempotent_WhenAlreadyReady()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));
        services.AddRaycynixDatabase(BuildSqliteConfiguration(databasePath), registerCallerAssembly: false)
            .AddSqlite();

        try
        {
            await using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
            await using var scope = serviceProvider.CreateAsyncScope();
            var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

            await initializer.InitializeAsync(TestContext.Current.CancellationToken);
            await initializer.InitializeAsync(TestContext.Current.CancellationToken);

            initializer.IsReady.Should().BeTrue();
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    private static IConfiguration BuildSqliteConfiguration(string databasePath)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = $"Data Source={databasePath}",
                ["DatabaseConfiguration:EnsureCreated"] = "true",
                ["DatabaseConfiguration:UseMigrations"] = "false",
                ["DatabaseConfiguration:EnableSeed"] = "false"
            })
            .Build();
    }

    private static void TryDelete(string databasePath)
    {
        try
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed class FakeLogger<T> : Logging.Abstractions.ILogger<T>
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

        public void Log(LogLevel logLevel, Exception? exception, string message, params object?[]? args)
        {
            throw new NotImplementedException();
        }
    }
}
