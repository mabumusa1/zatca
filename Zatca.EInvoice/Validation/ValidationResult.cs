using System.Collections.Generic;

namespace Zatca.EInvoice.Validation
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the list of validation errors.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        public ValidationResult()
        {
            IsValid = true;
            Errors = new List<string>();
        }

        /// <summary>
        /// Adds an error to the validation result and sets IsValid to false.
        /// </summary>
        /// <param name="error">The error message to add.</param>
        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <returns>A <see cref="ValidationResult"/> indicating success.</returns>
        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates a failed validation result with a single error.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating failure.</returns>
        public static ValidationResult Failure(string error)
        {
            var result = new ValidationResult();
            result.AddError(error);
            return result;
        }

        /// <summary>
        /// Creates a failed validation result with multiple errors.
        /// </summary>
        /// <param name="errors">The collection of error messages.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating failure.</returns>
        public static ValidationResult Failure(IEnumerable<string> errors)
        {
            var result = new ValidationResult();
            foreach (var error in errors)
            {
                result.AddError(error);
            }
            return result;
        }
    }
}
