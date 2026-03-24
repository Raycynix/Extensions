# Raycynix.Extensions.Configuration.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Configuration.Abstractions` contains the contracts used by the Raycynix configuration packages.

## What it contains

- `IConfigurationDefaults<TOptions>`
- `IConfigurationValidator<TOptions>`
- `ConfigurationValidationResult`

## Purpose

This package allows applications and libraries to provide their own default-value and validation strategies for typed configuration models without depending on the configuration implementation package.
