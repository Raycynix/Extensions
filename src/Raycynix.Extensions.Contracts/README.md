# Raycynix.Extensions.Contracts

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Contracts` contains reusable contract models and contract conventions for Raycynix applications.

## What it contains

- `Money`
- `Quantity`
- `UoM`
- `PagingRequest`
- `PageInfo`
- `PagedResult<TItem>`
- DTO and contract versioning conventions for cross-service APIs

## What it does not contain

- `master data` sources
- reference data synchronization
- service-specific DTOs
- API gateway contracts

## Contract Rules

- shared DTOs must be backward compatible within the same major contract version
- new fields must be additive and optional for existing consumers
- existing public fields must not be removed or renamed inside the same major contract version
- breaking changes require a new contract version
- contract models must stay serialization-friendly and avoid behavior-heavy logic
- cross-service reusable types belong here, service-local DTOs do not

## Usage

```csharp
var price = new Money
{
    Amount = 149.99m,
    Currency = "USD"
};

var quantity = new Quantity
{
    Value = 12.5m,
    UoM = new UoM
    {
        Code = "kg",
        Name = "Kilogram"
    }
};

var result = new PagedResult<Money>
{
    Items = new[] { price },
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
```
