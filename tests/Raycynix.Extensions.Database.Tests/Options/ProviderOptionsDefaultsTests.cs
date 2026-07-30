using FluentAssertions;
using Raycynix.Extensions.Database.MsSql.Options;
using Raycynix.Extensions.Database.MySql.Options;
using Raycynix.Extensions.Database.PostgreSql.Options;
using Raycynix.Extensions.Database.Sqlite.Options;

namespace Raycynix.Extensions.Database.Tests.Options;

/// <summary>
/// Covers defaults and validation for provider-specific database options.
/// </summary>
public sealed class ProviderOptionsDefaultsTests
{
    /// <summary>
    /// Verifies defaults for PostgreSQL settings.
    /// </summary>
    [Fact]
    public void PostgreSqlDefaults_ShouldMatchExpectedValues()
    {
        var configuration = new PostgreSqlOptions();

        configuration.Pooling.Should().BeTrue();
        configuration.IncludeErrorDetail.Should().BeFalse();
        configuration.MinimumPoolSize.Should().BeNull();
        configuration.MaximumPoolSize.Should().BeNull();
        configuration.CommandTimeoutSeconds.Should().BeNull();
    }

    /// <summary>
    /// Verifies defaults for SQL Server settings.
    /// </summary>
    [Fact]
    public void MsSqlServerDefaults_ShouldMatchExpectedValues()
    {
        var configuration = new MsSqlServerOptions();

        configuration.TrustServerCertificate.Should().BeFalse();
        configuration.MultipleActiveResultSets.Should().BeFalse();
        configuration.CommandTimeoutSeconds.Should().BeNull();
    }

    /// <summary>
    /// Verifies defaults for MySQL settings.
    /// </summary>
    [Fact]
    public void MySqlDefaults_ShouldMatchExpectedValues()
    {
        var configuration = new MySqlOptions();

        configuration.AllowUserVariables.Should().BeTrue();
        configuration.Pooling.Should().BeTrue();
        configuration.CommandTimeoutSeconds.Should().BeNull();
    }

    /// <summary>
    /// Verifies defaults for SQLite settings.
    /// </summary>
    [Fact]
    public void SqliteDefaults_ShouldMatchExpectedValues()
    {
        var configuration = new SqliteOptions();

        configuration.Mode.Should().BeNull();
        configuration.Cache.Should().BeNull();
        configuration.CommandTimeoutSeconds.Should().BeNull();
    }

    /// <summary>
    /// Verifies PostgreSQL pool and timeout validation.
    /// </summary>
    [Fact]
    public void PostgreSqlValidate_ShouldRejectInvalidPoolRange()
    {
        var options = new PostgreSqlOptions
        {
            MinimumPoolSize = 10,
            MaximumPoolSize = 5
        };

        var action = options.Validate;

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Minimum pool size cannot exceed maximum pool size*");
    }

    /// <summary>
    /// Verifies SQL Server timeout validation.
    /// </summary>
    [Fact]
    public void MsSqlServerValidate_ShouldRejectNegativeTimeout()
    {
        var options = new MsSqlServerOptions
        {
            CommandTimeoutSeconds = -1
        };

        var action = options.Validate;

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies MySQL timeout validation.
    /// </summary>
    [Fact]
    public void MySqlValidate_ShouldRejectNegativeTimeout()
    {
        var options = new MySqlOptions
        {
            CommandTimeoutSeconds = -1
        };

        var action = options.Validate;

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies SQLite mode validation.
    /// </summary>
    [Fact]
    public void SqliteValidate_ShouldRejectUnsupportedMode()
    {
        var options = new SqliteOptions
        {
            Mode = "NotARealMode"
        };

        var action = options.Validate;

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Mode contains unsupported value*");
    }
}
