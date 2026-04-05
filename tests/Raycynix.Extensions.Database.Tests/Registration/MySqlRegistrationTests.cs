using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.MySql;
using Raycynix.Extensions.Database.MySql.Configurations;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers MySQL-specific service registration.
/// </summary>
public sealed class MySqlRegistrationTests
{
    /// <summary>
    /// Verifies that MySQL options bind from the nested database configuration section.
    /// </summary>
    [Fact]
    public void AddMySql_ShouldBindOptionsFromDatabaseConfigurationSection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = "Server=localhost;Database=test;",
                ["DatabaseConfiguration:MySqlConfiguration:AllowUserVariables"] = "false",
                ["DatabaseConfiguration:MySqlConfiguration:Pooling"] = "false",
                ["DatabaseConfiguration:MySqlConfiguration:CommandTimeoutSeconds"] = "45"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddMySql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var options = serviceProvider.GetRequiredService<MySqlConfiguration>();

        options.AllowUserVariables.Should().BeFalse();
        options.Pooling.Should().BeFalse();
        options.CommandTimeoutSeconds.Should().Be(45);
    }
}
