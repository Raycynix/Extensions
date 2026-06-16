namespace Raycynix.Extensions.Email.Abstractions.Exceptions;

/// <summary>
/// Represents an exception thrown when an email provider cannot complete a send operation.
/// </summary>
public sealed class EmailSendException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSendException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public EmailSendException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSendException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The underlying provider exception.</param>
    public EmailSendException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
