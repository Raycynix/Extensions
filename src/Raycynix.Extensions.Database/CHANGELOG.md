# Changelog

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for database registration, initialization, and model configuration.
- Added optional diagnostics for active database provider resolution.

### Changed
- Removed the automatic `AddRaycynixLogging(...)` registration from database setup so applications can choose any Microsoft-compatible logging provider or run without one.

## 2.1.0
### Added
- Added explicit model and migrations assembly registration overloads.
- Added provider-specific validation hooks before provider configuration.
- Added default no-op database observability.

### Changed
- Improved DbContext registration safety for repeated registrations and custom contexts.
- Improved configurator dependency diagnostics.

## 2.0.0
### Added
- Added `RaycynixDatabaseContext` as the default Raycynix EF Core context.
- Added `AddRaycynixDatabase<TContext>(...)` for custom context registration.
- Added marker and explicit assembly overloads for separate model and migrations assemblies.
- Added fluent `AddAssembly(...)` model assembly registration.
- Added default no-op database observability through `IDatabaseObservability`.
- Added provider-specific validation invocation before connection-string resolution.

### Changed
- Moved shared database configuration and contracts to `Raycynix.Extensions.Database.Abstractions`.
- Moved tracing and metrics database observability to `Raycynix.Extensions.Database.Observability`.
- Replaced pooled context registration with scoped `AddDbContext` registration.
- Improved configurator dependency diagnostics for missing dependencies and circular dependencies.
- Rejects repeated database registration with a new configuration setup callback.

## 1.0.2
### Added
- Added package-level changelog tracking.

### Changed
- Updated database package examples and test stubs to align with the current logging abstractions.

### Fixed
- Updated test fake loggers so database test runs are no longer coupled to removed metadata-based logging APIs.
