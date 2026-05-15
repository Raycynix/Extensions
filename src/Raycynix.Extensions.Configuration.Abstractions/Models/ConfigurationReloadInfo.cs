using Raycynix.Extensions.Configuration.Abstractions.Enums;

namespace Raycynix.Extensions.Configuration.Abstractions.Models;

/// <summary>
/// Describes a retained runtime reload decision for a typed configuration registration.
/// </summary>
/// <param name="OptionsType">The options model type affected by the reload.</param>
/// <param name="OptionsName">The options name affected by the reload.</param>
/// <param name="Behavior">The reload behavior selected by the reload policies.</param>
/// <param name="Reason">The optional reason supplied by the reload policy.</param>
/// <param name="ChangedAtUtc">The UTC timestamp when the reload decision was recorded.</param>
public sealed record ConfigurationReloadInfo(
    Type OptionsType,
    string OptionsName,
    ConfigurationReloadBehavior Behavior,
    string? Reason,
    DateTimeOffset ChangedAtUtc);
