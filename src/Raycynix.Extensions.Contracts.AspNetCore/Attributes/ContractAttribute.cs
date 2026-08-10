using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Attributes;

/// <summary>
/// Declares contract metadata for an MVC action or controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ContractAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContractAttribute"/> class.
    /// </summary>
    /// <param name="name">The canonical contract name.</param>
    /// <param name="version">The semantic contract version.</param>
    public ContractAttribute(string name, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Metadata = new ContractMetadata
        {
            Name = name.Trim(),
            Version = ContractVersion.Parse(version)
        };
    }

    /// <summary>
    /// Gets the declared contract metadata.
    /// </summary>
    public ContractMetadata Metadata { get; }
}
