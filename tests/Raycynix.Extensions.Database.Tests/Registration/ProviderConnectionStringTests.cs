using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Npgsql;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.MsSql;
using Raycynix.Extensions.Database.MySql;
using Raycynix.Extensions.Database.PostgreSql;
using Raycynix.Extensions.Database.Sqlite;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers provider-specific connection-string resolution behavior.
/// </summary>
public sealed class ProviderConnectionStringTests
{
    /// <summary>
    /// Verifies that PostgreSQL connection-string resolution applies provider-specific settings.
    /// </summary>
    [Fact]
    public void PostgreSqlRegistration_ShouldBuildConnectionStringFromStructuredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:PostgreSqlConfiguration:Pooling"] = "false",
                ["DatabaseConfiguration:PostgreSqlConfiguration:MinimumPoolSize"] = "2",
                ["DatabaseConfiguration:PostgreSqlConfiguration:MaximumPoolSize"] = "25",
                ["DatabaseConfiguration:PostgreSqlConfiguration:CommandTimeoutSeconds"] = "45",
                ["DatabaseConfiguration:PostgreSqlConfiguration:IncludeErrorDetail"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration)
            .AddPostgreSql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "postgresql");

        var connectionString = registration.ResolveConnectionString(
            CreateConnectionConfiguration("db.local", 5433, "orders", "app", "secret"),
            serviceProvider);

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        builder.Host.Should().Be("db.local");
        builder.Port.Should().Be(5433);
        builder.Database.Should().Be("orders");
        builder.Username.Should().Be("app");
        builder.Password.Should().Be("secret");
        builder.Pooling.Should().BeFalse();
        builder.MinPoolSize.Should().Be(2);
        builder.MaxPoolSize.Should().Be(25);
        builder.CommandTimeout.Should().Be(45);
        builder.IncludeErrorDetail.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SQL Server connection-string resolution applies provider-specific settings.
    /// </summary>
    [Fact]
    public void MsSqlRegistration_ShouldBuildConnectionStringFromStructuredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:MsSqlServerConfiguration:TrustServerCertificate"] = "false",
                ["DatabaseConfiguration:MsSqlServerConfiguration:MultipleActiveResultSets"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration)
            .AddMsSql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "sqlserver");

        var connectionString = registration.ResolveConnectionString(
            CreateConnectionConfiguration("sql.local", null, "orders", "sa", "secret"),
            serviceProvider);

        var builder = new SqlConnectionStringBuilder(connectionString);
        builder.DataSource.Should().Be("sql.local");
        builder.InitialCatalog.Should().Be("orders");
        builder.UserID.Should().Be("sa");
        builder.Password.Should().Be("secret");
        builder.TrustServerCertificate.Should().BeFalse();
        builder.MultipleActiveResultSets.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that MySQL connection-string resolution applies provider-specific settings.
    /// </summary>
    [Fact]
    public void MySqlRegistration_ShouldBuildConnectionStringFromStructuredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:MySqlConfiguration:AllowUserVariables"] = "false",
                ["DatabaseConfiguration:MySqlConfiguration:Pooling"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration)
            .AddMySql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "mysql");

        var connectionString = registration.ResolveConnectionString(
            CreateConnectionConfiguration("mysql.local", 3307, "orders", "app", "secret"),
            serviceProvider);

        var builder = new MySqlConnectionStringBuilder(connectionString);
        builder.Server.Should().Be("mysql.local");
        builder.Port.Should().Be(3307);
        builder.Database.Should().Be("orders");
        builder.UserID.Should().Be("app");
        builder.Password.Should().Be("secret");
        builder.AllowUserVariables.Should().BeFalse();
        builder.Pooling.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that SQLite connection-string resolution applies provider-specific settings.
    /// </summary>
    [Fact]
    public void SqliteRegistration_ShouldBuildConnectionStringFromStructuredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:SqliteConfiguration:Mode"] = "ReadWriteCreate",
                ["DatabaseConfiguration:SqliteConfiguration:Cache"] = "Shared"
            })
            .Build();

        services.AddRaycynixDatabase(configuration)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "sqlite");

        var connectionString = registration.ResolveConnectionString(
            CreateConnectionConfiguration(null, null, "orders.db", null, null),
            serviceProvider);

        var builder = new SqliteConnectionStringBuilder(connectionString);
        builder.DataSource.Should().Be("orders.db");
        builder.Mode.Should().Be(SqliteOpenMode.ReadWriteCreate);
        builder.Cache.Should().Be(SqliteCacheMode.Shared);
    }

    /// <summary>
    /// Verifies that invalid SQLite mode values fail with a clear parse error during connection-string resolution.
    /// </summary>
    [Fact]
    public void SqliteRegistration_ShouldFail_WhenSqliteModeIsInvalid()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:SqliteConfiguration:Mode"] = "NotARealMode"
            })
            .Build();

        services.AddRaycynixDatabase(configuration)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "sqlite");

        var act = () => registration.ResolveConnectionString(
            CreateConnectionConfiguration(null, null, "orders.db", null, null),
            serviceProvider);

        act.Should().Throw<ArgumentException>();
    }

    private static IDatabaseProviderRegistration GetProviderRegistration(IServiceProvider serviceProvider, string providerName)
    {
        return serviceProvider
            .GetRequiredService<IEnumerable<IDatabaseProviderRegistration>>()
            .Single(registration => registration.ProviderName == providerName);
    }

    private static DatabaseConfiguration CreateConnectionConfiguration(
        string? host,
        int? port,
        string? name,
        string? username,
        string? password)
    {
        return new DatabaseConfiguration
        {
            ConnectionConfiguration = new TestConnectionConfiguration
            {
                Host = host,
                Port = port,
                Name = name,
                Username = username,
                Password = password
            }
        };
    }

    private sealed class TestConnectionConfiguration : ConnectionConfiguration;
}
