# Raycynix.Extensions.Security.Abstractions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Security.Abstractions` contains the contracts used by the Raycynix security and secrets packages.

## What it contains

- `ISecurityContext`
- `ISecretProvider`
- `ISecretResolver`
- `SecuritySubjectType`
- `SecurityClaimTypes`

## Purpose

This package allows other packages to depend on shared security and secret-resolution contracts without depending on the implementation packages.

The security model assumes:

- request subjects are authenticated and known
- supported subject types are `User` and `Service`
- `Roles` are aggregates
- `Permissions` are the canonical access checks

The secret model assumes:

- applications read secrets through shared abstractions
- a provider represents a single source
- a resolver aggregates providers and returns the first available secret
- built-in providers can target local env, GitHub Actions, and TeamCity-style injection

Permissions should use a stable `resource.action` format, for example `users.read` or `orders.approve`.
