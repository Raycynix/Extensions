# Changelog

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics for JWT challenges, security context mapping, dynamic policy resolution, authorization handlers, and authorization failure responses.

## 1.0.0
### Added
- Added an ASP.NET Core example that demonstrates JWT security registration, request pipeline wiring, and protected endpoints.
- Added example endpoints that show `RequireRaycynixAuthorization(...)`, dynamic permission policies, and request-scoped `ISecurityContext` usage.

### Changed
- Added example configuration for JWT authority, issuer, audience, and HTTPS metadata handling.
