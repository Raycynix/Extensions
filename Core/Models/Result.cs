namespace Raycynix.Extensions.Core.Models
{

    /// <summary>
    /// Represents a basic result of an operation with status and message.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess => Status == ResultStatus.Success;

        /// <summary>
        /// Gets the status of the operation.
        /// </summary>
        public ResultStatus Status { get; set; }

        /// <summary>
        /// Optional message describing the result or error details.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// </summary>
        /// <param name="status">The result status.</param>
        /// <param name="message">An optional result message.</param>
        protected Result(ResultStatus status, string? message)
        {
            Status = status;
            Message = message;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="message">Optional success message.</param>
        public static Result Success(string? message = null) => new(ResultStatus.Success, message);

        /// <summary>
        /// Creates a failure result.
        /// </summary>
        /// <param name="message">Optional error message.</param>
        public static Result Failure(string? message = null) => new(ResultStatus.Failure, message);
    }

    /// <summary>
    /// Represents a generic result of an operation with optional data payload.
    /// </summary>
    /// <typeparam name="T">The type of data associated with the result.</typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// Gets the data returned by the operation.
        /// </summary>
        public T? Data { get; set; }

        private Result(ResultStatus status, T? data = default, string? message = null)
            : base(status, message)
        {
            Data = data;
        }

        /// <summary>
        /// Creates a successful result with data.
        /// </summary>
        public static Result<T> Success(T? data = default, string? message = null) => new(ResultStatus.Success, data, message);

        /// <summary>
        /// Creates a failed result without data.
        /// </summary>
        public new static Result<T> Failure(string? message = null) => new(ResultStatus.Failure, default, message);
    }
}
