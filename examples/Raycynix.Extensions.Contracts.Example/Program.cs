using System.ComponentModel.DataAnnotations;
using Raycynix.Extensions.Contracts.Constants;
using Raycynix.Extensions.Contracts.Models;

var price = new Money
{
    Amount = 149.99m,
    Currency = "USD"
};

var pagedPrices = new PagedResult<Money>
{
    Items = [price],
    PageInfo = new PageInfo
    {
        Page = 1,
        PageSize = 20,
        TotalCount = 1,
        TotalPages = 1,
        HasPreviousPage = false,
        HasNextPage = false
    }
};

var contractVersion = ContractVersion.Parse("1.2.0");
var requestedVersion = ContractVersion.Parse("1.1.0");

var versionedContract = new VersionedContract<PagedResult<Money>>
{
    Metadata = new ContractMetadata
    {
        Name = "catalog.prices",
        Version = contractVersion
    },
    Payload = pagedPrices
};

var transportError = new ErrorContract
{
    Code = "validation_failed",
    Message = "One or more validation errors occurred.",
    TraceId = Guid.NewGuid().ToString("N"),
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

Console.WriteLine("Raycynix contracts example");
Console.WriteLine($"Contract: {versionedContract.Metadata.Name}");
Console.WriteLine($"Version: {versionedContract.Metadata.Version}");
Console.WriteLine($"Requested version: {requestedVersion}");
Console.WriteLine($"Compatible line: {requestedVersion <= versionedContract.Metadata.Version}");
Console.WriteLine($"Money is valid: {price.IsValid()}");
Console.WriteLine($"Paged result is valid: {pagedPrices.IsValid()}");
Console.WriteLine($"Error contract is valid: {transportError.IsValid()}");
Console.WriteLine();
Console.WriteLine("Shared transport headers:");
Console.WriteLine($"  {ContractHeaders.ContractName}: {versionedContract.Metadata.Name}");
Console.WriteLine($"  {ContractHeaders.ContractVersion}: {versionedContract.Metadata.Version}");
Console.WriteLine();
Console.WriteLine("DataAnnotations validation:");

foreach (var result in Validate(price))
{
    Console.WriteLine($"  {result.ErrorMessage}");
}

Console.WriteLine();
Console.WriteLine("Payload preview:");
Console.WriteLine($"  Amount: {price.Amount} {price.Currency}");
Console.WriteLine($"  Total items: {versionedContract.Payload?.PageInfo.TotalCount}");

static IReadOnlyCollection<ValidationResult> Validate(object instance)
{
    var results = new List<ValidationResult>();

    Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);

    return results;
}
