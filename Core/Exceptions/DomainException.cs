namespace Raycynix.Extensions.Core.Exceptions
{
    /// <summary>
    /// Represents an application-level exception that occurs due to domain logic errors.
    /// </summary>
    public class DomainException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that describes the domain violation.</param>
        public DomainException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public DomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}
