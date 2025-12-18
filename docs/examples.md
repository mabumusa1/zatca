---
layout: default
title: Examples
---

# Examples

This page provides complete, working examples for common ZATCA e-invoicing scenarios.

## Table of Contents

- [Simplified Invoice (B2C)](#simplified-invoice-b2c)
- [Standard Invoice (B2B)](#standard-invoice-b2b)
- [Credit Note](#credit-note)
- [Debit Note](#debit-note)
- [Invoice with Discounts](#invoice-with-discounts)
- [Zero-Rated Invoice](#zero-rated-invoice)
- [Exempt Invoice](#exempt-invoice)
- [Multi-Line Invoice](#multi-line-invoice)

---

## Simplified Invoice (B2C)

A simplified invoice for business-to-consumer transactions.

```csharp
using Zatca.EInvoice.Xml;
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Certificates;

// Create invoice data
var invoiceData = new Dictionary<string, object>
{
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "SMP-2024-00001",
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "simplified",  // B2C
        ["type"] = "388"
    },

    ["additionalDocuments"] = new List<object>
    {
        new Dictionary<string, object> { ["id"] = "ICV", ["uuid"] = "1" },
        new Dictionary<string, object>
        {
            ["id"] = "PIH",
            ["attachment"] = new Dictionary<string, object>
            {
                ["content"] = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==",
                ["mimeCode"] = "text/plain"
            }
        },
        new Dictionary<string, object> { ["id"] = "QR" }
    },

    ["supplier"] = new Dictionary<string, object>
    {
        ["registrationName"] = "Coffee Shop LLC",
        ["taxId"] = "312345678901233",
        ["identificationId"] = "1010101010",
        ["identificationType"] = "CRN",
        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
        ["address"] = new Dictionary<string, object>
        {
            ["street"] = "King Abdullah Road",
            ["buildingNumber"] = "1234",
            ["subdivision"] = "Al-Malaz",
            ["city"] = "Riyadh",
            ["postalZone"] = "12345",
            ["country"] = "SA"
        }
    },

    // Simplified invoices may have minimal customer info
    ["customer"] = new Dictionary<string, object>
    {
        ["registrationName"] = "Walk-in Customer",
        ["address"] = new Dictionary<string, object>
        {
            ["city"] = "Riyadh",
            ["country"] = "SA"
        }
    },

    ["paymentMeans"] = new Dictionary<string, object> { ["code"] = "10" }, // Cash
    ["delivery"] = new Dictionary<string, object>
    {
        ["actualDeliveryDate"] = DateTime.Now.ToString("yyyy-MM-dd")
    },

    ["taxTotal"] = new Dictionary<string, object>
    {
        ["taxAmount"] = 3.75m,
        ["subTotals"] = new List<object>
        {
            new Dictionary<string, object>
            {
                ["taxableAmount"] = 25.00m,
                ["taxAmount"] = 3.75m,
                ["taxCategory"] = new Dictionary<string, object>
                {
                    ["id"] = "S",
                    ["percent"] = 15,
                    ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                }
            }
        }
    },

    ["legalMonetaryTotal"] = new Dictionary<string, object>
    {
        ["lineExtensionAmount"] = 25.00m,
        ["taxExclusiveAmount"] = 25.00m,
        ["taxInclusiveAmount"] = 28.75m,
        ["payableAmount"] = 28.75m
    },

    ["invoiceLines"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["id"] = "1",
            ["quantity"] = 1.0m,
            ["unitCode"] = "PCE",
            ["lineExtensionAmount"] = 25.00m,
            ["item"] = new Dictionary<string, object>
            {
                ["name"] = "Cappuccino Large",
                ["classifiedTaxCategory"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "S",
                        ["percent"] = 15,
                        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                    }
                }
            },
            ["price"] = new Dictionary<string, object>
            {
                ["amount"] = 25.00m,
                ["unitCode"] = "PCE"
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 3.75m,
                ["roundingAmount"] = 28.75m
            }
        }
    }
};

// Generate XML
var generator = new InvoiceGenerator();
string xml = generator.Generate(invoiceData);

// Sign the invoice
var certificate = new CertificateInfo(certContent, privateKey, secret);
var signedResult = InvoiceSigner.Sign(xml, certificate);

// Submit to ZATCA (reporting for simplified)
var apiClient = new ZatcaApiClient(ZatcaEnvironment.Production);
var result = await apiClient.ReportInvoiceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceData["uuid"].ToString(),
    certificate.RawCertificate,
    certificate.Secret
);
```

---

## Standard Invoice (B2B)

A standard invoice for business-to-business transactions requiring clearance.

```csharp
var invoiceData = new Dictionary<string, object>
{
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "STD-2024-00001",
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "standard",  // B2B - requires clearance
        ["type"] = "388"
    },

    ["additionalDocuments"] = new List<object>
    {
        new Dictionary<string, object> { ["id"] = "ICV", ["uuid"] = "100" },
        new Dictionary<string, object>
        {
            ["id"] = "PIH",
            ["attachment"] = new Dictionary<string, object>
            {
                ["content"] = "previousHashBase64==",
                ["mimeCode"] = "text/plain"
            }
        },
        new Dictionary<string, object> { ["id"] = "QR" }
    },

    ["supplier"] = new Dictionary<string, object>
    {
        ["registrationName"] = "Tech Solutions Ltd",
        ["taxId"] = "312345678901233",
        ["identificationId"] = "1010101010",
        ["identificationType"] = "CRN",
        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
        ["address"] = new Dictionary<string, object>
        {
            ["street"] = "King Fahd Road",
            ["buildingNumber"] = "5678",
            ["subdivision"] = "Al-Olaya",
            ["city"] = "Riyadh",
            ["postalZone"] = "12345",
            ["country"] = "SA"
        }
    },

    // Standard invoices require full customer details
    ["customer"] = new Dictionary<string, object>
    {
        ["registrationName"] = "Enterprise Corp",
        ["taxId"] = "399999999900003",
        ["identificationId"] = "2020202020",
        ["identificationType"] = "CRN",
        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
        ["address"] = new Dictionary<string, object>
        {
            ["street"] = "Prince Sultan Street",
            ["buildingNumber"] = "9012",
            ["subdivision"] = "Al-Rawdah",
            ["city"] = "Jeddah",
            ["postalZone"] = "21442",
            ["country"] = "SA"
        }
    },

    ["paymentMeans"] = new Dictionary<string, object> { ["code"] = "30" }, // Credit transfer
    ["delivery"] = new Dictionary<string, object>
    {
        ["actualDeliveryDate"] = DateTime.Now.ToString("yyyy-MM-dd")
    },

    ["taxTotal"] = new Dictionary<string, object>
    {
        ["taxAmount"] = 1500.00m,
        ["subTotals"] = new List<object>
        {
            new Dictionary<string, object>
            {
                ["taxableAmount"] = 10000.00m,
                ["taxAmount"] = 1500.00m,
                ["taxCategory"] = new Dictionary<string, object>
                {
                    ["id"] = "S",
                    ["percent"] = 15,
                    ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                }
            }
        }
    },

    ["legalMonetaryTotal"] = new Dictionary<string, object>
    {
        ["lineExtensionAmount"] = 10000.00m,
        ["taxExclusiveAmount"] = 10000.00m,
        ["taxInclusiveAmount"] = 11500.00m,
        ["payableAmount"] = 11500.00m
    },

    ["invoiceLines"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["id"] = "1",
            ["quantity"] = 10.0m,
            ["unitCode"] = "PCE",
            ["lineExtensionAmount"] = 10000.00m,
            ["item"] = new Dictionary<string, object>
            {
                ["name"] = "Software License - Annual",
                ["classifiedTaxCategory"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "S",
                        ["percent"] = 15,
                        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                    }
                }
            },
            ["price"] = new Dictionary<string, object>
            {
                ["amount"] = 1000.00m,
                ["unitCode"] = "PCE"
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 1500.00m,
                ["roundingAmount"] = 11500.00m
            }
        }
    }
};

// Generate, sign, and submit for clearance
var generator = new InvoiceGenerator();
string xml = generator.Generate(invoiceData);

var certificate = new CertificateInfo(certContent, privateKey, secret);
var signedResult = InvoiceSigner.Sign(xml, certificate);

// Clearance required for standard invoices
var result = await apiClient.ClearInvoiceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceData["uuid"].ToString(),
    certificate.RawCertificate,
    certificate.Secret
);

if (result.ClearanceStatus == "CLEARED")
{
    // Use the cleared invoice from ZATCA
    string clearedXml = result.ClearedInvoice;
}
```

---

## Credit Note

A credit note for refunds or returns.

```csharp
var creditNoteData = new Dictionary<string, object>
{
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "CN-2024-00001",
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "simplified",
        ["type"] = "381"  // Credit Note
    },

    // Reference to original invoice
    ["billingReference"] = new Dictionary<string, object>
    {
        ["id"] = "SMP-2024-00001",
        ["uuid"] = "original-invoice-uuid-here",
        ["issueDate"] = "2024-01-15"
    },

    ["additionalDocuments"] = new List<object>
    {
        new Dictionary<string, object> { ["id"] = "ICV", ["uuid"] = "2" },
        new Dictionary<string, object>
        {
            ["id"] = "PIH",
            ["attachment"] = new Dictionary<string, object>
            {
                ["content"] = "previousHashBase64==",
                ["mimeCode"] = "text/plain"
            }
        },
        new Dictionary<string, object> { ["id"] = "QR" }
    },

    ["supplier"] = new Dictionary<string, object>
    {
        ["registrationName"] = "Coffee Shop LLC",
        ["taxId"] = "312345678901233",
        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
        ["address"] = new Dictionary<string, object>
        {
            ["street"] = "King Abdullah Road",
            ["buildingNumber"] = "1234",
            ["city"] = "Riyadh",
            ["postalZone"] = "12345",
            ["country"] = "SA"
        }
    },

    ["customer"] = new Dictionary<string, object>
    {
        ["registrationName"] = "Customer Name",
        ["address"] = new Dictionary<string, object>
        {
            ["city"] = "Riyadh",
            ["country"] = "SA"
        }
    },

    ["paymentMeans"] = new Dictionary<string, object> { ["code"] = "10" },

    // Credit amounts (refund)
    ["taxTotal"] = new Dictionary<string, object>
    {
        ["taxAmount"] = 3.75m,
        ["subTotals"] = new List<object>
        {
            new Dictionary<string, object>
            {
                ["taxableAmount"] = 25.00m,
                ["taxAmount"] = 3.75m,
                ["taxCategory"] = new Dictionary<string, object>
                {
                    ["id"] = "S",
                    ["percent"] = 15,
                    ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                }
            }
        }
    },

    ["legalMonetaryTotal"] = new Dictionary<string, object>
    {
        ["lineExtensionAmount"] = 25.00m,
        ["taxExclusiveAmount"] = 25.00m,
        ["taxInclusiveAmount"] = 28.75m,
        ["payableAmount"] = 28.75m
    },

    ["invoiceLines"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["id"] = "1",
            ["quantity"] = 1.0m,
            ["unitCode"] = "PCE",
            ["lineExtensionAmount"] = 25.00m,
            ["item"] = new Dictionary<string, object>
            {
                ["name"] = "Cappuccino Large - REFUND",
                ["classifiedTaxCategory"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "S",
                        ["percent"] = 15,
                        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                    }
                }
            },
            ["price"] = new Dictionary<string, object>
            {
                ["amount"] = 25.00m,
                ["unitCode"] = "PCE"
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 3.75m,
                ["roundingAmount"] = 28.75m
            }
        }
    }
};
```

---

## Invoice with Discounts

An invoice with line-level and document-level discounts.

```csharp
var invoiceData = new Dictionary<string, object>
{
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "DSC-2024-00001",
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "simplified",
        ["type"] = "388"
    },

    // ... supplier, customer, etc.

    // Document-level discounts
    ["allowanceCharges"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["isCharge"] = false,  // false = discount
            ["reason"] = "Loyalty Discount",
            ["reasonCode"] = "95",
            ["amount"] = 50.00m,
            ["taxCategories"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = "S",
                    ["percent"] = 15,
                    ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                }
            }
        }
    },

    ["taxTotal"] = new Dictionary<string, object>
    {
        ["taxAmount"] = 67.50m,  // 15% of (500 - 50)
        ["subTotals"] = new List<object>
        {
            new Dictionary<string, object>
            {
                ["taxableAmount"] = 450.00m,
                ["taxAmount"] = 67.50m,
                ["taxCategory"] = new Dictionary<string, object>
                {
                    ["id"] = "S",
                    ["percent"] = 15,
                    ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                }
            }
        }
    },

    ["legalMonetaryTotal"] = new Dictionary<string, object>
    {
        ["lineExtensionAmount"] = 500.00m,
        ["allowanceTotalAmount"] = 50.00m,  // Total discounts
        ["taxExclusiveAmount"] = 450.00m,   // After discount
        ["taxInclusiveAmount"] = 517.50m,
        ["payableAmount"] = 517.50m
    },

    ["invoiceLines"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["id"] = "1",
            ["quantity"] = 5.0m,
            ["unitCode"] = "PCE",
            ["lineExtensionAmount"] = 500.00m,
            ["item"] = new Dictionary<string, object>
            {
                ["name"] = "Premium Widget",
                ["classifiedTaxCategory"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "S",
                        ["percent"] = 15,
                        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                    }
                }
            },
            ["price"] = new Dictionary<string, object>
            {
                ["amount"] = 100.00m,
                ["unitCode"] = "PCE",
                // Line-level discount
                ["allowanceCharges"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["isCharge"] = false,
                        ["reason"] = "Volume discount",
                        ["amount"] = 0.00m
                    }
                }
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 67.50m,
                ["roundingAmount"] = 517.50m
            }
        }
    }
};
```

---

## Zero-Rated Invoice

An invoice for zero-rated supplies (exports).

```csharp
var invoiceData = new Dictionary<string, object>
{
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "EXP-2024-00001",
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "standard",
        ["type"] = "388",
        ["isExport"] = true  // Export invoice
    },

    // ... supplier, customer, additionalDocuments

    ["taxTotal"] = new Dictionary<string, object>
    {
        ["taxAmount"] = 0.00m,
        ["subTotals"] = new List<object>
        {
            new Dictionary<string, object>
            {
                ["taxableAmount"] = 1000.00m,
                ["taxAmount"] = 0.00m,
                ["taxCategory"] = new Dictionary<string, object>
                {
                    ["id"] = "Z",  // Zero-rated
                    ["percent"] = 0,
                    ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
                    ["taxExemptionReason"] = "Export of goods",
                    ["taxExemptionReasonCode"] = "VATEX-SA-36"
                }
            }
        }
    },

    ["legalMonetaryTotal"] = new Dictionary<string, object>
    {
        ["lineExtensionAmount"] = 1000.00m,
        ["taxExclusiveAmount"] = 1000.00m,
        ["taxInclusiveAmount"] = 1000.00m,  // No VAT
        ["payableAmount"] = 1000.00m
    },

    ["invoiceLines"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["id"] = "1",
            ["quantity"] = 100.0m,
            ["unitCode"] = "PCE",
            ["lineExtensionAmount"] = 1000.00m,
            ["item"] = new Dictionary<string, object>
            {
                ["name"] = "Export Product",
                ["classifiedTaxCategory"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "Z",
                        ["percent"] = 0,
                        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                    }
                }
            },
            ["price"] = new Dictionary<string, object>
            {
                ["amount"] = 10.00m,
                ["unitCode"] = "PCE"
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 0.00m,
                ["roundingAmount"] = 1000.00m
            }
        }
    }
};
```

---

## Multi-Line Invoice

An invoice with multiple line items and mixed tax categories.

```csharp
var invoiceData = new Dictionary<string, object>
{
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "MLT-2024-00001",
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "simplified",
        ["type"] = "388"
    },

    // ... supplier, customer, additionalDocuments

    ["taxTotal"] = new Dictionary<string, object>
    {
        ["taxAmount"] = 22.50m,  // Total tax
        ["subTotals"] = new List<object>
        {
            // Standard rate items
            new Dictionary<string, object>
            {
                ["taxableAmount"] = 150.00m,
                ["taxAmount"] = 22.50m,
                ["taxCategory"] = new Dictionary<string, object>
                {
                    ["id"] = "S",
                    ["percent"] = 15,
                    ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                }
            }
        }
    },

    ["legalMonetaryTotal"] = new Dictionary<string, object>
    {
        ["lineExtensionAmount"] = 150.00m,
        ["taxExclusiveAmount"] = 150.00m,
        ["taxInclusiveAmount"] = 172.50m,
        ["payableAmount"] = 172.50m
    },

    ["invoiceLines"] = new List<object>
    {
        // Line 1
        new Dictionary<string, object>
        {
            ["id"] = "1",
            ["quantity"] = 2.0m,
            ["unitCode"] = "PCE",
            ["lineExtensionAmount"] = 50.00m,
            ["item"] = new Dictionary<string, object>
            {
                ["name"] = "Product A",
                ["classifiedTaxCategory"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "S",
                        ["percent"] = 15,
                        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                    }
                }
            },
            ["price"] = new Dictionary<string, object>
            {
                ["amount"] = 25.00m,
                ["unitCode"] = "PCE"
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 7.50m,
                ["roundingAmount"] = 57.50m
            }
        },

        // Line 2
        new Dictionary<string, object>
        {
            ["id"] = "2",
            ["quantity"] = 1.0m,
            ["unitCode"] = "PCE",
            ["lineExtensionAmount"] = 75.00m,
            ["item"] = new Dictionary<string, object>
            {
                ["name"] = "Product B",
                ["classifiedTaxCategory"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "S",
                        ["percent"] = 15,
                        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                    }
                }
            },
            ["price"] = new Dictionary<string, object>
            {
                ["amount"] = 75.00m,
                ["unitCode"] = "PCE"
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 11.25m,
                ["roundingAmount"] = 86.25m
            }
        },

        // Line 3
        new Dictionary<string, object>
        {
            ["id"] = "3",
            ["quantity"] = 5.0m,
            ["unitCode"] = "PCE",
            ["lineExtensionAmount"] = 25.00m,
            ["item"] = new Dictionary<string, object>
            {
                ["name"] = "Product C",
                ["classifiedTaxCategory"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "S",
                        ["percent"] = 15,
                        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                    }
                }
            },
            ["price"] = new Dictionary<string, object>
            {
                ["amount"] = 5.00m,
                ["unitCode"] = "PCE"
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 3.75m,
                ["roundingAmount"] = 28.75m
            }
        }
    }
};
```

---

## Helper Functions

### Invoice Counter Management

```csharp
public class InvoiceCounterService
{
    private int _counter;
    private string _lastHash;

    public InvoiceCounterService()
    {
        _counter = 0;
        // Initial hash for first invoice
        _lastHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
    }

    public (int counter, string previousHash) GetNextCounter()
    {
        _counter++;
        return (_counter, _lastHash);
    }

    public void UpdateHash(string newHash)
    {
        _lastHash = newHash;
    }
}
```

### Batch Processing

```csharp
public async Task ProcessInvoiceBatch(List<Dictionary<string, object>> invoices)
{
    var generator = new InvoiceGenerator();
    var apiClient = new ZatcaApiClient(ZatcaEnvironment.Production);
    var certificate = new CertificateInfo(cert, key, secret);

    foreach (var invoiceData in invoices)
    {
        try
        {
            string xml = generator.Generate(invoiceData);
            var signedResult = InvoiceSigner.Sign(xml, certificate);

            var result = await apiClient.ReportInvoiceAsync(
                signedResult.SignedXml,
                signedResult.InvoiceHash,
                invoiceData["uuid"].ToString(),
                certificate.RawCertificate,
                certificate.Secret
            );

            Console.WriteLine($"Invoice {invoiceData["id"]}: {result.ReportingStatus}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing {invoiceData["id"]}: {ex.Message}");
        }
    }
}
```
