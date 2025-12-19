using System;
using System.Collections.Generic;

namespace Zatca.EInvoice.Validation
{
    /// <summary>
    /// Validates the financial amounts in the invoice data including monetary totals,
    /// tax amounts, and invoice lines to ensure correctness and consistency.
    ///
    /// This validator ensures that:
    /// - Legal monetary totals are numeric, non-negative, and consistent.
    /// - The taxInclusiveAmount is approximately equal to the sum of taxExclusiveAmount and taxTotal.
    /// - Each invoice line has valid numeric values for quantity, price, and line extension amounts,
    ///   and that calculations (such as price * quantity) are consistent with the provided amounts.
    /// </summary>
    public class InvoiceAmountValidator
    {
        private const decimal Tolerance = 0.01m;
        private const string LegalMonetaryTotal = "legalMonetaryTotal";
        private const string LineExtensionAmount = "lineExtensionAmount";
        private const string TaxExclusiveAmount = "taxExclusiveAmount";
        private const string TaxInclusiveAmount = "taxInclusiveAmount";
        private const string TaxTotal = "taxTotal";
        private const string TaxAmount = "taxAmount";
        private const string Quantity = "quantity";
        private const string Price = "price";
        private const string Amount = "amount";
        private const string RoundingAmount = "roundingAmount";

        /// <summary>
        /// Validates the monetary totals in the invoice.
        /// </summary>
        /// <param name="data">The invoice data dictionary.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether validation was successful.</returns>
        public ValidationResult ValidateMonetaryTotals(Dictionary<string, object> data)
        {
            var result = new ValidationResult();

            try
            {
                ValidateMonetaryTotalsInternal(data, result);
            }
            catch (Exception ex)
            {
                result.AddError($"Unexpected error during monetary totals validation: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates the monetary totals in the invoice and throws an exception if validation fails.
        /// </summary>
        /// <param name="data">The invoice data dictionary.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        public void ValidateMonetaryTotalsAndThrow(Dictionary<string, object> data)
        {
            // Check that the Legal Monetary Total section exists.
            if (!data.TryGetValue(LegalMonetaryTotal, out var lmtObj))
            {
                throw new ArgumentException("Legal Monetary Total section is missing.");
            }

            if (lmtObj is not Dictionary<string, object> lmt)
            {
                throw new ArgumentException("Legal Monetary Total must be a valid object.");
            }

            var requiredFields = new[] { LineExtensionAmount, TaxExclusiveAmount, TaxInclusiveAmount, "payableAmount" };

            // Ensure that required fields exist, are numeric, and non-negative.
            foreach (var field in requiredFields)
            {
                if (!lmt.TryGetValue(field, out var fieldValue) || !TryGetDecimal(fieldValue, out decimal value))
                {
                    throw new ArgumentException($"Legal Monetary Total field '{field}' must be a numeric value.");
                }

                if (value < 0)
                {
                    throw new ArgumentException($"Legal Monetary Total field '{field}' cannot be negative.");
                }
            }

            // Retrieve taxTotal amount if available.
            decimal taxTotalAmount = 0.0m;
            if (data.TryGetValue(TaxTotal, out var taxTotalObj) &&
                taxTotalObj is Dictionary<string, object> taxTotal &&
                taxTotal.TryGetValue(TaxAmount, out var taxAmountValue) &&
                TryGetDecimal(taxAmountValue, out decimal taxAmount))
            {
                taxTotalAmount = taxAmount;
            }

            decimal taxExclusiveAmount = GetDecimal(lmt[TaxExclusiveAmount]);
            decimal expectedTaxInclusive = taxExclusiveAmount + taxTotalAmount;
            decimal actualTaxInclusive = GetDecimal(lmt[TaxInclusiveAmount]);

            // Allow a small difference (e.g., 0.01) due to rounding differences.
            if (Math.Abs(expectedTaxInclusive - actualTaxInclusive) > Tolerance)
            {
                throw new ArgumentException(
                    $"The taxInclusiveAmount ({actualTaxInclusive}) does not equal taxExclusiveAmount ({taxExclusiveAmount}) plus taxTotal ({taxTotalAmount})."
                );
            }
        }

        /// <summary>
        /// Validates invoice lines for numeric consistency and calculation correctness.
        /// </summary>
        /// <param name="invoiceLines">Array of invoice lines.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether validation was successful.</returns>
        public ValidationResult ValidateInvoiceLines(IList<object> invoiceLines)
        {
            var result = new ValidationResult();

            try
            {
                ValidateInvoiceLinesInternal(invoiceLines, result);
            }
            catch (Exception ex)
            {
                result.AddError($"Unexpected error during invoice lines validation: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates invoice lines for numeric consistency and calculation correctness and throws an exception if validation fails.
        /// </summary>
        /// <param name="invoiceLines">Array of invoice lines.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        public void ValidateInvoiceLinesAndThrow(IList<object> invoiceLines)
        {
            for (int index = 0; index < invoiceLines.Count; index++)
            {
                if (invoiceLines[index] is Dictionary<string, object> line)
                {
                    ValidateSingleLineAndThrow(line, index);
                }
            }
        }

        private void ValidateSingleLineAndThrow(Dictionary<string, object> line, int index)
        {
            ValidateLineNumericFieldsAndThrow(line, index);
            var priceAmount = ValidateLinePriceAndThrow(line, index);
            ValidateLineExtensionCalculationAndThrow(line, index, priceAmount);
            ValidateLineItemTaxPercentAndThrow(line, index);
            ValidateLineTaxTotalAndThrow(line, index);
        }

        private void ValidateLineNumericFieldsAndThrow(Dictionary<string, object> line, int index)
        {
            var numericFields = new[] { Quantity, LineExtensionAmount };
            foreach (var field in numericFields)
            {
                if (!line.TryGetValue(field, out var fieldValue) || !TryGetDecimal(fieldValue, out decimal value))
                {
                    throw new ArgumentException($"Invoice Line [{index}] field '{field}' must be a numeric value.");
                }
                if (value < 0)
                {
                    throw new ArgumentException($"Invoice Line [{index}] field '{field}' cannot be negative.");
                }
            }
        }

        private decimal ValidateLinePriceAndThrow(Dictionary<string, object> line, int index)
        {
            if (!line.TryGetValue(Price, out var priceObj) || priceObj is not Dictionary<string, object> price)
            {
                throw new ArgumentException($"Invoice Line [{index}] must have a valid price object.");
            }
            if (!price.TryGetValue(Amount, out var amountObj) || !TryGetDecimal(amountObj, out decimal priceAmount))
            {
                throw new ArgumentException($"Invoice Line [{index}] Price amount must be a numeric value.");
            }
            if (priceAmount < 0)
            {
                throw new ArgumentException($"Invoice Line [{index}] Price amount cannot be negative.");
            }
            return priceAmount;
        }

        private void ValidateLineExtensionCalculationAndThrow(Dictionary<string, object> line, int index, decimal priceAmount)
        {
            decimal quantity = GetDecimal(line[Quantity]);
            decimal expectedLineExtension = priceAmount * quantity;
            decimal providedLineExtension = GetDecimal(line[LineExtensionAmount]);

            if (Math.Abs(expectedLineExtension - providedLineExtension) > Tolerance)
            {
                throw new ArgumentException(
                    $"Invoice Line [{index}] lineExtensionAmount is incorrect. Expected {expectedLineExtension}, got {providedLineExtension}.");
            }
        }

        private void ValidateLineItemTaxPercentAndThrow(Dictionary<string, object> line, int index)
        {
            if (!line.TryGetValue("item", out var itemObj) || itemObj is not Dictionary<string, object> item)
                return;
            if (!item.TryGetValue("taxPercent", out var taxPercentObj))
                return;

            if (!TryGetDecimal(taxPercentObj, out decimal taxPercent))
            {
                throw new ArgumentException($"Invoice Line [{index}] item taxPercent must be a numeric value.");
            }
            if (taxPercent < 0 || taxPercent > 100)
            {
                throw new ArgumentException($"Invoice Line [{index}] item taxPercent must be between 0 and 100.");
            }
        }

        private void ValidateLineTaxTotalAndThrow(Dictionary<string, object> line, int index)
        {
            if (!line.TryGetValue(TaxTotal, out var taxTotalObj) || taxTotalObj is not Dictionary<string, object> taxTotal)
            {
                throw new ArgumentException($"Invoice Line [{index}] must have a valid taxTotal object.");
            }
            if (!taxTotal.TryGetValue(TaxAmount, out var taxAmountObj) || !TryGetDecimal(taxAmountObj, out decimal taxLineAmount))
            {
                throw new ArgumentException($"Invoice Line [{index}] TaxTotal taxAmount must be a numeric value.");
            }
            if (taxLineAmount < 0)
            {
                throw new ArgumentException($"Invoice Line [{index}] TaxTotal taxAmount cannot be negative.");
            }

            if (!taxTotal.TryGetValue(RoundingAmount, out var roundingObj) || !TryGetDecimal(roundingObj, out decimal roundingAmount))
            {
                throw new ArgumentException($"Invoice Line [{index}] TaxTotal roundingAmount must be a numeric value.");
            }

            decimal providedLineExtension = GetDecimal(line[LineExtensionAmount]);
            decimal expectedRounding = providedLineExtension + taxLineAmount;
            if (Math.Abs(expectedRounding - roundingAmount) > Tolerance)
            {
                throw new ArgumentException(
                    $"Invoice Line [{index}] roundingAmount is incorrect. Expected {expectedRounding}, got {roundingAmount}.");
            }
        }

        private void ValidateMonetaryTotalsInternal(Dictionary<string, object> data, ValidationResult result)
        {
            // Check that the Legal Monetary Total section exists.
            if (!data.TryGetValue(LegalMonetaryTotal, out var lmtObj))
            {
                result.AddError("Legal Monetary Total section is missing.");
                return;
            }

            if (lmtObj is not Dictionary<string, object> lmt)
            {
                result.AddError("Legal Monetary Total must be a valid object.");
                return;
            }

            var requiredFields = new[] { LineExtensionAmount, TaxExclusiveAmount, TaxInclusiveAmount, "payableAmount" };

            // Ensure that required fields exist, are numeric, and non-negative.
            foreach (var field in requiredFields)
            {
                if (!lmt.TryGetValue(field, out var fieldValueObj) || !TryGetDecimal(fieldValueObj, out decimal value))
                {
                    result.AddError($"Legal Monetary Total field '{field}' must be a numeric value.");
                    continue;
                }

                if (value < 0)
                {
                    result.AddError($"Legal Monetary Total field '{field}' cannot be negative.");
                }
            }

            if (!result.IsValid)
            {
                return;
            }

            // Retrieve taxTotal amount if available.
            decimal taxTotalAmount = 0.0m;
            if (data.TryGetValue(TaxTotal, out var taxTotalInternalObj) &&
                taxTotalInternalObj is Dictionary<string, object> taxTotalInternal &&
                taxTotalInternal.TryGetValue(TaxAmount, out var taxAmountInternalValue) &&
                TryGetDecimal(taxAmountInternalValue, out decimal taxAmountInternal))
            {
                taxTotalAmount = taxAmountInternal;
            }

            decimal taxExclusiveAmount = GetDecimal(lmt[TaxExclusiveAmount]);
            decimal expectedTaxInclusive = taxExclusiveAmount + taxTotalAmount;
            decimal actualTaxInclusive = GetDecimal(lmt[TaxInclusiveAmount]);

            // Allow a small difference (e.g., 0.01) due to rounding differences.
            if (Math.Abs(expectedTaxInclusive - actualTaxInclusive) > Tolerance)
            {
                result.AddError(
                    $"The taxInclusiveAmount ({actualTaxInclusive}) does not equal taxExclusiveAmount ({taxExclusiveAmount}) plus taxTotal ({taxTotalAmount})."
                );
            }
        }

        private void ValidateInvoiceLinesInternal(IList<object> invoiceLines, ValidationResult result)
        {
            for (int index = 0; index < invoiceLines.Count; index++)
            {
                if (invoiceLines[index] is Dictionary<string, object> line)
                {
                    ValidateSingleLineInternal(line, index, result);
                }
            }
        }

        private void ValidateSingleLineInternal(Dictionary<string, object> line, int index, ValidationResult result)
        {
            ValidateLineNumericFieldsInternal(line, index, result);
            var priceAmount = ValidateLinePriceInternal(line, index, result);
            if (priceAmount.HasValue)
            {
                ValidateLineExtensionCalculationInternal(line, index, priceAmount.Value, result);
            }
            ValidateLineItemTaxPercentInternal(line, index, result);
            ValidateLineTaxTotalInternal(line, index, result);
        }

        private void ValidateLineNumericFieldsInternal(Dictionary<string, object> line, int index, ValidationResult result)
        {
            var numericFields = new[] { Quantity, LineExtensionAmount };
            foreach (var field in numericFields)
            {
                if (!line.TryGetValue(field, out var fieldValue) || !TryGetDecimal(fieldValue, out decimal value))
                {
                    result.AddError($"Invoice Line [{index}] field '{field}' must be a numeric value.");
                    continue;
                }
                if (value < 0)
                {
                    result.AddError($"Invoice Line [{index}] field '{field}' cannot be negative.");
                }
            }
        }

        private decimal? ValidateLinePriceInternal(Dictionary<string, object> line, int index, ValidationResult result)
        {
            if (!line.TryGetValue(Price, out var priceObj) || priceObj is not Dictionary<string, object> price)
            {
                result.AddError($"Invoice Line [{index}] must have a valid price object.");
                return null;
            }
            if (!price.TryGetValue(Amount, out var amountObj) || !TryGetDecimal(amountObj, out decimal priceAmount))
            {
                result.AddError($"Invoice Line [{index}] Price amount must be a numeric value.");
                return null;
            }
            if (priceAmount < 0)
            {
                result.AddError($"Invoice Line [{index}] Price amount cannot be negative.");
            }
            return priceAmount;
        }

        private void ValidateLineExtensionCalculationInternal(Dictionary<string, object> line, int index, decimal priceAmount, ValidationResult result)
        {
            if (!TryGetDecimal(line[Quantity], out decimal quantity) ||
                !TryGetDecimal(line[LineExtensionAmount], out decimal providedLineExtension))
                return;

            decimal expectedLineExtension = priceAmount * quantity;
            if (Math.Abs(expectedLineExtension - providedLineExtension) > Tolerance)
            {
                result.AddError(
                    $"Invoice Line [{index}] lineExtensionAmount is incorrect. Expected {expectedLineExtension}, got {providedLineExtension}.");
            }
        }

        private void ValidateLineItemTaxPercentInternal(Dictionary<string, object> line, int index, ValidationResult result)
        {
            if (!line.TryGetValue("item", out var itemObj) || itemObj is not Dictionary<string, object> item)
                return;
            if (!item.TryGetValue("taxPercent", out var taxPercentObj))
                return;

            if (!TryGetDecimal(taxPercentObj, out decimal taxPercent))
            {
                result.AddError($"Invoice Line [{index}] item taxPercent must be a numeric value.");
            }
            else if (taxPercent < 0 || taxPercent > 100)
            {
                result.AddError($"Invoice Line [{index}] item taxPercent must be between 0 and 100.");
            }
        }

        private void ValidateLineTaxTotalInternal(Dictionary<string, object> line, int index, ValidationResult result)
        {
            if (!line.TryGetValue(TaxTotal, out var taxTotalObj) || taxTotalObj is not Dictionary<string, object> taxTotal)
            {
                result.AddError($"Invoice Line [{index}] must have a valid taxTotal object.");
                return;
            }
            if (!taxTotal.TryGetValue(TaxAmount, out var taxAmountObj) || !TryGetDecimal(taxAmountObj, out decimal taxLineAmount))
            {
                result.AddError($"Invoice Line [{index}] TaxTotal taxAmount must be a numeric value.");
                return;
            }
            if (taxLineAmount < 0)
            {
                result.AddError($"Invoice Line [{index}] TaxTotal taxAmount cannot be negative.");
            }

            if (!taxTotal.TryGetValue(RoundingAmount, out var roundingObj) || !TryGetDecimal(roundingObj, out decimal roundingAmount))
            {
                result.AddError($"Invoice Line [{index}] TaxTotal roundingAmount must be a numeric value.");
                return;
            }

            if (TryGetDecimal(line[LineExtensionAmount], out decimal lineExtAmount))
            {
                decimal expectedRounding = lineExtAmount + taxLineAmount;
                if (Math.Abs(expectedRounding - roundingAmount) > Tolerance)
                {
                    result.AddError(
                        $"Invoice Line [{index}] roundingAmount is incorrect. Expected {expectedRounding}, got {roundingAmount}.");
                }
            }
        }

        // Helper methods
        private static bool TryGetDecimal(object value, out decimal result)
        {
            result = 0;

            if (value == null)
            {
                return false;
            }

            if (value is decimal d)
            {
                result = d;
                return true;
            }

            if (value is int i)
            {
                result = i;
                return true;
            }

            if (value is long l)
            {
                result = l;
                return true;
            }

            if (value is float f)
            {
                result = (decimal)f;
                return true;
            }

            if (value is double dbl)
            {
                result = (decimal)dbl;
                return true;
            }

            if (value is string str)
            {
                return decimal.TryParse(str, out result);
            }

            return false;
        }

        private decimal GetDecimal(object value)
        {
            if (TryGetDecimal(value, out decimal result))
            {
                return result;
            }

            throw new ArgumentException($"Cannot convert value '{value}' to decimal.");
        }
    }
}
