using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Sqlite;
using Raycynix.Extensions.Database.Sqlite.Configurations;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers SQLite-specific service registration.
/// </summary>
public sealed class SqliteRegistrationTests
{
    /// <summary>
    /// Verifies that SQLite options bind from the nested database configuration section.
    /// </summary>
    [Fact]
    public void AddSqlite_ShouldBindOptionsFromDatabaseConfigurationSection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = "Data Source=test.db",
                ["DatabaseConfiguration:SqliteConfiguration:Mode"] = "ReadWriteCreate",
                ["DatabaseConfiguration:SqliteConfiguration:Cache"] = "Shared",
                ["DatabaseConfiguration:SqliteConfiguration:CommandTimeoutSeconds"] = "45"
            })
            .Build();

        services.AddRaycynixDatabase(configuration)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var options = serviceProvider.GetRequiredService<SqliteConfiguration>();

        options.Mode.Should().Be("ReadWriteCreate");
        options.Cache.Should().Be("Shared");
        options.CommandTimeoutSeconds.Should().Be(45);
    }
}
