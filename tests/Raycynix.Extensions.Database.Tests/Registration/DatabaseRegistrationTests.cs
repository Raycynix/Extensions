using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Attributes;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Infrastructure;
using Raycynix.Extensions.Database.PostgreSql;
using Raycynix.Extensions.Database.Sqlite;
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
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=test.db",
                ["DatabaseOptions:EnsureCreated"] = "true",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var databaseConfiguration = serviceProvider.GetRequiredService<DatabaseOptions>();
        var accessor = serviceProvider.GetRequiredService<IConfigurationAccessor<DatabaseOptions>>();
        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        databaseConfiguration.ConnectionString.Should().Be("Data Source=test.db");
        databaseConfiguration.EnableSeed.Should().BeFalse();
        accessor.Current.ConnectionString.Should().Be("Data Source=test.db");
        initializer.Should().BeOfType<DatabaseInitializer<RaycynixDatabaseContext>>();
        context.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the setup callback executes during options creation.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldInvokeSetupCallback()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));
        var setupInvoked = false;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=original.db",
                ["DatabaseOptions:EnsureCreated"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, _ =>
        {
            setupInvoked = true;
        }, registerCallerAssembly: false);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        _ = serviceProvider.GetRequiredService<DatabaseOptions>();

        setupInvoked.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that configuration setup cannot be supplied after the database context has already been registered.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldFail_WhenSetupIsProvidedAfterInitialRegistration()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=repeat-setup.db",
                ["DatabaseOptions:EnsureCreated"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false);

        var act = () => services.AddRaycynixDatabase(
            configuration,
            _ => { },
            registerCallerAssembly: false);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Configure DatabaseOptions only on the first AddRaycynixDatabase call*");
    }

    /// <summary>
    /// Verifies that invalid database configuration fails during option validation.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldFail_WhenConfigurationIsInvalid()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:EnsureCreated"] = "true",
                ["DatabaseOptions:UseMigrations"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        var act = () => serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    /// <summary>
    /// Verifies that resolving the context fails when no provider package is registered.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldFail_WhenNoProviderPackageIsRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=no-provider.db",
                ["DatabaseOptions:EnsureCreated"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var act = () => scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        act.Should()
            .Throw<NotSupportedException>()
            .WithMessage("*No database provider is registered*");
    }

    /// <summary>
    /// Verifies that resolving the context fails when multiple provider packages are registered.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldFail_WhenMultipleProviderPackagesAreRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=multiple-providers.db",
                ["DatabaseOptions:EnsureCreated"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite()
            .AddPostgreSql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var act = () => scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Multiple database providers are registered*");
    }

    /// <summary>
    /// Verifies that the removed legacy Provider configuration key no longer selects a provider package.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldFail_WhenOnlyLegacyProviderKeyIsConfigured()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:Provider"] = "Sqlite",
                ["DatabaseOptions:ConnectionString"] = "Data Source=legacy-provider.db",
                ["DatabaseOptions:EnsureCreated"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var act = () => scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        act.Should()
            .Throw<NotSupportedException>()
            .WithMessage("*No database provider is registered*");
    }

    /// <summary>
    /// Verifies that the removed legacy Provider configuration key does not override the explicitly registered package.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldIgnoreLegacyProviderKey_WhenProviderPackageIsExplicitlyRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:Provider"] = "PostgreSql",
                ["DatabaseOptions:ConnectionString"] = "Data Source=legacy-provider-ignored.db",
                ["DatabaseOptions:EnsureCreated"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        context.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that configurators from explicitly registered external assemblies are applied to the shared model.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabaseAssembly_ShouldIncludeConfiguratorsFromExternalAssembly()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));
        services.AddSingleton(new MessagingDatabasePersistenceConfiguration());

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=model-test.db",
                ["DatabaseOptions:EnsureCreated"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite();
        services.AddRaycynixDatabaseAssembly(typeof(MessagingInboxEntryEntity).Assembly);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

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
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=builder-test.db",
                ["DatabaseOptions:EnsureCreated"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite()
            .AddAssembly<ExternalConfiguredEntity>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
        var entityType = context.Model.FindEntityType(typeof(ExternalConfiguredEntity));

        entityType.Should().NotBeNull();
        entityType.GetTableName().Should().Be("external_configured_entities");
    }

    /// <summary>
    /// Verifies that disabling caller assembly registration prevents local configurators from being scanned implicitly.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_ShouldNotScanCallerAssembly_WhenCallerAssemblyRegistrationIsDisabled()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=no-caller-scan.db",
                ["DatabaseOptions:EnsureCreated"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        context.Model.FindEntityType(typeof(ExternalConfiguredEntity)).Should().BeNull();
    }

    /// <summary>
    /// Verifies that the single-marker overload uses the marker assembly as the primary registration assembly.
    /// </summary>
    [Fact]
    public void AddRaycynixDatabase_WithMarker_ShouldExposeMarkerAssemblyAsPrimaryAssembly()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(Logging.Abstractions.ILogger<>), typeof(FakeLogger<>));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Data Source=marker-assembly.db",
                ["DatabaseOptions:EnsureCreated"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();

        var builder = services.AddRaycynixDatabase<RaycynixDatabaseContext, DatabaseRegistrationTests>(configuration);

        builder.CallerAssembly.GetName().Name.Should().Be(typeof(DatabaseRegistrationTests).Assembly.GetName().Name);
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

    private sealed class FakeLogger<T> : Logging.Abstractions.ILogger<T>
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
        
        public void Log(LogLevel logLevel, Exception? exception, string message, params object?[]? args)
        {
        }
    }
}
