# Raycynix.Extensions.Exceptions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Exceptions` is the core exception handling package.

## What it contains

- `AddRaycynixExceptions(...)`
- exception mapping
- secure data masking
- transient exception classification
- retry execution services
- background task execution helpers
- common Raycynix exception types

## What it does not contain

- ASP.NET Core middleware
- `IApplicationBuilder` extensions
- HTTP response formatting

## Usage

```csharp
builder.Services.AddRaycynixExceptions(options =>
{
    options.Map<InvalidOperationException>(
        errorCode: "invalid_operation",
        message: "The operation is not valid.",
        statusCode: 400);
});
```

For ASP.NET Core request pipeline integration, add `Raycynix.Extensions.Exceptions.Web`.
