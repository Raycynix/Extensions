using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Raycynix.Extensions.Contracts.AspNetCore.Extensions;

namespace Raycynix.Extensions.Contracts.AspNetCore.Tests.Extensions;

/// <summary>
/// Covers conversion from the MVC model state to the shared error contract.
/// </summary>
public sealed class ModelStateContractExtensionsTests
{
    /// <summary>
    /// Verifies that model state errors are converted into validation error entries.
    /// </summary>
    [Fact]
    public void ToErrorContract_ShouldMapModelStateErrors()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("pageSize", "Page size must be greater than zero.");

        var error = modelState.ToErrorContract(traceId: "trace-1");

        error.Code.Should().Be("validation_failed");
        error.TraceId.Should().Be("trace-1");
        error.ValidationErrors.Should().ContainSingle();
        error.ValidationErrors.Single().Field.Should().Be("pageSize");
    }

    /// <summary>
    /// Verifies that the generated top-level error identity cannot be empty.
    /// </summary>
    [Fact]
    public void ToErrorContract_ShouldRejectEmptyErrorCode()
    {
        var modelState = new ModelStateDictionary();

        var action = () => modelState.ToErrorContract(code: string.Empty);

        action.Should().Throw<ArgumentException>();
    }
}
