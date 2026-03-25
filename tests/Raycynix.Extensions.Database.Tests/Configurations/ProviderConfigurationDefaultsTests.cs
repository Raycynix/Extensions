using FluentAssertions;
using Raycynix.Extensions.Database.Configurations;

namespace Raycynix.Extensions.Database.Tests.Configurations;

/// <summary>
/// Covers defaults for provider-specific database configuration types.
/// </summary>
public sealed class ProviderConfigurationDefaultsTests
{
    /// <summary>
    /// Verifies defaults for PostgreSQL settings.
    /// </summary>
    [Fact]
    public void PostgreSqlDefaults_ShouldMatchExpectedValues()
    {
        var configuration = new PostgreSqlConfiguration();

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
        var configuration = new MsSqlServerConfiguration();

        configuration.TrustServerCertificate.Should().BeTrue();
        configuration.MultipleActiveResultSets.Should().BeFalse();
        configuration.CommandTimeoutSeconds.Should().BeNull();
    }

    /// <summary>
    /// Verifies defaults for MySQL settings.
    /// </summary>
    [Fact]
    public void MySqlDefaults_ShouldMatchExpectedValues()
    {
        var configuration = new MySqlConfiguration();

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
        var configuration = new SqlliteConfiguration();

        configuration.Mode.Should().BeNull();
        configuration.Cache.Should().BeNull();
        configuration.CommandTimeoutSeconds.Should().BeNull();
    }
}
