using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Sqlite;
using Raycynix.Extensions.Database.Sqlite.Options;

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
    public void AddSqlite_ShouldBindOptionsFromDatabaseOptionsSection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=test.db",
                ["DatabaseOptions:SqliteOptions:Mode"] = "ReadWriteCreate",
                ["DatabaseOptions:SqliteOptions:Cache"] = "Shared",
                ["DatabaseOptions:SqliteOptions:CommandTimeoutSeconds"] = "45"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var options = serviceProvider.GetRequiredService<SqliteOptions>();

        options.Mode.Should().Be("ReadWriteCreate");
        options.Cache.Should().Be("Shared");
        options.CommandTimeoutSeconds.Should().Be(45);
    }
}
