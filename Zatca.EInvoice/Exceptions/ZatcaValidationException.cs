using System;
using System.Collections.Generic;
using System.Linq;

namespace Zatca.EInvoice.Exceptions
{
    /// <summary>
    /// Exception thrown for ZATCA validation errors.
    /// </summary>
    public class ZatcaValidationException : ZatcaException
    {
        /// <summary>
        /// Gets the list of validation errors.
        /// </summary>
        public List<string> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaValidationException"/> class.
        /// </summary>
        public ZatcaValidationException() : this("Validation failed.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ZatcaValidationException(string message) : this(message, new List<string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaValidationException"/> class with a specified error message and errors list.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="errors">The list of validation errors.</param>
        public ZatcaValidationException(string message, List<string> errors) : base(message)
        {
            Errors = errors ?? new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaValidationException"/> class with a list of validation errors.
        /// </summary>
        /// <param name="errors">The list of validation errors.</param>
        public ZatcaValidationException(List<string> errors) : base(BuildMessage(errors))
        {
            Errors = errors ?? new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaValidationException"/> class with a single validation error.
        /// </summary>
        /// <param name="error">The validation error.</param>
        public ZatcaValidationException(string message, string error) : this(message, new List<string> { error })
        {
        }

        /// <summary>
        /// Adds an error to the errors list.
        /// </summary>
        /// <param name="error">The error to add.</param>
        /// <returns>The current exception instance.</returns>
        public ZatcaValidationException AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Errors.Add(error);
            }
            return this;
        }

        private static string BuildMessage(List<string> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return "Validation failed.";
            }

            if (errors.Count == 1)
            {
                return $"Validation failed: {errors[0]}";
            }

            return $"Validation failed with {errors.Count} errors: {string.Join(", ", errors.Take(3))}" +
                   (errors.Count > 3 ? "..." : "");
        }
    }
}
