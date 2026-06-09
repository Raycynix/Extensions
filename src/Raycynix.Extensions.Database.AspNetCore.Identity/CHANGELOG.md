# Changelog

## 2.1.0

- Added ASP.NET Core Identity database integration for `Raycynix.Extensions.Database`.
- Added the default `RaycynixIdentityDatabaseContext`.
- Added generic Raycynix Identity contexts for custom users, roles, keys, claims, logins, and tokens.
- Added `IRaycynixIdentityDatabaseContext` so identity registration remains scoped to Identity-compatible database contexts.
- Added `AddRaycynixIdentityDatabase` registration overloads for caller, marker, explicit model, and explicit migrations assembly scenarios.
- Integrated Raycynix model configurators, provider validation, initialization, and model-cache behavior with ASP.NET Core Identity contexts.
