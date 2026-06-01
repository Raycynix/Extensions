# Raycynix.Extensions.Logging.Abstractions

`Raycynix.Extensions.Logging.Abstractions` contains the contracts and shared configuration models used by the Raycynix logging packages.

## What it contains

- `ILogger<T>`
- `LoggingConfiguration`
- `IRaycynixLoggingConfigurator`

## Purpose

This package allows other packages to depend on Raycynix logging contracts without depending on the Serilog-backed implementation package.

Optional logging integrations use `IRaycynixLoggingConfigurator` to participate in the Serilog pipeline configured by `Raycynix.Extensions.Logging`.

## Usage

```csharp
public sealed class PriceCalculator(ILogger<PriceCalculator> logger)
{
    public decimal Calculate(string productId)
    {
        logger.Debug("Calculating price for product {ProductId}", productId);
        return 42m;
    }
}
```

Use this package when you want to expose or consume the Raycynix typed logger contract, shared logging configuration, or optional Serilog integration contract.
