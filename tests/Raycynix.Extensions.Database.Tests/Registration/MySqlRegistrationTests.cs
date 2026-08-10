using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.MySql;
using Raycynix.Extensions.Database.MySql.Options;

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
    public void AddMySql_ShouldBindOptionsFromDatabaseOptionsSection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Server=localhost;Database=test;",
                ["DatabaseOptions:MySqlOptions:AllowUserVariables"] = "false",
                ["DatabaseOptions:MySqlOptions:Pooling"] = "false",
                ["DatabaseOptions:MySqlOptions:CommandTimeoutSeconds"] = "45"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddMySql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var options = serviceProvider.GetRequiredService<MySqlOptions>();

        options.AllowUserVariables.Should().BeFalse();
        options.Pooling.Should().BeFalse();
        options.CommandTimeoutSeconds.Should().Be(45);
    }
}
