# Raycynix.Extensions.Common

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

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
