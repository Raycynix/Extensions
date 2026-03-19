# Raycynix.Extensions.Exceptions.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Exceptions.Abstractions` contains the contracts and shared models used by the Raycynix exceptions packages.

## What it contains

- exception contracts such as `IRaycynixException`
- mapping and masking contracts
- retry and background execution contracts
- shared models such as `ErrorExecutionContext`, `ExceptionDetail`, and `RetryExecutionOptions`

## Purpose

This package lets other packages depend on Raycynix exception contracts without taking a dependency on the full implementation package.
