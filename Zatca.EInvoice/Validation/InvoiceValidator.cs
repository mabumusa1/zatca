using System;
using System.Collections.Generic;

namespace Zatca.EInvoice.Validation
{
    /// <summary>
    /// Validates the required fields for invoice data according to the
    /// 20210819_ZATCA_E-invoice_Validation_Rules.
    ///
    /// This validator ensures that all necessary fields are present and non-empty,
    /// including top-level invoice fields, supplier and (if applicable) customer data,
    /// payment means, tax totals, legal monetary totals, invoice lines, and additional
    /// document references.
    /// </summary>
    public class InvoiceValidator : IInvoiceValidator
    {
        /// <summary>
        /// Validates the required invoice data fields.
        /// </summary>
        /// <param name="data">The invoice data dictionary.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether validation was successful.</returns>
        public ValidationResult Validate(Dictionary<string, object> data)
        {
            var result = new ValidationResult();

            try
            {
                ValidateTopLevelFields(data, result);
                ValidateInvoiceType(data, result);
                ValidateSupplier(data, result);
                ValidateCustomer(data, result);
                ValidatePaymentMeans(data, result);
                ValidateTaxTotal(data, result);
                ValidateLegalMonetaryTotal(data, result);
                ValidateInvoiceLines(data, result);
                ValidateAdditionalDocuments(data, result);
            }
            catch (Exception ex)
            {
                result.AddError($"Unexpected validation error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates the invoice data and throws an exception if validation fails.
        /// </summary>
        /// <param name="data">The invoice data dictionary.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        public void ValidateAndThrow(Dictionary<string, object> data)
        {
            // Validate top-level invoice fields.
            var requiredFields = new Dictionary<string, string>
            {
                { "uuid", "UUID" },
                { "id", "Invoice ID" },
                { "issueDate", "Issue Date" },
                { "currencyCode", "Invoice Currency Code" },
                { "taxCurrencyCode", "Tax Currency Code" }
            };

            foreach (var field in requiredFields)
            {
                if (!data.ContainsKey(field.Key) || IsEmpty(data[field.Key]))
                {
                    throw new ArgumentException($"The field '{field.Value}' is required and cannot be empty.");
                }
            }

            // Validate invoiceType fields if provided.
            if (data.ContainsKey("invoiceType") && data["invoiceType"] is Dictionary<string, object> invoiceType)
            {
                var invoiceTypeRequired = new Dictionary<string, string>
                {
                    { "invoice", "Invoice Type (invoice)" },
                    { "type", "Invoice Type (type)" }
                };

                foreach (var field in invoiceTypeRequired)
                {
                    if (!invoiceType.ContainsKey(field.Key) || IsEmpty(invoiceType[field.Key]))
                    {
                        throw new ArgumentException($"The field '{field.Value}' in invoiceType is required and cannot be empty.");
                    }
                }
            }

            // Validate supplier data.
            ValidateSupplierAndThrow(data);

            // Determine invoice type: if not simplified, then customer data is required.
            bool isSimplified = IsSimplifiedInvoice(data);
            if (!isSimplified)
            {
                ValidateCustomerAndThrow(data);
            }

            // Validate paymentMeans if provided.
            ValidatePaymentMeansAndThrow(data);

            // Validate taxTotal if provided.
            ValidateTaxTotalAndThrow(data);

            // Validate legalMonetaryTotal.
            ValidateLegalMonetaryTotalAndThrow(data);

            // Validate invoiceLines.
            ValidateInvoiceLinesAndThrow(data);

            // Validate additionalDocuments if provided.
            ValidateAdditionalDocumentsAndThrow(data);
        }

        private void ValidateTopLevelFields(Dictionary<string, object> data, ValidationResult result)
        {
            var requiredFields = new Dictionary<string, string>
            {
                { "uuid", "UUID" },
                { "id", "Invoice ID" },
                { "issueDate", "Issue Date" },
                { "currencyCode", "Invoice Currency Code" },
                { "taxCurrencyCode", "Tax Currency Code" }
            };

            foreach (var field in requiredFields)
            {
                if (!data.ContainsKey(field.Key) || IsEmpty(data[field.Key]))
                {
                    result.AddError($"The field '{field.Value}' is required and cannot be empty.");
                }
            }
        }

        private void ValidateInvoiceType(Dictionary<string, object> data, ValidationResult result)
        {
            if (!data.ContainsKey("invoiceType") || !(data["invoiceType"] is Dictionary<string, object> invoiceType))
            {
                return;
            }

            var invoiceTypeRequired = new Dictionary<string, string>
            {
                { "invoice", "Invoice Type (invoice)" },
                { "type", "Invoice Type (type)" }
            };

            foreach (var field in invoiceTypeRequired)
            {
                if (!invoiceType.ContainsKey(field.Key) || IsEmpty(invoiceType[field.Key]))
                {
                    result.AddError($"The field '{field.Value}' in invoiceType is required and cannot be empty.");
                }
            }
        }

        private void ValidateSupplier(Dictionary<string, object> data, ValidationResult result)
        {
            if (!data.ContainsKey("supplier") || IsEmpty(data["supplier"]))
            {
                result.AddError("Supplier data is required.");
                return;
            }

            if (!(data["supplier"] is Dictionary<string, object> supplier))
            {
                result.AddError("Supplier data must be a valid object.");
                return;
            }

            var supplierRequired = new[] { "registrationName", "taxId", "address" };
            foreach (var field in supplierRequired)
            {
                if (!supplier.ContainsKey(field) || IsEmpty(supplier[field]))
                {
                    result.AddError($"The field 'Supplier {field}' is required and cannot be empty.");
                }
            }

            // Validate supplier address fields.
            if (supplier.ContainsKey("address") && supplier["address"] is Dictionary<string, object> address)
            {
                var supplierAddressRequired = new[] { "street", "buildingNumber", "city", "postalZone", "country" };
                foreach (var field in supplierAddressRequired)
                {
                    if (!address.ContainsKey(field) || IsEmpty(address[field]))
                    {
                        result.AddError($"The field 'Supplier Address {field}' is required and cannot be empty.");
                    }
                }
            }
        }

        private void ValidateCustomer(Dictionary<string, object> data, ValidationResult result)
        {
            bool isSimplified = IsSimplifiedInvoice(data);
            if (isSimplified)
            {
                return;
            }

            if (!data.ContainsKey("customer") || IsEmpty(data["customer"]))
            {
                result.AddError("Customer data is required for non-simplified invoices.");
                return;
            }

            if (!(data["customer"] is Dictionary<string, object> customer))
            {
                result.AddError("Customer data must be a valid object.");
                return;
            }

            var customerRequired = new[] { "registrationName", "taxId", "address" };
            foreach (var field in customerRequired)
            {
                if (!customer.ContainsKey(field) || IsEmpty(customer[field]))
                {
                    result.AddError($"The field 'Customer {field}' is required and cannot be empty.");
                }
            }

            // Validate customer address fields.
            if (customer.ContainsKey("address") && customer["address"] is Dictionary<string, object> address)
            {
                var customerAddressRequired = new[] { "street", "buildingNumber", "city", "postalZone", "country" };
                foreach (var field in customerAddressRequired)
                {
                    if (!address.ContainsKey(field) || IsEmpty(address[field]))
                    {
                        result.AddError($"The field 'Customer Address {field}' is required and cannot be empty.");
                    }
                }
            }
        }

        private void ValidatePaymentMeans(Dictionary<string, object> data, ValidationResult result)
        {
            if (!data.ContainsKey("paymentMeans"))
            {
                return;
            }

            if (data["paymentMeans"] is Dictionary<string, object> paymentMeans)
            {
                if (!paymentMeans.ContainsKey("code") || IsEmpty(paymentMeans["code"]))
                {
                    result.AddError("The field 'Payment Means code' is required and cannot be empty.");
                }
            }
        }

        private void ValidateTaxTotal(Dictionary<string, object> data, ValidationResult result)
        {
            if (!data.ContainsKey("taxTotal"))
            {
                return;
            }

            if (!(data["taxTotal"] is Dictionary<string, object> taxTotal))
            {
                return;
            }

            if (!taxTotal.ContainsKey("taxAmount") || !IsNumericValue(taxTotal["taxAmount"]))
            {
                result.AddError("The field 'Tax Total taxAmount' is required and cannot be empty.");
            }

            // Validate subTotals if provided.
            if (taxTotal.ContainsKey("subTotals") && taxTotal["subTotals"] is IList<object> subTotals)
            {
                for (int i = 0; i < subTotals.Count; i++)
                {
                    if (!(subTotals[i] is Dictionary<string, object> subTotal))
                    {
                        continue;
                    }

                    var subRequired = new[] { "taxableAmount", "taxCategory" };
                    foreach (var field in subRequired)
                    {
                        if (!subTotal.ContainsKey(field) || IsEmpty(subTotal[field]))
                        {
                            result.AddError($"The field 'Tax Total subTotals[{i}] {field}' is required and cannot be empty.");
                        }
                    }

                    // Validate taxScheme id in subTotal.
                    if (subTotal.ContainsKey("taxCategory") &&
                        subTotal["taxCategory"] is Dictionary<string, object> taxCategory &&
                        taxCategory.ContainsKey("taxScheme") &&
                        taxCategory["taxScheme"] is Dictionary<string, object> taxScheme)
                    {
                        if (!taxScheme.ContainsKey("id") || IsEmpty(taxScheme["id"]))
                        {
                            result.AddError($"The field 'Tax Total subTotals[{i}] TaxScheme id' is required and cannot be empty.");
                        }
                    }
                }
            }
        }

        private void ValidateLegalMonetaryTotal(Dictionary<string, object> data, ValidationResult result)
        {
            if (!data.ContainsKey("legalMonetaryTotal") || IsEmpty(data["legalMonetaryTotal"]))
            {
                result.AddError("Legal Monetary Total data is required.");
                return;
            }

            if (!(data["legalMonetaryTotal"] is Dictionary<string, object> lmt))
            {
                result.AddError("Legal Monetary Total must be a valid object.");
                return;
            }

            var lmtRequired = new[] { "lineExtensionAmount", "taxExclusiveAmount", "taxInclusiveAmount", "payableAmount" };
            foreach (var field in lmtRequired)
            {
                if (!lmt.ContainsKey(field) || !IsNumericValue(lmt[field]))
                {
                    result.AddError($"The field 'Legal Monetary Total {field}' is required and cannot be empty.");
                }
            }
        }

        private void ValidateInvoiceLines(Dictionary<string, object> data, ValidationResult result)
        {
            if (!data.ContainsKey("invoiceLines") ||
                !(data["invoiceLines"] is IList<object> invoiceLines) ||
                invoiceLines.Count == 0)
            {
                result.AddError("At least one invoice line is required.");
                return;
            }

            for (int lineIndex = 0; lineIndex < invoiceLines.Count; lineIndex++)
            {
                if (!(invoiceLines[lineIndex] is Dictionary<string, object> line))
                {
                    continue;
                }

                var lineRequired = new[] { "id", "unitCode", "quantity", "lineExtensionAmount", "item", "price", "taxTotal" };
                foreach (var field in lineRequired)
                {
                    if (!line.ContainsKey(field) || (!IsNumericValue(line[field]) && IsEmpty(line[field])))
                    {
                        result.AddError($"The field 'Invoice Lines[{lineIndex}] {field}' is required and cannot be empty.");
                    }
                }

                // Validate item within invoice line.
                if (line.ContainsKey("item") && line["item"] is Dictionary<string, object> item)
                {
                    if (!item.ContainsKey("name") || IsEmpty(item["name"]))
                    {
                        result.AddError($"The field 'Invoice Lines[{lineIndex}] Item name' is required and cannot be empty.");
                    }

                    if (item.ContainsKey("classifiedTaxCategory") &&
                        item["classifiedTaxCategory"] is IList<object> classifiedTaxCategory &&
                        classifiedTaxCategory.Count > 0 &&
                        classifiedTaxCategory[0] is Dictionary<string, object> firstCategory)
                    {
                        if (!firstCategory.ContainsKey("taxScheme") ||
                            !(firstCategory["taxScheme"] is Dictionary<string, object> taxScheme) ||
                            !taxScheme.ContainsKey("id") || IsEmpty(taxScheme["id"]))
                        {
                            result.AddError($"The field 'Invoice Lines[{lineIndex}] Item TaxScheme id' is required and cannot be empty.");
                        }

                        if (!firstCategory.ContainsKey("percent") || !IsNumericValue(firstCategory["percent"]))
                        {
                            result.AddError($"The field 'Invoice Lines[{lineIndex}] Item percent' is required and cannot be empty.");
                        }
                    }
                }

                // Validate price within invoice line.
                if (line.ContainsKey("price") && line["price"] is Dictionary<string, object> price)
                {
                    if (!price.ContainsKey("amount") || !IsNumericValue(price["amount"]))
                    {
                        result.AddError($"The field 'Invoice Lines[{lineIndex}] Price amount' is required and cannot be empty.");
                    }
                }

                // Validate taxTotal within invoice line.
                if (line.ContainsKey("taxTotal") && line["taxTotal"] is Dictionary<string, object> taxTotal)
                {
                    if (!taxTotal.ContainsKey("taxAmount") || !IsNumericValue(taxTotal["taxAmount"]))
                    {
                        result.AddError($"The field 'Invoice Lines[{lineIndex}] TaxTotal taxAmount' is required and cannot be empty.");
                    }
                }
            }
        }

        private void ValidateAdditionalDocuments(Dictionary<string, object> data, ValidationResult result)
        {
            if (!data.ContainsKey("additionalDocuments") ||
                !(data["additionalDocuments"] is IList<object> additionalDocuments))
            {
                return;
            }

            for (int docIndex = 0; docIndex < additionalDocuments.Count; docIndex++)
            {
                if (!(additionalDocuments[docIndex] is Dictionary<string, object> doc))
                {
                    continue;
                }

                if (!doc.ContainsKey("id") || IsEmpty(doc["id"]))
                {
                    result.AddError($"The field 'AdditionalDocuments[{docIndex}] id' is required and cannot be empty.");
                }

                // For documents with id 'PIH', attachment is required.
                if (doc.ContainsKey("id") && doc["id"]?.ToString() == "PIH")
                {
                    if (!doc.ContainsKey("attachment") || IsEmpty(doc["attachment"]))
                    {
                        result.AddError($"The attachment for AdditionalDocuments[{docIndex}] with id 'PIH' is required.");
                    }
                }
            }
        }

        // Throw-based validation methods
        private void ValidateSupplierAndThrow(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("supplier") || IsEmpty(data["supplier"]))
            {
                throw new ArgumentException("Supplier data is required.");
            }

            if (!(data["supplier"] is Dictionary<string, object> supplier))
            {
                throw new ArgumentException("Supplier data must be a valid object.");
            }

            var supplierRequired = new[] { "registrationName", "taxId", "address" };
            foreach (var field in supplierRequired)
            {
                if (!supplier.ContainsKey(field) || IsEmpty(supplier[field]))
                {
                    throw new ArgumentException($"The field 'Supplier {field}' is required and cannot be empty.");
                }
            }

            // Validate supplier address fields.
            if (!(supplier["address"] is Dictionary<string, object> address))
            {
                throw new ArgumentException("Supplier address must be a valid object.");
            }

            var supplierAddressRequired = new[] { "street", "buildingNumber", "city", "postalZone", "country" };
            foreach (var field in supplierAddressRequired)
            {
                if (!address.ContainsKey(field) || IsEmpty(address[field]))
                {
                    throw new ArgumentException($"The field 'Supplier Address {field}' is required and cannot be empty.");
                }
            }
        }

        private void ValidateCustomerAndThrow(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("customer") || IsEmpty(data["customer"]))
            {
                throw new ArgumentException("Customer data is required for non-simplified invoices.");
            }

            if (!(data["customer"] is Dictionary<string, object> customer))
            {
                throw new ArgumentException("Customer data must be a valid object.");
            }

            var customerRequired = new[] { "registrationName", "taxId", "address" };
            foreach (var field in customerRequired)
            {
                if (!customer.ContainsKey(field) || IsEmpty(customer[field]))
                {
                    throw new ArgumentException($"The field 'Customer {field}' is required and cannot be empty.");
                }
            }

            // Validate customer address fields.
            if (!(customer["address"] is Dictionary<string, object> address))
            {
                throw new ArgumentException("Customer address must be a valid object.");
            }

            var customerAddressRequired = new[] { "street", "buildingNumber", "city", "postalZone", "country" };
            foreach (var field in customerAddressRequired)
            {
                if (!address.ContainsKey(field) || IsEmpty(address[field]))
                {
                    throw new ArgumentException($"The field 'Customer Address {field}' is required and cannot be empty.");
                }
            }
        }

        private void ValidatePaymentMeansAndThrow(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("paymentMeans"))
            {
                return;
            }

            if (data["paymentMeans"] is Dictionary<string, object> paymentMeans)
            {
                if (!paymentMeans.ContainsKey("code") || IsEmpty(paymentMeans["code"]))
                {
                    throw new ArgumentException("The field 'Payment Means code' is required and cannot be empty.");
                }
            }
        }

        private void ValidateTaxTotalAndThrow(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("taxTotal"))
            {
                return;
            }

            if (!(data["taxTotal"] is Dictionary<string, object> taxTotal))
            {
                return;
            }

            if (!taxTotal.ContainsKey("taxAmount") || !IsNumericValue(taxTotal["taxAmount"]))
            {
                throw new ArgumentException("The field 'Tax Total taxAmount' is required and cannot be empty.");
            }

            // Validate subTotals if provided.
            if (taxTotal.ContainsKey("subTotals") && taxTotal["subTotals"] is IList<object> subTotals)
            {
                for (int i = 0; i < subTotals.Count; i++)
                {
                    if (!(subTotals[i] is Dictionary<string, object> subTotal))
                    {
                        continue;
                    }

                    var subRequired = new[] { "taxableAmount", "taxCategory" };
                    foreach (var field in subRequired)
                    {
                        if (!subTotal.ContainsKey(field) || IsEmpty(subTotal[field]))
                        {
                            throw new ArgumentException($"The field 'Tax Total subTotals[{i}] {field}' is required and cannot be empty.");
                        }
                    }

                    // Validate taxScheme id in subTotal.
                    if (!(subTotal["taxCategory"] is Dictionary<string, object> taxCategory) ||
                        !taxCategory.ContainsKey("taxScheme") ||
                        !(taxCategory["taxScheme"] is Dictionary<string, object> taxScheme) ||
                        !taxScheme.ContainsKey("id") || IsEmpty(taxScheme["id"]))
                    {
                        throw new ArgumentException($"The field 'Tax Total subTotals[{i}] TaxScheme id' is required and cannot be empty.");
                    }
                }
            }
        }

        private void ValidateLegalMonetaryTotalAndThrow(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("legalMonetaryTotal") || IsEmpty(data["legalMonetaryTotal"]))
            {
                throw new ArgumentException("Legal Monetary Total data is required.");
            }

            if (!(data["legalMonetaryTotal"] is Dictionary<string, object> lmt))
            {
                throw new ArgumentException("Legal Monetary Total must be a valid object.");
            }

            var lmtRequired = new[] { "lineExtensionAmount", "taxExclusiveAmount", "taxInclusiveAmount", "payableAmount" };
            foreach (var field in lmtRequired)
            {
                if (!lmt.ContainsKey(field) || !IsNumericValue(lmt[field]))
                {
                    throw new ArgumentException($"The field 'Legal Monetary Total {field}' is required and cannot be empty.");
                }
            }
        }

        private void ValidateInvoiceLinesAndThrow(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("invoiceLines") ||
                !(data["invoiceLines"] is IList<object> invoiceLines) ||
                invoiceLines.Count == 0)
            {
                throw new ArgumentException("At least one invoice line is required.");
            }

            for (int lineIndex = 0; lineIndex < invoiceLines.Count; lineIndex++)
            {
                if (!(invoiceLines[lineIndex] is Dictionary<string, object> line))
                {
                    continue;
                }

                var lineRequired = new[] { "id", "unitCode", "quantity", "lineExtensionAmount", "item", "price", "taxTotal" };
                foreach (var field in lineRequired)
                {
                    if (!line.ContainsKey(field) || (!IsNumericValue(line[field]) && IsEmpty(line[field])))
                    {
                        throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] {field}' is required and cannot be empty.");
                    }
                }

                // Validate item within invoice line.
                if (!(line["item"] is Dictionary<string, object> item))
                {
                    throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] item' must be a valid object.");
                }

                if (!item.ContainsKey("name") || IsEmpty(item["name"]))
                {
                    throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] Item name' is required and cannot be empty.");
                }

                if (!(item.ContainsKey("classifiedTaxCategory") &&
                      item["classifiedTaxCategory"] is IList<object> classifiedTaxCategory &&
                      classifiedTaxCategory.Count > 0 &&
                      classifiedTaxCategory[0] is Dictionary<string, object> firstCategory))
                {
                    throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] Item classifiedTaxCategory' is required.");
                }
                else
                {
                    var categoryItem = firstCategory;
                    if (!(categoryItem.ContainsKey("taxScheme") &&
                          categoryItem["taxScheme"] is Dictionary<string, object> taxScheme &&
                          taxScheme.ContainsKey("id") && !IsEmpty(taxScheme["id"])))
                    {
                        throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] Item TaxScheme id' is required and cannot be empty.");
                    }

                    if (!categoryItem.ContainsKey("percent") || !IsNumericValue(categoryItem["percent"]))
                    {
                        throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] Item percent' is required and cannot be empty.");
                    }
                }

                // Validate price within invoice line.
                if (!(line["price"] is Dictionary<string, object> price))
                {
                    throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] price' must be a valid object.");
                }

                if (!price.ContainsKey("amount") || !IsNumericValue(price["amount"]))
                {
                    throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] Price amount' is required and cannot be empty.");
                }

                // Validate taxTotal within invoice line.
                if (!(line["taxTotal"] is Dictionary<string, object> taxTotal))
                {
                    throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] taxTotal' must be a valid object.");
                }

                if (!taxTotal.ContainsKey("taxAmount") || !IsNumericValue(taxTotal["taxAmount"]))
                {
                    throw new ArgumentException($"The field 'Invoice Lines[{lineIndex}] TaxTotal taxAmount' is required and cannot be empty.");
                }
            }
        }

        private void ValidateAdditionalDocumentsAndThrow(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("additionalDocuments") ||
                !(data["additionalDocuments"] is IList<object> additionalDocuments))
            {
                return;
            }

            for (int docIndex = 0; docIndex < additionalDocuments.Count; docIndex++)
            {
                if (!(additionalDocuments[docIndex] is Dictionary<string, object> doc))
                {
                    continue;
                }

                if (!doc.ContainsKey("id") || IsEmpty(doc["id"]))
                {
                    throw new ArgumentException($"The field 'AdditionalDocuments[{docIndex}] id' is required and cannot be empty.");
                }

                // For documents with id 'PIH', attachment is required.
                if (doc.ContainsKey("id") && doc["id"]?.ToString() == "PIH")
                {
                    if (!doc.ContainsKey("attachment") || IsEmpty(doc["attachment"]))
                    {
                        throw new ArgumentException($"The attachment for AdditionalDocuments[{docIndex}] with id 'PIH' is required.");
                    }
                }
            }
        }

        // Helper methods
        private bool IsSimplifiedInvoice(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("invoiceType") || !(data["invoiceType"] is Dictionary<string, object> invoiceType))
            {
                return false;
            }

            if (!invoiceType.ContainsKey("invoice"))
            {
                return false;
            }

            return invoiceType["invoice"]?.ToString()?.ToLower() == "simplified";
        }

        private bool IsEmpty(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str);
            }

            return false;
        }

        private bool IsNumericValue(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is string str)
            {
                return !string.IsNullOrWhiteSpace(str) && decimal.TryParse(str, out _);
            }

            return value is int || value is long || value is float || value is double || value is decimal;
        }
    }
}
