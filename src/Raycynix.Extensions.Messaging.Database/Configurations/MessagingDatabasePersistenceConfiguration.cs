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
    /// Gets or sets a value indicating whether background cleanup should remove expired inbox and outbox rows.
    /// </summary>
    public bool EnableCleanup { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval between cleanup cycles.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the maximum number of rows removed per inbox or outbox cleanup batch.
    /// </summary>
    public int CleanupBatchSize { get; set; } = 500;

    /// <summary>
    /// Gets or sets the retention period for successfully processed inbox rows.
    /// </summary>
    public TimeSpan ProcessedInboxRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets the retention period for failed inbox rows.
    /// </summary>
    public TimeSpan FailedInboxRetention { get; set; } = TimeSpan.FromDays(14);

    /// <summary>
    /// Gets or sets the retention period for dispatched outbox rows.
    /// </summary>
    public TimeSpan DispatchedOutboxRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets the retention period for failed outbox rows.
    /// </summary>
    public TimeSpan FailedOutboxRetention { get; set; } = TimeSpan.FromDays(14);

    /// <summary>
    /// Validates the persistence configuration.
    /// </summary>
    public void Validate()
    {
        ValidateIdentifier(InboxTableName, nameof(InboxTableName));
        ValidateIdentifier(OutboxTableName, nameof(OutboxTableName));
        ValidatePositive(CleanupInterval, nameof(CleanupInterval));
        ValidatePositive(ProcessedInboxRetention, nameof(ProcessedInboxRetention));
        ValidatePositive(FailedInboxRetention, nameof(FailedInboxRetention));
        ValidatePositive(DispatchedOutboxRetention, nameof(DispatchedOutboxRetention));
        ValidatePositive(FailedOutboxRetention, nameof(FailedOutboxRetention));

        if (CleanupBatchSize <= 0)
        {
            throw new InvalidOperationException("Messaging database cleanup batch size must be greater than zero.");
        }
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

    private static void ValidatePositive(TimeSpan value, string parameterName)
    {
        if (value <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                $"Messaging database persistence option '{parameterName}' must be greater than zero.");
        }
    }
}
