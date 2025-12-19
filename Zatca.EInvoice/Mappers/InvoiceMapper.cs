using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Models.Signature;
using Zatca.EInvoice.Models.References;
using Zatca.EInvoice.Models.Enums;
using Zatca.EInvoice.Helpers;
using Zatca.EInvoice.Validation;

namespace Zatca.EInvoice.Mappers
{
    /// <summary>
    /// Maps complete invoice data (provided as a JSON string or dictionary)
    /// into an Invoice object according to ZATCA specifications.
    ///
    /// The mapping process uses several dependent mappers to convert nested data sections,
    /// such as supplier, customer, invoice lines, payment means, and additional documents.
    /// </summary>
    public class InvoiceMapper : IInvoiceMapper
    {
        private readonly SupplierMapper _supplierMapper;
        private readonly CustomerMapper _customerMapper;
        private readonly InvoiceLineMapper _invoiceLineMapper;
        private readonly PaymentMeansMapper _paymentMeansMapper;
        private readonly AdditionalDocumentMapper _additionalDocumentMapper;

        /// <summary>
        /// InvoiceMapper constructor.
        /// Initializes all dependent mappers.
        /// </summary>
        public InvoiceMapper()
        {
            _supplierMapper = new SupplierMapper();
            _customerMapper = new CustomerMapper();
            _invoiceLineMapper = new InvoiceLineMapper();
            _paymentMeansMapper = new PaymentMeansMapper();
            _additionalDocumentMapper = new AdditionalDocumentMapper();
        }

        /// <summary>
        /// InvoiceMapper constructor with dependency injection.
        /// </summary>
        public InvoiceMapper(
            SupplierMapper supplierMapper,
            CustomerMapper customerMapper,
            InvoiceLineMapper invoiceLineMapper,
            PaymentMeansMapper paymentMeansMapper,
            AdditionalDocumentMapper additionalDocumentMapper)
        {
            _supplierMapper = supplierMapper;
            _customerMapper = customerMapper;
            _invoiceLineMapper = invoiceLineMapper;
            _paymentMeansMapper = paymentMeansMapper;
            _additionalDocumentMapper = additionalDocumentMapper;
        }

        /// <summary>
        /// Maps input data to an Invoice object.
        /// </summary>
        /// <param name="data">An dictionary of invoice data.</param>
        /// <returns>The mapped Invoice object.</returns>
        public Invoice MapToInvoice(Dictionary<string, object> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Invoice data cannot be null");
            }

            // Optionally, validate the required invoice fields
            var validator = new InvoiceValidator();
            validator.Validate(data);

            var invoice = new Invoice
            {
                UblExtensions = MapUblExtensions(DictionaryHelper.GetDictionary(data, "ublExtensions")),
                UUID = DictionaryHelper.GetString(data, "uuid") ?? string.Empty,
                Id = DictionaryHelper.GetString(data, "id") ?? string.Empty,
                IssueDate = MapDateOnly(DictionaryHelper.GetString(data, "issueDate") ?? string.Empty),
                IssueTime = MapTimeOnly(DictionaryHelper.GetString(data, "issueTime") ?? string.Empty),
                InvoiceType = MapInvoiceType(DictionaryHelper.GetDictionary(data, "invoiceType")),
                Note = DictionaryHelper.GetString(data, "note"),
                LanguageID = DictionaryHelper.GetString(data, "languageID") ?? "en",
                InvoiceCurrencyCode = DictionaryHelper.GetString(data, "currencyCode") ?? "SAR",
                TaxCurrencyCode = DictionaryHelper.GetString(data, "taxCurrencyCode") ?? "SAR",
                BillingReferences = MapBillingReferences(DictionaryHelper.GetList(data, "billingReferences")),
                AdditionalDocumentReferences = _additionalDocumentMapper.MapAdditionalDocuments(
                    DictionaryHelper.GetList(data, "additionalDocuments")),
                AccountingSupplierParty = _supplierMapper.Map(DictionaryHelper.GetDictionary(data, "supplier")),
                AccountingCustomerParty = _customerMapper.Map(DictionaryHelper.GetDictionary(data, "customer")),
                Delivery = MapDelivery(DictionaryHelper.GetDictionary(data, "delivery")),
                PaymentMeans = _paymentMeansMapper.Map(DictionaryHelper.GetDictionary(data, "paymentMeans")),
                AllowanceCharges = MapAllowanceCharges(data),
                TaxTotal = MapTaxTotal(DictionaryHelper.GetDictionary(data, "taxTotal")),
                LegalMonetaryTotal = MapLegalMonetaryTotal(DictionaryHelper.GetDictionary(data, "legalMonetaryTotal")),
                InvoiceLines = _invoiceLineMapper.MapInvoiceLines(DictionaryHelper.GetList(data, "invoiceLines")),
                Signature = MapSignature(DictionaryHelper.GetDictionary(data, "signature"))
            };

            // Notes are handled via the Note property assignment above

            return invoice;
        }

        /// <summary>
        /// Maps input data to an Invoice object from JSON string.
        /// </summary>
        /// <param name="jsonData">Invoice data as a JSON string.</param>
        /// <returns>The mapped Invoice object.</returns>
        public Invoice MapToInvoice(string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                throw new ArgumentException("JSON data cannot be null or empty", nameof(jsonData));
            }

            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new Dictionary<string, object>();

                return MapToInvoice(data);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Invalid JSON data provided", nameof(jsonData), ex);
            }
        }

        /// <summary>
        /// Maps input data to an Invoice object from JsonElement.
        /// </summary>
        /// <param name="jsonElement">Invoice data as a JsonElement.</param>
        /// <returns>The mapped Invoice object.</returns>
        public Invoice MapToInvoice(JsonElement jsonElement)
        {
            var jsonString = jsonElement.GetRawText();
            return MapToInvoice(jsonString);
        }

        /// <summary>
        /// Maps UblExtensions data to a UblExtensions object.
        /// </summary>
        private UblExtensions MapUblExtensions(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            var signatureInfo = new SignatureInformation
            {
                ReferencedSignatureID = DictionaryHelper.GetString(data, "referencedSignatureId",
                    "urn:oasis:names:specification:ubl:signature:Invoice"),
                Id = DictionaryHelper.GetString(data, "id", "urn:oasis:names:specification:ubl:signature:1")
            };

            var ublDocSignatures = new UblDocumentSignatures
            {
                SignatureInformation = signatureInfo
            };

            var extensionContent = new ExtensionContent
            {
                UblDocumentSignatures = ublDocSignatures
            };

            var ublExtension = new UblExtension
            {
                ExtensionURI = DictionaryHelper.GetString(data, "extensionUri",
                    "urn:oasis:names:specification:ubl:dsig:enveloped:xades"),
                ExtensionContent = extensionContent
            };

            return new UblExtensions
            {
                Extensions = new List<UblExtension> { ublExtension }
            };
        }

        /// <summary>
        /// Maps Signature data to a Signature object.
        /// </summary>
        private Signature MapSignature(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            return new Signature
            {
                Id = DictionaryHelper.GetString(data, "id") ?? "urn:oasis:names:specification:ubl:signature:Invoice",
                SignatureMethod = DictionaryHelper.GetString(data, "method")
                    ?? "urn:oasis:names:specification:ubl:dsig:enveloped:xades"
            };
        }

        /// <summary>
        /// Maps InvoiceType data to an InvoiceType object.
        /// </summary>
        private InvoiceType MapInvoiceType(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            return new InvoiceType
            {
                Invoice = DictionaryHelper.GetString(data, "invoice", "simplified"),
                InvoiceSubType = DictionaryHelper.GetString(data, "type", "invoice"),
                IsThirdParty = DictionaryHelper.GetBoolean(data, "isThirdParty", false),
                IsNominal = DictionaryHelper.GetBoolean(data, "isNominal", false),
                IsExportInvoice = DictionaryHelper.GetBoolean(data, "isExport", false),
                IsSummary = DictionaryHelper.GetBoolean(data, "isSummary", false),
                IsSelfBilled = DictionaryHelper.GetBoolean(data, "isSelfBilled", false)
            };
        }

        /// <summary>
        /// Maps BillingReferences data to an array of BillingReference objects.
        /// </summary>
        private static List<BillingReference> MapBillingReferences(IEnumerable<object>? data)
        {
            var billingReferences = new List<BillingReference>();

            if (data == null)
            {
                return billingReferences;
            }

            foreach (var billingObj in data)
            {
                if (billingObj is Dictionary<string, object> billingData)
                {
                    billingReferences.Add(new BillingReference
                    {
                        Id = DictionaryHelper.GetString(billingData, "id", string.Empty)
                    });
                }
            }

            return billingReferences;
        }

        /// <summary>
        /// Maps AllowanceCharge data to an array of AllowanceCharge objects.
        /// </summary>
        private static List<AllowanceCharge> MapAllowanceCharges(Dictionary<string, object> data)
        {
            var allowanceChargesList = DictionaryHelper.GetList(data, "allowanceCharges");
            if (allowanceChargesList == null)
            {
                return new List<AllowanceCharge>();
            }

            var allowanceCharges = new List<AllowanceCharge>();
            foreach (var allowanceChargeObj in allowanceChargesList)
            {
                if (allowanceChargeObj is Dictionary<string, object> allowanceCharge)
                {
                    allowanceCharges.Add(MapSingleAllowanceCharge(allowanceCharge));
                }
            }

            return allowanceCharges;
        }

        private static AllowanceCharge MapSingleAllowanceCharge(Dictionary<string, object> allowanceCharge)
        {
            return new AllowanceCharge
            {
                ChargeIndicator = DictionaryHelper.GetBoolean(allowanceCharge, "isCharge", false),
                AllowanceChargeReason = DictionaryHelper.GetString(allowanceCharge, "reason", "discount"),
                Amount = DictionaryHelper.GetDecimal(allowanceCharge, "amount", 0m),
                TaxCategories = MapTaxCategories(allowanceCharge)
            };
        }

        private static List<TaxCategory> MapTaxCategories(Dictionary<string, object> allowanceCharge)
        {
            var taxCategories = new List<TaxCategory>();

            if (!allowanceCharge.TryGetValue("taxCategories", out var taxCatValue) ||
                taxCatValue is not IEnumerable<object> taxCatList)
            {
                return taxCategories;
            }

            foreach (var taxCatObj in taxCatList)
            {
                if (taxCatObj is Dictionary<string, object> taxCatData)
                {
                    var taxSchemeData = DictionaryHelper.GetDictionary(taxCatData, "taxScheme");
                    taxCategories.Add(new TaxCategory
                    {
                        Percent = DictionaryHelper.GetDecimal(taxCatData, "percent", 15m),
                        TaxScheme = new TaxScheme { Id = DictionaryHelper.GetString(taxSchemeData, "id", "VAT") }
                    });
                }
            }

            return taxCategories;
        }

        /// <summary>
        /// Maps Delivery data to a Delivery object.
        /// </summary>
        private Delivery MapDelivery(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            return new Delivery
            {
                ActualDeliveryDate = MapNullableDateOnly(DictionaryHelper.GetString(data, "actualDeliveryDate", null)),
                LatestDeliveryDate = MapNullableDateOnly(DictionaryHelper.GetString(data, "latestDeliveryDate", null))
            };
        }

        /// <summary>
        /// Maps TaxTotal data to a TaxTotal object.
        /// </summary>
        private TaxTotal MapTaxTotal(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            var taxTotal = new TaxTotal
            {
                TaxAmount = DictionaryHelper.GetDecimal(data, "taxAmount", 0m),
                TaxSubTotals = new List<TaxSubTotal>()
            };

            if (data.TryGetValue("subTotals", out var subTotalsValue) && subTotalsValue is IEnumerable<object> subTotalsList)
            {
                foreach (var subTotalObj in subTotalsList)
                {
                    if (subTotalObj is Dictionary<string, object> subTotal)
                    {
                        // Extract tax category data from subTotal
                        var taxCategoryData = DictionaryHelper.GetDictionary(subTotal, "taxCategory");
                        var percent = DictionaryHelper.GetDecimal(taxCategoryData, "percent", 15m);
                        var reasonCode = DictionaryHelper.GetString(taxCategoryData, "reasonCode", null);
                        var reason = DictionaryHelper.GetString(taxCategoryData, "reason", null);

                        // Build the TaxScheme object
                        var taxSchemeData = DictionaryHelper.GetDictionary(taxCategoryData, "taxScheme");
                        var taxScheme = new TaxScheme
                        {
                            Id = DictionaryHelper.GetString(taxSchemeData, "id", "VAT")
                        };

                        // Build the TaxCategory object
                        var taxCategory = new TaxCategory
                        {
                            Percent = percent,
                            TaxExemptionReasonCode = reasonCode,
                            TaxExemptionReason = reason,
                            TaxScheme = taxScheme
                        };

                        // Create the TaxSubTotal object
                        var taxSubTotal = new TaxSubTotal
                        {
                            TaxableAmount = DictionaryHelper.GetDecimal(subTotal, "taxableAmount", 0m),
                            TaxAmount = DictionaryHelper.GetDecimal(subTotal, "taxAmount", 0m),
                            TaxCategory = taxCategory
                        };

                        taxTotal.TaxSubTotals.Add(taxSubTotal);
                    }
                }
            }

            return taxTotal;
        }

        /// <summary>
        /// Maps LegalMonetaryTotal data to a LegalMonetaryTotal object.
        /// </summary>
        private LegalMonetaryTotal MapLegalMonetaryTotal(Dictionary<string, object> data)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            return new LegalMonetaryTotal
            {
                LineExtensionAmount = DictionaryHelper.GetDecimal(data, "lineExtensionAmount", 0m),
                TaxExclusiveAmount = DictionaryHelper.GetDecimal(data, "taxExclusiveAmount", 0m),
                TaxInclusiveAmount = DictionaryHelper.GetDecimal(data, "taxInclusiveAmount", 0m),
                PrepaidAmount = DictionaryHelper.GetDecimal(data, "prepaidAmount", 0m),
                PayableAmount = DictionaryHelper.GetDecimal(data, "payableAmount", 0m),
                AllowanceTotalAmount = DictionaryHelper.GetDecimal(data, "allowanceTotalAmount", 0m)
            };
        }

        /// <summary>
        /// Converts a date string to a DateOnly object.
        /// </summary>
        private static DateOnly MapDateOnly(string dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr))
            {
                return DateOnly.FromDateTime(DateTime.Now);
            }

            try
            {
                if (DateTime.TryParse(dateTimeStr, CultureInfo.InvariantCulture, out var dateTime))
                {
                    return DateOnly.FromDateTime(dateTime);
                }
                return DateOnly.FromDateTime(DateTime.Now);
            }
            catch
            {
                return DateOnly.FromDateTime(DateTime.Now);
            }
        }

        /// <summary>
        /// Converts a time string to a TimeOnly object.
        /// </summary>
        private static TimeOnly MapTimeOnly(string dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr))
            {
                return TimeOnly.FromDateTime(DateTime.Now);
            }

            try
            {
                if (TimeOnly.TryParse(dateTimeStr, CultureInfo.InvariantCulture, out var timeOnly))
                {
                    return timeOnly;
                }
                if (DateTime.TryParse(dateTimeStr, CultureInfo.InvariantCulture, out var dateTime))
                {
                    return TimeOnly.FromDateTime(dateTime);
                }
                return TimeOnly.FromDateTime(DateTime.Now);
            }
            catch
            {
                return TimeOnly.FromDateTime(DateTime.Now);
            }
        }

        /// <summary>
        /// Converts a date string to a nullable DateOnly object.
        /// </summary>
        private static DateOnly? MapNullableDateOnly(string? dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr))
            {
                return null;
            }

            try
            {
                if (DateTime.TryParse(dateTimeStr, CultureInfo.InvariantCulture, out var dateTime))
                {
                    return DateOnly.FromDateTime(dateTime);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
