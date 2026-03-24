# Raycynix.Extensions.Configuration.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

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
