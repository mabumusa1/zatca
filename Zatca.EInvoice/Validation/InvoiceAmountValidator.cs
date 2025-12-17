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
            if (!data.ContainsKey("legalMonetaryTotal"))
            {
                throw new ArgumentException("Legal Monetary Total section is missing.");
            }

            if (!(data["legalMonetaryTotal"] is Dictionary<string, object> lmt))
            {
                throw new ArgumentException("Legal Monetary Total must be a valid object.");
            }

            var requiredFields = new[] { "lineExtensionAmount", "taxExclusiveAmount", "taxInclusiveAmount", "payableAmount" };

            // Ensure that required fields exist, are numeric, and non-negative.
            foreach (var field in requiredFields)
            {
                if (!lmt.ContainsKey(field) || !TryGetDecimal(lmt[field], out decimal value))
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
            if (data.ContainsKey("taxTotal") &&
                data["taxTotal"] is Dictionary<string, object> taxTotal &&
                taxTotal.ContainsKey("taxAmount") &&
                TryGetDecimal(taxTotal["taxAmount"], out decimal taxAmount))
            {
                taxTotalAmount = taxAmount;
            }

            decimal taxExclusiveAmount = GetDecimal(lmt["taxExclusiveAmount"]);
            decimal expectedTaxInclusive = taxExclusiveAmount + taxTotalAmount;
            decimal actualTaxInclusive = GetDecimal(lmt["taxInclusiveAmount"]);

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
                if (!(invoiceLines[index] is Dictionary<string, object> line))
                {
                    continue;
                }

                // Validate that 'quantity' and 'lineExtensionAmount' are provided, numeric, and non-negative.
                var numericFields = new[] { "quantity", "lineExtensionAmount" };
                foreach (var field in numericFields)
                {
                    if (!line.ContainsKey(field) || !TryGetDecimal(line[field], out decimal value))
                    {
                        throw new ArgumentException($"Invoice Line [{index}] field '{field}' must be a numeric value.");
                    }

                    if (value < 0)
                    {
                        throw new ArgumentException($"Invoice Line [{index}] field '{field}' cannot be negative.");
                    }
                }

                // Validate that the price amount exists, is numeric, and non-negative.
                if (!line.ContainsKey("price") || !(line["price"] is Dictionary<string, object> price))
                {
                    throw new ArgumentException($"Invoice Line [{index}] must have a valid price object.");
                }

                if (!price.ContainsKey("amount") || !TryGetDecimal(price["amount"], out decimal priceAmount))
                {
                    throw new ArgumentException($"Invoice Line [{index}] Price amount must be a numeric value.");
                }

                if (priceAmount < 0)
                {
                    throw new ArgumentException($"Invoice Line [{index}] Price amount cannot be negative.");
                }

                // Calculate expected lineExtensionAmount = price amount * quantity.
                decimal quantity = GetDecimal(line["quantity"]);
                decimal expectedLineExtension = priceAmount * quantity;
                decimal providedLineExtension = GetDecimal(line["lineExtensionAmount"]);

                if (Math.Abs(expectedLineExtension - providedLineExtension) > Tolerance)
                {
                    throw new ArgumentException(
                        $"Invoice Line [{index}] lineExtensionAmount is incorrect. Expected {expectedLineExtension}, got {providedLineExtension}."
                    );
                }

                // Validate item taxPercent if provided.
                if (line.ContainsKey("item") &&
                    line["item"] is Dictionary<string, object> item &&
                    item.ContainsKey("taxPercent"))
                {
                    if (!TryGetDecimal(item["taxPercent"], out decimal taxPercent))
                    {
                        throw new ArgumentException($"Invoice Line [{index}] item taxPercent must be a numeric value.");
                    }

                    if (taxPercent < 0 || taxPercent > 100)
                    {
                        throw new ArgumentException($"Invoice Line [{index}] item taxPercent must be between 0 and 100.");
                    }
                }

                // Validate that taxTotal's taxAmount exists, is numeric, and non-negative.
                if (!line.ContainsKey("taxTotal") || !(line["taxTotal"] is Dictionary<string, object> taxTotal))
                {
                    throw new ArgumentException($"Invoice Line [{index}] must have a valid taxTotal object.");
                }

                if (!taxTotal.ContainsKey("taxAmount") || !TryGetDecimal(taxTotal["taxAmount"], out decimal taxLineAmount))
                {
                    throw new ArgumentException($"Invoice Line [{index}] TaxTotal taxAmount must be a numeric value.");
                }

                if (taxLineAmount < 0)
                {
                    throw new ArgumentException($"Invoice Line [{index}] TaxTotal taxAmount cannot be negative.");
                }

                // Validate that taxTotal's roundingAmount exists, is numeric, and equals lineExtensionAmount + taxAmount.
                if (!taxTotal.ContainsKey("roundingAmount") || !TryGetDecimal(taxTotal["roundingAmount"], out decimal roundingAmount))
                {
                    throw new ArgumentException($"Invoice Line [{index}] TaxTotal roundingAmount must be a numeric value.");
                }

                decimal expectedRounding = providedLineExtension + taxLineAmount;
                if (Math.Abs(expectedRounding - roundingAmount) > Tolerance)
                {
                    throw new ArgumentException(
                        $"Invoice Line [{index}] roundingAmount is incorrect. Expected {expectedRounding}, got {roundingAmount}."
                    );
                }
            }
        }

        private void ValidateMonetaryTotalsInternal(Dictionary<string, object> data, ValidationResult result)
        {
            // Check that the Legal Monetary Total section exists.
            if (!data.ContainsKey("legalMonetaryTotal"))
            {
                result.AddError("Legal Monetary Total section is missing.");
                return;
            }

            if (!(data["legalMonetaryTotal"] is Dictionary<string, object> lmt))
            {
                result.AddError("Legal Monetary Total must be a valid object.");
                return;
            }

            var requiredFields = new[] { "lineExtensionAmount", "taxExclusiveAmount", "taxInclusiveAmount", "payableAmount" };

            // Ensure that required fields exist, are numeric, and non-negative.
            foreach (var field in requiredFields)
            {
                if (!lmt.ContainsKey(field) || !TryGetDecimal(lmt[field], out decimal value))
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
            if (data.ContainsKey("taxTotal") &&
                data["taxTotal"] is Dictionary<string, object> taxTotal &&
                taxTotal.ContainsKey("taxAmount") &&
                TryGetDecimal(taxTotal["taxAmount"], out decimal taxAmount))
            {
                taxTotalAmount = taxAmount;
            }

            decimal taxExclusiveAmount = GetDecimal(lmt["taxExclusiveAmount"]);
            decimal expectedTaxInclusive = taxExclusiveAmount + taxTotalAmount;
            decimal actualTaxInclusive = GetDecimal(lmt["taxInclusiveAmount"]);

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
                if (!(invoiceLines[index] is Dictionary<string, object> line))
                {
                    continue;
                }

                // Validate that 'quantity' and 'lineExtensionAmount' are provided, numeric, and non-negative.
                var numericFields = new[] { "quantity", "lineExtensionAmount" };
                foreach (var field in numericFields)
                {
                    if (!line.ContainsKey(field) || !TryGetDecimal(line[field], out decimal value))
                    {
                        result.AddError($"Invoice Line [{index}] field '{field}' must be a numeric value.");
                        continue;
                    }

                    if (value < 0)
                    {
                        result.AddError($"Invoice Line [{index}] field '{field}' cannot be negative.");
                    }
                }

                // Validate that the price amount exists, is numeric, and non-negative.
                if (!line.ContainsKey("price") || !(line["price"] is Dictionary<string, object> price))
                {
                    result.AddError($"Invoice Line [{index}] must have a valid price object.");
                    continue;
                }

                if (!price.ContainsKey("amount") || !TryGetDecimal(price["amount"], out decimal priceAmount))
                {
                    result.AddError($"Invoice Line [{index}] Price amount must be a numeric value.");
                    continue;
                }

                if (priceAmount < 0)
                {
                    result.AddError($"Invoice Line [{index}] Price amount cannot be negative.");
                }

                // Calculate expected lineExtensionAmount = price amount * quantity.
                if (TryGetDecimal(line["quantity"], out decimal quantity) &&
                    TryGetDecimal(line["lineExtensionAmount"], out decimal providedLineExtension))
                {
                    decimal expectedLineExtension = priceAmount * quantity;

                    if (Math.Abs(expectedLineExtension - providedLineExtension) > Tolerance)
                    {
                        result.AddError(
                            $"Invoice Line [{index}] lineExtensionAmount is incorrect. Expected {expectedLineExtension}, got {providedLineExtension}."
                        );
                    }
                }

                // Validate item taxPercent if provided.
                if (line.ContainsKey("item") &&
                    line["item"] is Dictionary<string, object> item &&
                    item.ContainsKey("taxPercent"))
                {
                    if (!TryGetDecimal(item["taxPercent"], out decimal taxPercent))
                    {
                        result.AddError($"Invoice Line [{index}] item taxPercent must be a numeric value.");
                    }
                    else if (taxPercent < 0 || taxPercent > 100)
                    {
                        result.AddError($"Invoice Line [{index}] item taxPercent must be between 0 and 100.");
                    }
                }

                // Validate that taxTotal's taxAmount exists, is numeric, and non-negative.
                if (!line.ContainsKey("taxTotal") || !(line["taxTotal"] is Dictionary<string, object> taxTotal))
                {
                    result.AddError($"Invoice Line [{index}] must have a valid taxTotal object.");
                    continue;
                }

                if (!taxTotal.ContainsKey("taxAmount") || !TryGetDecimal(taxTotal["taxAmount"], out decimal taxLineAmount))
                {
                    result.AddError($"Invoice Line [{index}] TaxTotal taxAmount must be a numeric value.");
                    continue;
                }

                if (taxLineAmount < 0)
                {
                    result.AddError($"Invoice Line [{index}] TaxTotal taxAmount cannot be negative.");
                }

                // Validate that taxTotal's roundingAmount exists, is numeric, and equals lineExtensionAmount + taxAmount.
                if (!taxTotal.ContainsKey("roundingAmount") || !TryGetDecimal(taxTotal["roundingAmount"], out decimal roundingAmount))
                {
                    result.AddError($"Invoice Line [{index}] TaxTotal roundingAmount must be a numeric value.");
                    continue;
                }

                if (TryGetDecimal(line["lineExtensionAmount"], out decimal lineExtAmount))
                {
                    decimal expectedRounding = lineExtAmount + taxLineAmount;
                    if (Math.Abs(expectedRounding - roundingAmount) > Tolerance)
                    {
                        result.AddError(
                            $"Invoice Line [{index}] roundingAmount is incorrect. Expected {expectedRounding}, got {roundingAmount}."
                        );
                    }
                }
            }
        }

        // Helper methods
        private bool TryGetDecimal(object value, out decimal result)
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
