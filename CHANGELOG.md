# Changelog

All notable changes to this repository will be documented in this file.

The format is based on Keep a Changelog, adapted for this multi-package repository.

## [Unreleased]

### Added

- Placeholder for upcoming changes before the next published release.

## [1.0.0] - 2026-04-08

### Added

- Established `v1` package baselines across the repository with aligned metadata, README files, XML documentation, and package-level tests.
- Added GitHub issue forms for bug reports and feature requests.
- Added a shared root release-notes flow through `Directory.Build.props`.
- Added a root repository changelog for future package and release tracking.

### Changed

- Split database provider implementations into separate packages and removed provider selection through `DatabaseProvider`.
- Standardized configuration binding and validation patterns across package families.
- Aligned messaging transport and persistence packages around configuration-based registration and package-level binding coverage.
- Consolidated shared no-op disposable behavior into `Raycynix.Extensions.Common`.
- Removed package-level TeamCity badges from package README files and kept repository-level status only in the root README.

### Fixed

- Restored PostgreSQL provider option binding from the nested database configuration section.
- Ignored empty SQLite `Mode` and `Cache` values during provider binding instead of throwing on startup.
- Fixed EF Core model cache behavior for runtime table name overrides in database configurators.
- Fixed database outbox dispatch leasing to avoid duplicate concurrent leases.
- Allowed anonymous ASP.NET Core security context resolution without throwing for unauthenticated requests.
- Fixed Kafka packaging so native `librdkafka.redist` assets are not duplicated during pack.

### Package Notes

- `Common`: shared operation context, assembly helpers, and reusable disposable utilities with dedicated tests.
- `Configuration`: typed configuration, feature flags, validation, reload handling, and ASP.NET Core integration aligned for `v1`.
- `Contracts`: core contracts and ASP.NET Core integration documented and covered for versioned and plain contract flows.
- `Database`: core, abstractions, hosting, ASP.NET Core, and all relational provider packages stabilized for `v1`.
- `Exceptions`: core and ASP.NET Core exception handling packages aligned with docs, coverage, and structured error behavior.
- `Logging`: logging packages aligned for `v1`, including configuration validation.
- `Messaging`: core, abstractions, database persistence, Kafka, RabbitMQ, HTTP JSON, and gRPC packages aligned for `v1`.
- `Metrics`: metrics packages aligned for `v1`, including configuration validation and ASP.NET Core coverage.
- `Observability`: shared and ASP.NET Core observability packages aligned for `v1`.
- `Security` and `Secrets`: packages aligned for `v1`, including provider-specific secret tests and safe anonymous security context behavior.
- `Tracing`: tracing packages aligned for `v1` with docs, metadata, and coverage.
