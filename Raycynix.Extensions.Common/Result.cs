namespace Raycynix.Extensions.Common;

/// <summary>
/// Represents the outcome of an operation, providing either a successful value or an error message.
/// </summary>
/// <typeparam name="T">
/// The type of the result value when the operation is successful.
/// </typeparam>
public class Result<T>
{
    /// Gets a value indicating whether the operation was successful.
    /// If true, the operation completed successfully, and the Value property contains the result.
    /// If false, the operation failed, and the ErrorMessage property provides details about the failure.
    public bool IsSuccess { get; }

    /// Gets the result value if the operation was successful.
    /// If the operation failed, this property will be null.
    /// This property represents the primary output or data resulting from a successful operation.
    public T? Value { get; }

    /// Gets the error message if the operation failed.
    /// If the operation was successful, this property is null.
    /// This property provides additional information about the reason for the failure,
    /// allowing for more detailed error handling or user feedback.
    public string? ErrorMessage { get; }

    ///<summary>
    /// Represents the outcome of an operation, providing either a successful value or an error message.
    /// Encapsulates the success state, a result value, and an error message.
    /// </summary>
    /// <typeparam name="T">The type of the result value when the operation is successful.</typeparam>
    /// <property>
    /// IsSuccess: Gets a value indicating whether the operation was successful.
    /// Value: Gets the result value if the operation was successful. If the operation failed, this will be null.
    /// ErrorMessage: Gets the error message if the operation failed. If the operation was successful, this will be null.
    /// </property>
    /// <method name="Success" param="value">
    /// Success(T value): Creates a new successful result containing the specified value.
    /// <param name="value">The value to represent a successful operation.</param>
    /// <return>A instance indicating success with the provided value.</return>
    /// </method>
    protected Result(bool isSuccess, T? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    /// Creates a new successful result containing the specified value.
    /// <param name="value">The value to represent a successful operation.</param>
    /// <return>A Result<T> instance indicating success with the provided value.</return>
    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
}