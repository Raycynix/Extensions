namespace Core.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs when model validation fails.
    /// </summary>
    public class ValidationException(Dictionary<string, string[]> errors) : Exception("Validation Failed.")
    {
        /// <summary>
        /// Gets the collection of validation errors grouped by field name.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
    }
}
