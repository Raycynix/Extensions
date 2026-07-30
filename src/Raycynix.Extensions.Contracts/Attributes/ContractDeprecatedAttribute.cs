using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.Attributes;

/// <summary>
/// Marks the contract member or type as deprecated for future removal.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ContractDeprecatedAttribute : Attribute
{
    private string? _removalVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContractDeprecatedAttribute"/> class.
    /// </summary>
    /// <param name="deprecatedSinceVersion">The semantic contract version in which deprecation started.</param>
    public ContractDeprecatedAttribute(string deprecatedSinceVersion)
    {
        DeprecatedSinceVersion = ContractVersion.Parse(deprecatedSinceVersion).ToString();
    }

    /// <summary>
    /// Gets the semantic contract version in which the member or type became deprecated.
    /// </summary>
    public string DeprecatedSinceVersion { get; }

    /// <summary>
    /// Gets or sets the planned removal version.
    /// </summary>
    public string? RemovalVersion
    {
        get => _removalVersion;
        set => _removalVersion = value is null
            ? null
            : ContractVersion.Parse(value).ToString();
    }

    /// <summary>
    /// Gets or sets the deprecation reason.
    /// </summary>
    public string? Reason { get; set; }
}
