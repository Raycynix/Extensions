using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.PostgreSql;
using Raycynix.Extensions.Database.PostgreSql.Configurations;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers PostgreSQL-specific service registration.
/// </summary>
public sealed class PostgreSqlRegistrationTests
{
    /// <summary>
    /// Verifies that PostgreSQL options continue to bind from the nested database configuration section.
    /// </summary>
    [Fact]
    public void AddPostgreSql_ShouldBindOptionsFromDatabaseConfigurationSection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = "Host=localhost;Database=test;",
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
        var options = serviceProvider.GetRequiredService<PostgreSqlConfiguration>();

        options.Pooling.Should().BeFalse();
        options.MinimumPoolSize.Should().Be(2);
        options.MaximumPoolSize.Should().Be(25);
        options.CommandTimeoutSeconds.Should().Be(45);
        options.IncludeErrorDetail.Should().BeTrue();
    }
}
