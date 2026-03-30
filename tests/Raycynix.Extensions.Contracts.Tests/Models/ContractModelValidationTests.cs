using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.Tests.Models;

/// <summary>
/// Covers validation-friendly behavior of shared contract models.
/// </summary>
public sealed class ContractModelValidationTests
{
    /// <summary>
    /// Verifies that money contracts require uppercase ISO-like currency codes.
    /// </summary>
    [Fact]
    public void Money_ShouldExposeMatchingRuntimeAndDataAnnotationValidation()
    {
        var invalid = new Money
        {
            Amount = 12.5m,
            Currency = "usd"
        };

        invalid.IsValid().Should().BeFalse();
        Validate(invalid).Should().ContainSingle(error => error.MemberNames.Contains(nameof(Money.Currency)));
    }

    /// <summary>
    /// Verifies that paging requests reject non-positive page parameters.
    /// </summary>
    [Fact]
    public void PagingRequest_ShouldRejectNonPositiveValues()
    {
        var request = new PagingRequest
        {
            Page = 0,
            PageSize = -1
        };

        request.IsValid().Should().BeFalse();
        Validate(request).Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that page info is valid for empty collections without navigation flags.
    /// </summary>
    [Fact]
    public void PageInfo_ShouldAllowEmptyCollectionShape()
    {
        var pageInfo = new PageInfo
        {
            Page = 1,
            PageSize = 20,
            TotalCount = 0,
            TotalPages = 0,
            HasPreviousPage = false,
            HasNextPage = false
        };

        pageInfo.IsValid().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that error contracts require a code, message, and valid validation entries.
    /// </summary>
    [Fact]
    public void ErrorContract_ShouldRequireStructuredValidationEntries()
    {
        var error = new ErrorContract
        {
            Code = "validation_failed",
            Message = "Request is invalid.",
            ValidationErrors =
            [
                new ValidationError
                {
                    Field = "pageSize",
                    Code = "out_of_range",
                    Message = "Page size must be greater than zero."
                }
            ]
        };

        error.IsValid().Should().BeTrue();
    }

    private static IReadOnlyCollection<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);

        return results;
    }
}
