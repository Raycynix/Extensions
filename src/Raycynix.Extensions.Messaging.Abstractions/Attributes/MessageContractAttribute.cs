namespace Raycynix.Extensions.Messaging.Abstractions.Attributes;

/// <summary>
/// Declares the canonical messaging contract identity for a payload type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MessageContractAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageContractAttribute"/> class.
    /// </summary>
    /// <param name="name">The canonical contract name.</param>
    public MessageContractAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageContractAttribute"/> class.
    /// </summary>
    /// <param name="name">The canonical contract name.</param>
    /// <param name="version">The semantic contract version.</param>
    public MessageContractAttribute(string name, string version)
        : this(name)
    {
        Version = version;
    }

    /// <summary>
    /// Gets the canonical contract name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the explicit semantic contract version.
    /// </summary>
    public string? Version { get; }
}
