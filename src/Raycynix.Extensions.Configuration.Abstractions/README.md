# Raycynix.Extensions.Configuration.Abstractions

`Raycynix.Extensions.Configuration.Abstractions` contains the contracts used by the Raycynix configuration packages.

## What it contains

- `ConfigurationReloadBehaviorAttribute`
- `IApplicationEnvironment`
- `IConfigurationAccessor<TOptions>`
- `IConfigurationChangeHandler<TOptions>`
- `IConfigurationDefaults<TOptions>`
- `IFeatureFlagAccessor`
- `IConfigurationReloadPolicy<TOptions>`
- `IConfigurationValidator<TOptions>`
- `ConfigurationChangeContext<TOptions>`
- `ConfigurationReloadBehavior`
- `ConfigurationReloadResult`
- `ConfigurationValidationResult`

## Purpose

This package allows applications and libraries to provide their own typed access, feature-flag access, default-value, validation, reload-governance, and change-handling strategies for typed configuration models without depending on the configuration implementation package.

## Example

Libraries can depend only on abstractions and consume the current typed configuration through `IConfigurationAccessor<TOptions>`:

```csharp
public sealed class CacheService(IConfigurationAccessor<CacheOptions> configurationAccessor)
{
    public int GetDefaultTtlSeconds()
    {
        return configurationAccessor.Current.DefaultTtlSeconds;
    }
}
```

Runtime reload rules can also be expressed at the options model level:

```csharp
public sealed class CacheOptions
{
    [ConfigurationReloadBehavior(ConfigurationReloadBehavior.Reject)]
    public string ConnectionString { get; init; } = string.Empty;

    public int DefaultTtlSeconds { get; init; }
}
```
