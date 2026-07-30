# Changelog

## 3.0.0
### Changed
- Replaced `LoggingConfiguration` with `LoggingOptions`.
- Moved the shared model from the `Configurations` namespace to `Options`.
- Updated `IRaycynixLoggingConfigurator` to receive `LoggingOptions`.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

## 2.1.0
### Added
- Added `LoggingConfiguration` as the shared base logging configuration model.
- Added `IRaycynixLoggingConfigurator` for optional packages that extend the Serilog pipeline.

## 2.0.0
### Added
- Initial package release.
