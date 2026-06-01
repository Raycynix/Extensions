namespace Raycynix.Extensions.Configuration.Abstractions.Models;

/// <summary>
/// Describes a typed configuration registration tracked by diagnostics.
/// </summary>
/// <param name="OptionsType">The registered options model type.</param>
/// <param name="SectionName">The configuration section used to bind the options.</param>
/// <param name="OptionsName">The options name used by the Options pipeline.</param>
/// <param name="RequiredSection">Whether the configuration section was required at registration time.</param>
public sealed record ConfigurationRegistrationInfo(
    Type OptionsType,
    string SectionName,
    string OptionsName,
    bool RequiredSection);
