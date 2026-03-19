# Raycynix.Extensions.Database.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database.Abstractions` contains the contracts used by the Raycynix database packages.

## What it contains

- `IDatabaseInitializer`
- `IConfigurator`
- `IGenericConfigurator<T>`

## Purpose

This package exists so database-related contracts can be shared without depending on the full database implementation package.
