using FluentAssertions;
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Options;

namespace Raycynix.Extensions.Exceptions.Tests.Options;

/// <summary>
/// Covers mapping registration overloads for <see cref="ExceptionMapperOptions"/>.
/// </summary>
public sealed class ExceptionMapperOptionsTests
{
    /// <summary>
    /// Verifies that delegate mappings are stored and executed.
    /// </summary>
    [Fact]
    public void Map_WithDelegate_ShouldRegisterMapping()
    {
        var options = new ExceptionMapperOptions();
        options.Map<ArgumentException>(exception => new ConflictException(exception.Message));

        var mapping = options.Mappings[typeof(ArgumentException)];
        var result = mapping(new ArgumentException("duplicate"));

        result.Should().BeOfType<ConflictException>();
        result.Message.Should().Be("duplicate");
    }
}
