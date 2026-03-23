# Raycynix.Extensions.Security.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Security.Abstractions` contains the contracts used by the Raycynix security packages.

## What it contains

- `ISecurityContext`
- `SecuritySubjectType`
- `SecurityClaimTypes`

## Purpose

This package allows other packages to depend on shared security contracts without depending on the security implementation package.

The security model assumes:

- request subjects are authenticated and known
- supported subject types are `User` and `Service`
- `Roles` are aggregates
- `Permissions` are the canonical access checks

Permissions should use a stable `resource.action` format, for example `users.read` or `orders.approve`.
