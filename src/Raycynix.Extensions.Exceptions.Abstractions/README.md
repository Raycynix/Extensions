# Raycynix.Extensions.Exceptions.Abstractions

`Raycynix.Extensions.Exceptions.Abstractions` contains the contracts and shared models used by the Raycynix exceptions packages.

## What it contains

- exception contracts such as `IRaycynixException`
- mapping and masking contracts
- retry and background execution contracts
- shared models such as `ErrorExecutionContext`, `ExceptionDetail`, and `RetryExecutionOptions`

## Purpose

This package lets other packages depend on Raycynix exception contracts without taking a dependency on the full implementation package.

## Logging

This package contains contracts, options, and shared models only. Runtime diagnostics belong to implementation packages, so this package does not add a logging dependency.

## Example

Libraries can depend only on abstractions when they need retry or masking contracts:

```csharp
public sealed class SyncService(IRetryExecutor retryExecutor)
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return retryExecutor.ExecuteAsync(
            operation: async token =>
            {
                await Task.Delay(10, token);
            },
            operationName: "SyncService.Execute",
            cancellationToken: cancellationToken);
    }
}
```
