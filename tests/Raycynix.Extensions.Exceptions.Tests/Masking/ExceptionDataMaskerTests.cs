using FluentAssertions;
using Raycynix.Extensions.Exceptions.Defaults;

namespace Raycynix.Extensions.Exceptions.Tests.Masking;

/// <summary>
/// Covers sensitive data masking behavior.
/// </summary>
public sealed class ExceptionDataMaskerTests
{
    /// <summary>
    /// Verifies that sensitive keys are masked in dictionaries and nested objects.
    /// </summary>
    [Fact]
    public void Mask_ShouldHideSensitiveValues()
    {
        var masker = new ExceptionDataMasker();
        var payload = new
        {
            UserName = "john",
            Password = "secret",
            Nested = new Dictionary<string, object?>
            {
                ["ApiKey"] = "123",
                ["Visible"] = "ok"
            }
        };

        var result = masker.Mask(payload).Should().BeAssignableTo<Dictionary<string, object?>>().Subject;

        result["Password"].Should().Be("***MASKED***");
        var nested = result["Nested"].Should().BeAssignableTo<Dictionary<object, object?>>().Subject;
        nested["ApiKey"].Should().Be("***MASKED***");
        nested["Visible"].Should().Be("ok");
    }

    /// <summary>
    /// Verifies that circular references are handled safely.
    /// </summary>
    [Fact]
    public void Mask_ShouldHandleCircularReferences()
    {
        var masker = new ExceptionDataMasker();
        var node = new Node();
        node.Next = node;

        var result = masker.Mask(node).Should().BeAssignableTo<Dictionary<string, object?>>().Subject;

        result["Next"].Should().Be("[CircularReference]");
    }

    private sealed class Node
    {
        public Node? Next { get; set; }
    }
}
