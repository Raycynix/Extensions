using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.MsSql;
using Raycynix.Extensions.Database.MsSql.Configurations;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers SQL Server-specific service registration.
/// </summary>
public sealed class MsSqlRegistrationTests
{
    /// <summary>
    /// Verifies that SQL Server options bind from the nested database configuration section.
    /// </summary>
    [Fact]
    public void AddMsSql_ShouldBindOptionsFromDatabaseConfigurationSection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = "Server=localhost;Database=test;",
                ["DatabaseConfiguration:MsSqlServerConfiguration:TrustServerCertificate"] = "false",
                ["DatabaseConfiguration:MsSqlServerConfiguration:CommandTimeoutSeconds"] = "45",
                ["DatabaseConfiguration:MsSqlServerConfiguration:MultipleActiveResultSets"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration)
            .AddMsSql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var options = serviceProvider.GetRequiredService<MsSqlServerConfiguration>();

        options.TrustServerCertificate.Should().BeFalse();
        options.CommandTimeoutSeconds.Should().Be(45);
        options.MultipleActiveResultSets.Should().BeTrue();
    }
}
