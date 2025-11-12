namespace Core.Models
{
    /// <summary>
    /// Represents the status of an operation result.
    /// </summary>
    public enum ResultStatus
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The operation failed.
        /// </summary>
        Failure = 1,

        /// <summary>
        /// The requested resource was not found.
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// The operation failed due to validation errors.
        /// </summary>
        ValidationError = 3,

        /// <summary>
        /// The operation was rejected due to missing authentication.
        /// </summary>
        Unauthorized = 4,

        /// <summary>
        /// The operation was rejected due to insufficient permissions.
        /// </summary>
        Forbidden = 5,

        /// <summary>
        /// The operation conflicted with the current state of the resource.
        /// </summary>
        Conflict = 6
    }
}
