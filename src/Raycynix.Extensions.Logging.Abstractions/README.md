# Raycynix.Extensions.Logging.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Logging.Abstractions` contains the contracts used by the Raycynix logging packages.

## What it contains

- `ILogger<T>`

## Purpose

This package allows other packages to depend on Raycynix logging contracts without depending on the logging implementation package.

## Usage

```csharp
public sealed class PriceCalculator(ILogger<PriceCalculator> logger)
{
    public decimal Calculate(string productId)
    {
        logger.Debug("Calculating price", new { ProductId = productId });
        return 42m;
    }
}
```

Use this package when you want to expose or consume the Raycynix typed logger contract without pulling in the Serilog-based implementation package.
