using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Attributes;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Enums;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Messaging.Database.Configurations;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers service registration for the database package.
/// </summary>
public sealed class DatabaseRegistrationTests
{
    /// <summary>
    /// Verifies that database services are registered and bound from configuration.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldRegisterCoreServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = "Data Source=test.db",
                ["DatabaseConfiguration:Provider"] = nameof(DatabaseProvider.Sqlite),
                ["DatabaseConfiguration:EnsureCreated"] = "true",
                ["DatabaseConfiguration:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var databaseConfiguration = serviceProvider.GetRequiredService<DatabaseConfiguration>();
        var accessor = serviceProvider.GetRequiredService<IConfigurationAccessor<DatabaseConfiguration>>();
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        databaseConfiguration.ConnectionString.Should().Be("Data Source=test.db");
        databaseConfiguration.Provider.Should().Be(DatabaseProvider.Sqlite);
        databaseConfiguration.EnableSeed.Should().BeFalse();
        accessor.Current.ConnectionString.Should().Be("Data Source=test.db");
        initializer.Should().BeOfType<DatabaseInitializer>();
        context.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the setup callback executes during options creation.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldInvokeSetupCallback()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        var setupInvoked = false;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = "Data Source=original.db",
                ["DatabaseConfiguration:Provider"] = nameof(DatabaseProvider.Sqlite),
                ["DatabaseConfiguration:EnsureCreated"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, _ =>
        {
            setupInvoked = true;
        });

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        _ = serviceProvider.GetRequiredService<DatabaseConfiguration>();

        setupInvoked.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that invalid database configuration fails during option validation.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldFail_WhenConfigurationIsInvalid()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:EnsureCreated"] = "true",
                ["DatabaseConfiguration:UseMigrations"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        var act = () => serviceProvider.GetRequiredService<IOptions<DatabaseConfiguration>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    /// <summary>
    /// Verifies that configurators from explicitly registered external assemblies are applied to the shared model.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabaseAssembly_ShouldIncludeConfiguratorsFromExternalAssembly()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddSingleton(new MessagingDatabasePersistenceConfiguration());

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = "Data Source=model-test.db",
                ["DatabaseConfiguration:Provider"] = nameof(DatabaseProvider.Sqlite),
                ["DatabaseConfiguration:EnsureCreated"] = "false",
                ["DatabaseConfiguration:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration);
        services.AddRaycynixDatabaseAssembly(typeof(MessagingInboxEntryEntity).Assembly);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        context.Model.FindEntityType(typeof(MessagingInboxEntryEntity)).Should().NotBeNull();
        context.Model.FindEntityType(typeof(MessagingOutboxEntryEntity)).Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the fluent database builder can register additional configurator assemblies.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldReturnBuilderThatSupportsAssemblyRegistration()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = "Data Source=builder-test.db",
                ["DatabaseConfiguration:Provider"] = nameof(DatabaseProvider.Sqlite),
                ["DatabaseConfiguration:EnsureCreated"] = "false",
                ["DatabaseConfiguration:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration)
            .AddAssembly<ExternalConfiguredEntity>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var entityType = context.Model.FindEntityType(typeof(ExternalConfiguredEntity));

        entityType.Should().NotBeNull();
        entityType.GetTableName().Should().Be("external_configured_entities");
    }

    [DatabaseTable("external_configured_entities")]
    private sealed class ExternalConfiguredEntityConfigurator : GenericConfigurator<ExternalConfiguredEntity>
    {
        public override Type[] DependsOn => [];
    }

    private sealed class ExternalConfiguredEntity
    {
        public int Id { get; set; }
    }

    private sealed class FakeLogger<T> : ILogger<T>
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

        public void Log(Microsoft.Extensions.Logging.LogLevel logLevel, string message, Exception? exception = null,
            object? metadata = null)
        {
        }
    }
}
