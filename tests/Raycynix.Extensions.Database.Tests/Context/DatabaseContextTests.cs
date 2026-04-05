using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions.Attributes;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Sqlite;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Tests.Context;

/// <summary>
/// Covers public runtime behavior of <see cref="DatabaseContext"/>.
/// </summary>
public sealed class DatabaseContextTests
{
    /// <summary>
    /// Verifies that the context applies tracking-related settings from the configuration.
    /// </summary>
    [Fact]
    public void Constructor_ShouldApplyChangeTrackerSettingsFromConfiguration()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddRaycynixDatabase(BuildConfiguration(), registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        context.ChangeTracker.LazyLoadingEnabled.Should().BeTrue();
        context.ChangeTracker.AutoDetectChangesEnabled.Should().BeFalse();
        context.ChangeTracker.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.NoTracking);
    }

    /// <summary>
    /// Verifies that <see cref="GenericConfigurator{T}"/> uses <see cref="DatabaseTableAttribute"/> when present.
    /// </summary>
    [Fact]
    public void GenericConfigurator_ShouldUseConfiguredTableNameAttribute()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddRaycynixDatabase(BuildConfiguration(), registerCallerAssembly: false)
            .AddSqlite()
            .AddAssembly<AttributedEntity>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var entityType = context.Model.FindEntityType(typeof(AttributedEntity));

        entityType.Should().NotBeNull();
        entityType.FindAnnotation("Relational:TableName")!.Value.Should().Be("attributed_entities");
    }

    /// <summary>
    /// Verifies that <see cref="GenericConfigurator{T}"/> can override the attribute table name with a runtime value.
    /// </summary>
    [Fact]
    public void GenericConfigurator_ShouldUseRuntimeTableNameOverride()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddSingleton(new RuntimeAttributedEntityConfiguration("runtime_attributed_entities"));
        services.AddRaycynixDatabase(BuildConfiguration(), registerCallerAssembly: false)
            .AddSqlite()
            .AddAssembly<RuntimeAttributedEntity>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var entityType = context.Model.FindEntityType(typeof(RuntimeAttributedEntity));

        entityType.Should().NotBeNull();
        entityType.FindAnnotation("Relational:TableName")!.Value.Should().Be("runtime_attributed_entities");
    }

    [DatabaseTable("attributed_entities")]
    private sealed class AttributedEntityConfigurator : GenericConfigurator<AttributedEntity>
    {
        public override Type[] DependsOn => [];
    }

    private sealed class AttributedEntity
    {
        public int Id { get; set; }
    }

    [DatabaseTable("ignored_attributed_entities")]
    private sealed class RuntimeAttributedEntityConfigurator(
        RuntimeAttributedEntityConfiguration? configuration = null) : GenericConfigurator<RuntimeAttributedEntity>
    {
        private readonly RuntimeAttributedEntityConfiguration _configuration =
            configuration ?? new RuntimeAttributedEntityConfiguration("ignored_attributed_entities");

        public override Type[] DependsOn => [];

        public override void Configure(ModelBuilder modelBuilder)
        {
            ConfigureEntity(modelBuilder, _configuration.TableName);
        }

        protected override string? GetModelShapeCacheKey()
        {
            return _configuration.TableName;
        }
    }

    private sealed class RuntimeAttributedEntity
    {
        public int Id { get; set; }
    }

    private sealed record RuntimeAttributedEntityConfiguration(string TableName);

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseConfiguration:ConnectionString"] = "Data Source=test.db",
                ["DatabaseConfiguration:EnsureCreated"] = "true",
                ["DatabaseConfiguration:EnableLazyLoading"] = "true",
                ["DatabaseConfiguration:EnableAutoDetectChanges"] = "false",
                ["DatabaseConfiguration:UseQueryTrackingByDefault"] = "false",
                ["DatabaseConfiguration:EnableSeed"] = "false"
            })
            .Build();
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
