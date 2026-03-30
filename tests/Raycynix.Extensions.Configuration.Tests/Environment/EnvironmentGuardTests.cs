using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Configuration.Tests.Environment;

/// <summary>
/// Covers environment registration guard behavior.
/// </summary>
public class EnvironmentGuardTests
{
    /// <summary>
    /// Verifies that an empty explicit environment name is rejected.
    /// </summary>
    [Fact]
    public void AddRaycynixEnvironment_ShouldRejectEmptyEnvironmentName()
    {
        var services = new ServiceCollection();

        var action = () => services.AddRaycynixEnvironment(string.Empty);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Environment name cannot be null or whitespace.*");
    }
}
