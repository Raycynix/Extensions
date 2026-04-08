# Raycynix.Extensions.Exceptions

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

You can also customize mappings for domain-specific exceptions:

```csharp
builder.Services.AddRaycynixExceptions(options =>
{
    options.Map<UnauthorizedAccessException>(
        errorCode: "access_denied",
        message: "You do not have permission to perform this action.",
        statusCode: 403);
});
```

For ASP.NET Core request pipeline integration, add `Raycynix.Extensions.Exceptions.AspNetCore`.
