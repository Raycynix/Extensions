# Raycynix.Extensions.Configuration

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Configuration` contains the core typed-configuration registration helpers for Raycynix applications.

## What it contains

- `AddRaycynixConfiguration<TOptions>(...)`
- `AddRaycynixConfigurationValidator<TOptions, TValidator>()`
- `AddRaycynixConfigurationValidator<TOptions>(...)`
- typed configuration binding based on the standard Options pipeline
- support for default values through delegates and `IConfigurationDefaults<TOptions>`
- startup validation through standard `IValidateOptions<TOptions>` integration

## What it does not contain

- custom replacement for `IConfiguration`
- custom configuration providers
- feature flag infrastructure

## Usage

```csharp
builder.Services.AddRaycynixConfiguration<MyOptions>(
    builder.Configuration,
    configureDefaults: options =>
    {
        options.TimeoutSeconds = 30;
    });

builder.Services.AddRaycynixConfigurationValidator<MyOptions, MyOptionsValidator>();
```

By default, the package binds the section named after the options type, for example `MyOptions`.

You can override the section name explicitly when needed.

Registered validators run through the standard Options validation pipeline and are enforced on startup through `ValidateOnStart()`.
