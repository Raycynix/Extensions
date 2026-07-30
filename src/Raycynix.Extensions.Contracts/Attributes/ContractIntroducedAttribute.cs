using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.Attributes;

/// <summary>
/// Marks the contract member or type version in which it was introduced.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ContractIntroducedAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContractIntroducedAttribute"/> class.
    /// </summary>
    /// <param name="version">The semantic contract version.</param>
    public ContractIntroducedAttribute(string version)
    {
        Version = ContractVersion.Parse(version).ToString();
    }

    /// <summary>
    /// Gets the semantic contract version in which the member or type was introduced.
    /// </summary>
    public string Version { get; }
}
