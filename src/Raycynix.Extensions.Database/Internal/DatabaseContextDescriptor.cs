namespace Raycynix.Extensions.Database.Internal;

/// <summary>
/// Captures the Raycynix database context type selected for the application.
/// </summary>
internal sealed class DatabaseContextDescriptor
{
    /// <summary>
    /// Gets the concrete context type registered for the Raycynix database infrastructure.
    /// </summary>
    public required Type ContextType { get; init; }
}
