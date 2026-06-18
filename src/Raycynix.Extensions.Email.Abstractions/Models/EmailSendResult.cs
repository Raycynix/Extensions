using System.Collections.ObjectModel;

namespace Raycynix.Extensions.Email.Abstractions.Models;

/// <summary>
/// Represents the result of an email send operation.
/// </summary>
public sealed class EmailSendResult
{
    private static readonly IReadOnlyDictionary<string, string> _emptyMetadata =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    private EmailSendResult(
        bool succeeded,
        string provider,
        string? messageId,
        string? errorCode,
        string? errorMessage,
        IReadOnlyDictionary<string, string> metadata)
    {
        Succeeded = succeeded;
        Provider = provider;
        MessageId = messageId;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Metadata = metadata;
    }

    /// <summary>
    /// Gets a value indicating whether the provider accepted the message.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the logical provider name.
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// Gets the provider message identifier.
    /// </summary>
    public string? MessageId { get; }

    /// <summary>
    /// Gets the provider error code for expected send failures.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the provider error message for expected send failures.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets provider-specific metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>
    /// Creates a successful send result.
    /// </summary>
    /// <param name="provider">The logical provider name.</param>
    /// <param name="messageId">The optional provider message identifier.</param>
    /// <param name="metadata">The optional provider-specific metadata.</param>
    /// <returns>The successful send result.</returns>
    public static EmailSendResult Success(
        string provider,
        string? messageId = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);

        return new EmailSendResult(
            succeeded: true,
            provider: provider,
            messageId: messageId,
            errorCode: null,
            errorMessage: null,
            metadata: CopyMetadata(metadata));
    }

    /// <summary>
    /// Creates a failed send result.
    /// </summary>
    /// <param name="provider">The logical provider name.</param>
    /// <param name="errorMessage">The provider error message.</param>
    /// <param name="errorCode">The provider error code.</param>
    /// <param name="metadata">The optional provider-specific metadata.</param>
    /// <returns>The failed send result.</returns>
    public static EmailSendResult Failure(
        string provider,
        string errorMessage,
        string? errorCode = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return new EmailSendResult(
            succeeded: false,
            provider: provider,
            messageId: null,
            errorCode: errorCode,
            errorMessage: errorMessage,
            metadata: CopyMetadata(metadata));
    }

    private static IReadOnlyDictionary<string, string> CopyMetadata(
        IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return _emptyMetadata;
        }

        return new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata));
    }
}
