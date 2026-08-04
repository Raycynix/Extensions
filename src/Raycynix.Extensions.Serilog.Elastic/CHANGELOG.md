# Changelog

## 3.0.0

### Added

- Added integration with the officially supported `Elastic.Serilog.Sinks`.
- Added direct Elasticsearch node connections.
- Added optional Elasticsearch node sniffing.
- Added Elastic Cloud support.
- Added API key authentication.
- Added basic authentication.
- Added ECS data stream configuration.
- Added ILM policy configuration.
- Added sink-specific minimum level.
- Added ECS host, process, user, and Activity configuration.
- Added channel buffer configuration.
- Added proxy configuration.
- Added certificate fingerprint configuration.
- Added transport and sink escape hatches.
- Added startup configuration validation.

### Changed

- Renamed the package from  `Raycynix.Extensions.Logging.Elastic` to `Raycynix.Extensions.Serilog.Elastic`.
- Removed the dependency on the old Raycynix logging abstraction.
- Moved all sink-specific dependencies out of the core Serilog package.

### Fixed

- Prevented the Elasticsearch configurator from resolving options through the logging provider during Serilog startup.

## 2.2.0

### Added

- Starts unified versioning for Raycynix packages from this release.

## 2.1.0

### Added

- Initial Elasticsearch logging integration package.
- Added `AddElastic()` for registering the Serilog Elasticsearch sink through `LoggingBuilder`.
- Added `ElasticConfiguration`, bound from `LoggingConfiguration:ElasticConfiguration`.
