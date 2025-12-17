namespace Zatca.EInvoice.Tests.TestFixtures;

/// <summary>
/// Provides shared test data for invoice testing.
/// Uses the same test data patterns from PHP tests (UUID: 3cf5ee18-ee25-44ea-a444-2c37ba7f28be, ID: SME00023).
/// </summary>
public static class InvoiceTestData
{
    /// <summary>
    /// Returns complete valid invoice data with all required and optional fields.
    /// </summary>
    public static Dictionary<string, object> GetValidInvoiceData()
    {
        return new Dictionary<string, object>
        {
            { "uuid", "3cf5ee18-ee25-44ea-a444-2c37ba7f28be" },
            { "id", "SME00023" },
            { "issueDate", "2024-09-07" },
            { "issueTime", "17:41:08" },
            { "currencyCode", "SAR" },
            { "taxCurrencyCode", "SAR" },
            { "invoiceType", new Dictionary<string, object>
                {
                    { "invoice", "standard" },
                    { "type", "invoice" },
                    { "isThirdParty", false },
                    { "isNominal", false },
                    { "isExport", false },
                    { "isSummary", false },
                    { "isSelfBilled", false }
                }
            },
            { "additionalDocuments", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "id", "ICV" },
                        { "uuid", "10" }
                    },
                    new Dictionary<string, object>
                    {
                        { "id", "PIH" },
                        { "attachment", new Dictionary<string, object>
                            {
                                { "content", "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==" },
                                { "mimeCode", "base64" },
                                { "mimeType", "text/plain" }
                            }
                        }
                    },
                    new Dictionary<string, object>
                    {
                        { "id", "QR" }
                    }
                }
            },
            { "supplier", GetSampleSupplierData() },
            { "customer", GetSampleCustomerData() },
            { "paymentMeans", new Dictionary<string, object>
                {
                    { "code", "10" }
                }
            },
            { "delivery", new Dictionary<string, object>
                {
                    { "actualDeliveryDate", "2022-09-07" }
                }
            },
            { "allowanceCharges", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "isCharge", false },
                        { "reason", "discount" },
                        { "amount", 0.00m },
                        { "taxCategories", new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    { "percent", 15m },
                                    { "taxScheme", new Dictionary<string, object>
                                        {
                                            { "id", "VAT" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 0.6m },
                    { "subTotals", new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                { "taxableAmount", 4m },
                                { "taxAmount", 0.6m },
                                { "taxCategory", new Dictionary<string, object>
                                    {
                                        { "percent", 15m },
                                        { "taxScheme", new Dictionary<string, object>
                                            {
                                                { "id", "VAT" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 4m },
                    { "taxExclusiveAmount", 4m },
                    { "taxInclusiveAmount", 4.60m },
                    { "prepaidAmount", 0m },
                    { "payableAmount", 4.60m },
                    { "allowanceTotalAmount", 0m }
                }
            },
            { "invoiceLines", new List<object>
                {
                    GetSampleInvoiceLineData()
                }
            }
        };
    }

    /// <summary>
    /// Returns minimal invoice data with only required fields.
    /// </summary>
    public static Dictionary<string, object> GetMinimalInvoiceData()
    {
        return new Dictionary<string, object>
        {
            { "uuid", "3cf5ee18-ee25-44ea-a444-2c37ba7f28be" },
            { "id", "SME00023" },
            { "issueDate", "2024-09-07" },
            { "currencyCode", "SAR" },
            { "taxCurrencyCode", "SAR" },
            { "invoiceType", new Dictionary<string, object>
                {
                    { "invoice", "simplified" },
                    { "type", "invoice" }
                }
            },
            { "additionalDocuments", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "id", "ICV" },
                        { "uuid", "1" }
                    },
                    new Dictionary<string, object>
                    {
                        { "id", "PIH" },
                        { "attachment", new Dictionary<string, object>
                            {
                                { "content", "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==" },
                                { "mimeCode", "base64" },
                                { "mimeType", "text/plain" }
                            }
                        }
                    }
                }
            },
            { "supplier", GetSampleSupplierData() },
            { "paymentMeans", new Dictionary<string, object>
                {
                    { "code", "10" }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 0.6m }
                }
            },
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 4m },
                    { "taxExclusiveAmount", 4m },
                    { "taxInclusiveAmount", 4.60m },
                    { "payableAmount", 4.60m }
                }
            },
            { "invoiceLines", new List<object>
                {
                    GetSampleInvoiceLineData()
                }
            }
        };
    }

    /// <summary>
    /// Returns sample supplier party data.
    /// </summary>
    public static Dictionary<string, object> GetSampleSupplierData()
    {
        return new Dictionary<string, object>
        {
            { "registrationName", "Maximum Speed Tech Supply" },
            { "taxId", "399999999900003" },
            { "taxScheme", new Dictionary<string, object>
                {
                    { "id", "VAT" }
                }
            },
            { "identificationId", "1010010000" },
            { "identificationType", "CRN" },
            { "address", new Dictionary<string, object>
                {
                    { "street", "Prince Sultan" },
                    { "buildingNumber", "2322" },
                    { "subdivision", "Al-Murabba" },
                    { "city", "Riyadh" },
                    { "postalZone", "23333" },
                    { "country", "SA" }
                }
            }
        };
    }

    /// <summary>
    /// Returns sample customer party data.
    /// </summary>
    public static Dictionary<string, object> GetSampleCustomerData()
    {
        return new Dictionary<string, object>
        {
            { "registrationName", "Fatoora Samples" },
            { "taxId", "399999999800003" },
            { "taxScheme", new Dictionary<string, object>
                {
                    { "id", "VAT" }
                }
            },
            { "address", new Dictionary<string, object>
                {
                    { "street", "Salah Al-Din" },
                    { "buildingNumber", "1111" },
                    { "subdivision", "Al-Murooj" },
                    { "city", "Riyadh" },
                    { "postalZone", "12222" },
                    { "country", "SA" }
                }
            }
        };
    }

    /// <summary>
    /// Returns sample invoice line data.
    /// </summary>
    public static Dictionary<string, object> GetSampleInvoiceLineData()
    {
        return new Dictionary<string, object>
        {
            { "id", "1" },
            { "unitCode", "PCE" },
            { "quantity", 2m },
            { "lineExtensionAmount", 4m },
            { "item", new Dictionary<string, object>
                {
                    { "name", "Product" },
                    { "classifiedTaxCategory", new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                { "percent", 15m },
                                { "taxScheme", new Dictionary<string, object>
                                    {
                                        { "id", "VAT" }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            { "price", new Dictionary<string, object>
                {
                    { "amount", 2m },
                    { "unitCode", "UNIT" },
                    { "allowanceCharges", new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                { "isCharge", true },
                                { "reason", "discount" },
                                { "amount", 0.00m }
                            }
                        }
                    }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 0.60m },
                    { "roundingAmount", 4.60m }
                }
            }
        };
    }
}
