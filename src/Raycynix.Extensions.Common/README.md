# Raycynix.Extensions.Common

`Raycynix.Extensions.Common` contains shared primitives and helper utilities used across Raycynix extension packages.

## What it contains

- `IOperationContext` and `OperationContext`
- assembly metadata helpers
- reusable disposable helpers

## Usage

```csharp
services.TryAddScoped<IOperationContext, OperationContext>();

var serviceName = AssemblyHelper.CurrentName();
var serviceVersion = AssemblyHelper.CurrentVersion();

using var _ = NoopDisposable.Instance;
```
