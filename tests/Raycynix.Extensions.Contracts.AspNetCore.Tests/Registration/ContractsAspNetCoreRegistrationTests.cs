using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Contracts.AspNetCore.Tests.Registration;

/// <summary>
/// Covers ASP.NET Core contract service registration behavior.
/// </summary>
public sealed class ContractsAspNetCoreRegistrationTests
{
    /// <summary>
    /// Verifies that ASP.NET Core contract registration is safe to call and preserves the service collection for chaining.
    /// </summary>
    [Fact]
    public void AddRaycynixContractsAspNetCore_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();

        var returned = services.AddRaycynixContractsAspNetCore();

        returned.Should().BeSameAs(services);
    }
}
