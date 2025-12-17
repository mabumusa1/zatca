using System.Collections.Generic;

namespace Zatca.EInvoice.Validation
{
    /// <summary>
    /// Interface for invoice validators.
    /// </summary>
    public interface IInvoiceValidator
    {
        /// <summary>
        /// Validates the invoice data.
        /// </summary>
        /// <param name="data">The invoice data dictionary.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether validation was successful.</returns>
        ValidationResult Validate(Dictionary<string, object> data);

        /// <summary>
        /// Validates the invoice data and throws an exception if validation fails.
        /// </summary>
        /// <param name="data">The invoice data dictionary.</param>
        /// <exception cref="System.ArgumentException">Thrown when validation fails.</exception>
        void ValidateAndThrow(Dictionary<string, object> data);
    }
}
