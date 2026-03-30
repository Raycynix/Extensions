using FluentAssertions;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Implementation;

namespace Raycynix.Extensions.Security.Tests.Context;

/// <summary>
/// Covers the default security context model.
/// </summary>
public class SecurityContextTests
{
    /// <summary>
    /// Verifies that the default security context implementation preserves all configured values.
    /// </summary>
    [Fact]
    public void SecurityContext_ShouldExposeConfiguredValues()
    {
        var context = new SecurityContext
        {
            IsAuthenticated = true,
            SubjectId = "user-1",
            SubjectType = SecuritySubjectType.User,
            Roles = ["admin", "support"],
            Permissions = ["users.read", "users.update"]
        };

        context.IsAuthenticated.Should().BeTrue();
        context.SubjectId.Should().Be("user-1");
        context.SubjectType.Should().Be(SecuritySubjectType.User);
        context.Roles.Should().BeEquivalentTo(["admin", "support"]);
        context.Permissions.Should().BeEquivalentTo(["users.read", "users.update"]);
    }
}
