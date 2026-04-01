namespace Raycynix.Extensions.Messaging.Database.Configurations;

/// <summary>
/// Configures the database-backed messaging inbox and outbox persistence layer.
/// </summary>
public sealed class MessagingDatabasePersistenceConfiguration
{
    /// <summary>
    /// Gets or sets the inbox table name.
    /// </summary>
    public string InboxTableName { get; set; } = "raycynix_messaging_inbox";

    /// <summary>
    /// Gets or sets the outbox table name.
    /// </summary>
    public string OutboxTableName { get; set; } = "raycynix_messaging_outbox";

    /// <summary>
    /// Validates the persistence configuration.
    /// </summary>
    public void Validate()
    {
        ValidateIdentifier(InboxTableName, nameof(InboxTableName));
        ValidateIdentifier(OutboxTableName, nameof(OutboxTableName));
    }

    private static void ValidateIdentifier(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        if (!value.All(static character => char.IsLetterOrDigit(character) || character == '_'))
        {
            throw new InvalidOperationException(
                $"Messaging database table name '{value}' must contain only letters, digits, or underscores.");
        }
    }
}
