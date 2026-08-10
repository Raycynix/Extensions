# Changelog

## 3.0.0
### Added
- Added the shared `RaycynixMetrics` meter identity and factory helper.
- Added `Histogram<double>.MeasureDuration(...)` for recording elapsed seconds.

### Removed
- Removed `IMetricsService`, `IMetricCounter`, `IMetricGauge`, and `IMetricHistogram` in favor of standard .NET instruments.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

### Changed
- Removed the unused logging abstractions dependency from the metrics contracts package.

## 1.0.0
### Added
- Initial package release.
