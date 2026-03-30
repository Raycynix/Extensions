using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Tests.Registration;

/// <summary>
/// Covers service registration for the core exceptions package.
/// </summary>
public sealed class ExceptionsRegistrationTests
{
    /// <summary>
    /// Verifies that the package registers all core exception services.
    /// </summary>
    [Fact]
    public void AddRaycynixExceptions_ShouldRegisterCoreServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixExceptions();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        serviceProvider.GetRequiredService<IExceptionMapper>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IExceptionDataMasker>().Should().NotBeNull();
        serviceProvider.GetRequiredService<ITransientExceptionClassifier>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IRetryExecutor>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IBackgroundTaskRunner>().Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that custom mapping configuration is applied to the registered mapper.
    /// </summary>
    [Fact]
    public void AddRaycynixExceptions_ShouldApplyCustomMappings()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixExceptions(options =>
        {
            options.Map<InvalidOperationException>("invalid_operation", "Invalid operation.", 400,
                Abstractions.Enums.ErrorCategory.Validation);
        });

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var mapper = serviceProvider.GetRequiredService<IExceptionMapper>();

        var result = mapper.Map(new InvalidOperationException("boom"));

        result.ErrorCode.Should().Be("invalid_operation");
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("Invalid operation.");
    }
}