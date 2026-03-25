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
- `ContractVersion`
- `ContractMetadata`
- `VersionedContract<TContract>`
- `ContractHeaders`
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
- `Major` changes are breaking changes
- `Minor` changes are additive, backward-compatible changes
- `Patch` changes are non-breaking fixes that do not alter the contract shape
- contract models must stay serialization-friendly and avoid behavior-heavy logic
- cross-service reusable types belong here, service-local DTOs do not
- contract identifiers and versions should be explicit at transport boundaries when contracts are shared across services

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

var versioned = new VersionedContract<PagedResult<Money>>
{
    Metadata = new ContractMetadata
    {
        Name = "catalog.prices",
        Version = new ContractVersion
        {
            Major = 1,
            Minor = 0,
            Patch = 0
        }
    },
    Payload = result
};
```

## Versioning

Use `ContractVersion` to express the current shared contract version:

```csharp
var version = new ContractVersion
{
    Major = 1,
    Minor = 2,
    Patch = 0
};
```

When contract metadata must cross process boundaries explicitly, use:

- `ContractHeaders.ContractName`
- `ContractHeaders.ContractVersion`

This package only defines the common contract model and conventions. It does not enforce transport-specific version negotiation by itself.
