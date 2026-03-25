using FluentAssertions;
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Defaults;

namespace Raycynix.Extensions.Exceptions.Tests.Mapping;

/// <summary>
/// Covers exception mapping behavior.
/// </summary>
public sealed class ExceptionMapperTests
{
    /// <summary>
    /// Verifies that Raycynix exceptions pass through without remapping.
    /// </summary>
    [Fact]
    public void Map_ShouldReturnSameException_WhenInputIsAlreadyRaycynixException()
    {
        var mapper = new ExceptionMapper(new Dictionary<Type, Func<Exception, RaycynixException>>());
        var exception = new NotFoundException("Missing");

        var result = mapper.Map(exception);

        result.Should().BeSameAs(exception);
    }

    /// <summary>
    /// Verifies that the most specific configured mapping is selected.
    /// </summary>
    [Fact]
    public void Map_ShouldUseMostSpecificConfiguredMapping()
    {
        var mapper = new ExceptionMapper(new Dictionary<Type, Func<Exception, RaycynixException>>
        {
            [typeof(Exception)] = _ => new ConflictException("generic"),
            [typeof(InvalidOperationException)] = _ => new NotFoundException("specific")
        });

        var result = mapper.Map(new InvalidOperationException("boom"));

        result.Should().BeOfType<NotFoundException>();
        result.Message.Should().Be("specific");
    }

    /// <summary>
    /// Verifies that unmapped exceptions fall back to internal server errors.
    /// </summary>
    [Fact]
    public void Map_ShouldFallbackToInternalServerException_WhenNoMappingExists()
    {
        var mapper = new ExceptionMapper(new Dictionary<Type, Func<Exception, RaycynixException>>());

        var result = mapper.Map(new Exception("boom"));

        result.Should().BeOfType<InternalServerException>();
        result.StatusCode.Should().Be(500);
        result.ErrorCode.Should().Be("INTERNAL_SERVER_ERROR");
        result.Category.Should().Be(ErrorCategory.Internal);
    }
}
