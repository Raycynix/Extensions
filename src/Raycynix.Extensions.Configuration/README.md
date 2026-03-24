# Raycynix.Extensions.Configuration

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Configuration` contains the core typed-configuration registration helpers for Raycynix applications.

## What it contains

- `AddRaycynixConfiguration<TOptions>(...)`
- typed configuration binding based on the standard Options pipeline
- support for default values through delegates and `IConfigurationDefaults<TOptions>`

## What it does not contain

- custom replacement for `IConfiguration`
- custom configuration providers
- validation-on-start policies
- feature flag infrastructure

## Usage

```csharp
builder.Services.AddRaycynixConfiguration<MyOptions>(
    builder.Configuration,
    configureDefaults: options =>
    {
        options.TimeoutSeconds = 30;
    });
```

By default, the package binds the section named after the options type, for example `MyOptions`.

You can override the section name explicitly when needed.
