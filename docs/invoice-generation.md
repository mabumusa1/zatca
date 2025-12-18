---
layout: default
title: Invoice Generation
---

# Invoice Generation

This guide covers creating and generating ZATCA-compliant invoices in XML format.

## Overview

The invoice generation process involves:
1. Creating invoice data as a dictionary structure
2. Using the `InvoiceMapper` to create an `Invoice` object
3. Using the `InvoiceGenerator` to produce XML output

## InvoiceGenerator

### Basic Usage

```csharp
using Zatca.EInvoice.Xml;

var generator = new InvoiceGenerator();
string xml = generator.Generate(invoiceData);
```

---

## Invoice Data Structure

### Required Fields

```csharp
var invoiceData = new Dictionary<string, object>
{
    // Unique identifier (UUID v4)
    ["uuid"] = "3cf5ee18-ee25-44ea-a444-2c37ba7f28be",

    // Sequential invoice number
    ["id"] = "INV-2024-00001",

    // Issue date and time
    ["issueDate"] = "2024-01-15",
    ["issueTime"] = "14:30:00",

    // Currency codes
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    // Invoice type configuration
    ["invoiceType"] = new Dictionary<string, object> { ... },

    // Supplier information
    ["supplier"] = new Dictionary<string, object> { ... },

    // Customer information
    ["customer"] = new Dictionary<string, object> { ... },

    // Tax totals
    ["taxTotal"] = new Dictionary<string, object> { ... },

    // Monetary totals
    ["legalMonetaryTotal"] = new Dictionary<string, object> { ... },

    // Invoice lines
    ["invoiceLines"] = new List<object> { ... }
};
```

---

## Invoice Type

```csharp
["invoiceType"] = new Dictionary<string, object>
{
    // "standard" for B2B, "simplified" for B2C
    ["invoice"] = "simplified",

    // Invoice type code
    ["type"] = "388",

    // Transaction flags
    ["isThirdParty"] = false,
    ["isNominal"] = false,
    ["isExport"] = false,
    ["isSummary"] = false,
    ["isSelfBilled"] = false
}
```

### Invoice Type Codes

| Code | Name | Description |
|------|------|-------------|
| 388 | Tax Invoice | Standard tax invoice |
| 381 | Credit Note | Refund/return credit |
| 383 | Debit Note | Additional charges |

### Invoice Categories

| Category | Description | Submission |
|----------|-------------|------------|
| Standard | B2B transactions | Clearance required |
| Simplified | B2C transactions | Reporting only |

---

## Supplier Information

```csharp
["supplier"] = new Dictionary<string, object>
{
    // Legal registration name
    ["registrationName"] = "My Company Ltd",

    // VAT registration number
    ["taxId"] = "312345678901233",

    // Commercial registration number
    ["identificationId"] = "1010101010",
    ["identificationType"] = "CRN",

    // Tax scheme
    ["taxScheme"] = new Dictionary<string, object>
    {
        ["id"] = "VAT"
    },

    // Address
    ["address"] = new Dictionary<string, object>
    {
        ["street"] = "King Fahd Road",
        ["additionalStreet"] = "Near City Center",
        ["buildingNumber"] = "1234",
        ["subdivision"] = "Al-Olaya",
        ["city"] = "Riyadh",
        ["postalZone"] = "12345",
        ["countrySubentity"] = "Riyadh Region",
        ["country"] = "SA"
    }
}
```

### Identification Types

| Type | Description |
|------|-------------|
| CRN | Commercial Registration Number |
| MOM | Momra License |
| MLS | MLSD License |
| SAG | Sagia License |
| NAT | National ID |
| GCC | GCC ID |
| IQA | Iqama Number |
| PAS | Passport Number |
| OTH | Other |

---

## Customer Information

```csharp
["customer"] = new Dictionary<string, object>
{
    ["registrationName"] = "Customer Company",
    ["taxId"] = "399999999900003",

    // Optional identification
    ["identificationId"] = "987654321",
    ["identificationType"] = "NAT",

    ["taxScheme"] = new Dictionary<string, object>
    {
        ["id"] = "VAT"
    },

    ["address"] = new Dictionary<string, object>
    {
        ["street"] = "Prince Sultan Street",
        ["buildingNumber"] = "5678",
        ["city"] = "Jeddah",
        ["postalZone"] = "21442",
        ["country"] = "SA"
    }
}
```

---

## Additional Document References

```csharp
["additionalDocuments"] = new List<object>
{
    // Invoice Counter Value (ICV)
    new Dictionary<string, object>
    {
        ["id"] = "ICV",
        ["uuid"] = "1"  // Sequential counter
    },

    // Previous Invoice Hash (PIH)
    new Dictionary<string, object>
    {
        ["id"] = "PIH",
        ["attachment"] = new Dictionary<string, object>
        {
            ["content"] = "previousInvoiceHashBase64==",
            ["mimeCode"] = "text/plain"
        }
    },

    // QR Code placeholder
    new Dictionary<string, object>
    {
        ["id"] = "QR"
    }
}
```

### First Invoice PIH

For the first invoice, use a zero hash:
```csharp
["content"] = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ=="
```

---

## Payment Means

```csharp
["paymentMeans"] = new Dictionary<string, object>
{
    ["code"] = "10"
}
```

### Payment Means Codes

| Code | Description |
|------|-------------|
| 10 | Cash |
| 30 | Credit transfer |
| 42 | Bank account |
| 48 | Bank card |
| 1 | Not defined |

---

## Delivery Information

```csharp
["delivery"] = new Dictionary<string, object>
{
    ["actualDeliveryDate"] = "2024-01-15",
    ["latestDeliveryDate"] = "2024-01-20"  // Optional
}
```

---

## Tax Total

```csharp
["taxTotal"] = new Dictionary<string, object>
{
    ["taxAmount"] = 150.00m,
    ["subTotals"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["taxableAmount"] = 1000.00m,
            ["taxAmount"] = 150.00m,
            ["taxCategory"] = new Dictionary<string, object>
            {
                ["id"] = "S",
                ["percent"] = 15,
                ["taxScheme"] = new Dictionary<string, object>
                {
                    ["id"] = "VAT"
                },
                // For exemptions
                ["taxExemptionReason"] = "Exempt supply",
                ["taxExemptionReasonCode"] = "VATEX-SA-29"
            }
        }
    }
}
```

### Tax Category IDs

| ID | Description |
|----|-------------|
| S | Standard rate |
| Z | Zero rated |
| E | Exempt |
| O | Out of scope |

### Common Exemption Codes

| Code | Description |
|------|-------------|
| VATEX-SA-29 | Financial services |
| VATEX-SA-29-7 | Insurance services |
| VATEX-SA-30 | Real estate |
| VATEX-SA-32 | Qualifying metals |
| VATEX-SA-33 | Private education |
| VATEX-SA-34 | Private healthcare |
| VATEX-SA-35 | Supply of qualified goods |
| VATEX-SA-36 | Export of goods/services |
| VATEX-SA-HEA | Healthcare |
| VATEX-SA-EDU | Education |

---

## Legal Monetary Total

```csharp
["legalMonetaryTotal"] = new Dictionary<string, object>
{
    // Sum of line extension amounts
    ["lineExtensionAmount"] = 1000.00m,

    // Amount before VAT
    ["taxExclusiveAmount"] = 1000.00m,

    // Amount including VAT
    ["taxInclusiveAmount"] = 1150.00m,

    // Any prepaid amount
    ["prepaidAmount"] = 0.00m,

    // Final payable amount
    ["payableAmount"] = 1150.00m,

    // Total allowances
    ["allowanceTotalAmount"] = 0.00m,

    // Total charges
    ["chargeTotalAmount"] = 0.00m
}
```

---

## Invoice Lines

```csharp
["invoiceLines"] = new List<object>
{
    new Dictionary<string, object>
    {
        // Line identifier
        ["id"] = "1",

        // Quantity and unit
        ["quantity"] = 5.0m,
        ["unitCode"] = "PCE",

        // Line total (before VAT)
        ["lineExtensionAmount"] = 500.00m,

        // Item details
        ["item"] = new Dictionary<string, object>
        {
            ["name"] = "Product Name",
            ["description"] = "Product description",
            ["classifiedTaxCategory"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = "S",
                    ["percent"] = 15,
                    ["taxScheme"] = new Dictionary<string, object>
                    {
                        ["id"] = "VAT"
                    }
                }
            }
        },

        // Price per unit
        ["price"] = new Dictionary<string, object>
        {
            ["amount"] = 100.00m,
            ["unitCode"] = "PCE",
            ["allowanceCharges"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["isCharge"] = false,
                    ["reason"] = "Discount",
                    ["amount"] = 0.00m
                }
            }
        },

        // Line tax
        ["taxTotal"] = new Dictionary<string, object>
        {
            ["taxAmount"] = 75.00m,
            ["roundingAmount"] = 575.00m
        }
    }
}
```

### Unit Codes

| Code | Description |
|------|-------------|
| PCE | Piece |
| EA | Each |
| KGM | Kilogram |
| MTR | Meter |
| LTR | Liter |
| H87 | Piece (alternative) |
| DAY | Day |
| HUR | Hour |
| C62 | One (unit) |

---

## Allowances and Charges

### Document Level

```csharp
["allowanceCharges"] = new List<object>
{
    // Discount
    new Dictionary<string, object>
    {
        ["isCharge"] = false,
        ["reason"] = "Promotional discount",
        ["reasonCode"] = "95",
        ["amount"] = 50.00m,
        ["baseAmount"] = 1000.00m,
        ["percentage"] = 5.0m,
        ["taxCategories"] = new List<object>
        {
            new Dictionary<string, object>
            {
                ["id"] = "S",
                ["percent"] = 15,
                ["taxScheme"] = new Dictionary<string, object>
                {
                    ["id"] = "VAT"
                }
            }
        }
    },

    // Charge
    new Dictionary<string, object>
    {
        ["isCharge"] = true,
        ["reason"] = "Delivery fee",
        ["reasonCode"] = "FC",
        ["amount"] = 20.00m
    }
}
```

---

## Billing Reference (Credit/Debit Notes)

For credit or debit notes, reference the original invoice:

```csharp
["billingReference"] = new Dictionary<string, object>
{
    ["id"] = "INV-2024-00001",
    ["uuid"] = "original-invoice-uuid",
    ["issueDate"] = "2024-01-10"
}
```

---

## Order Reference

```csharp
["orderReference"] = new Dictionary<string, object>
{
    ["id"] = "PO-2024-001"
}
```

---

## Contract Reference

```csharp
["contract"] = new Dictionary<string, object>
{
    ["id"] = "CONTRACT-001"
}
```

---

## Complete Example

```csharp
var invoiceData = new Dictionary<string, object>
{
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "INV-2024-00001",
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "simplified",
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
        ["registrationName"] = "My Company Ltd",
        ["taxId"] = "312345678901233",
        ["identificationId"] = "1010101010",
        ["identificationType"] = "CRN",
        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
        ["address"] = new Dictionary<string, object>
        {
            ["street"] = "King Fahd Road",
            ["buildingNumber"] = "1234",
            ["subdivision"] = "Al-Olaya",
            ["city"] = "Riyadh",
            ["postalZone"] = "12345",
            ["country"] = "SA"
        }
    },

    ["customer"] = new Dictionary<string, object>
    {
        ["registrationName"] = "Customer Company",
        ["taxId"] = "399999999900003",
        ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
        ["address"] = new Dictionary<string, object>
        {
            ["street"] = "Prince Sultan Street",
            ["buildingNumber"] = "5678",
            ["city"] = "Jeddah",
            ["postalZone"] = "21442",
            ["country"] = "SA"
        }
    },

    ["paymentMeans"] = new Dictionary<string, object> { ["code"] = "10" },
    ["delivery"] = new Dictionary<string, object> { ["actualDeliveryDate"] = DateTime.Now.ToString("yyyy-MM-dd") },

    ["taxTotal"] = new Dictionary<string, object>
    {
        ["taxAmount"] = 15.00m,
        ["subTotals"] = new List<object>
        {
            new Dictionary<string, object>
            {
                ["taxableAmount"] = 100.00m,
                ["taxAmount"] = 15.00m,
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
        ["lineExtensionAmount"] = 100.00m,
        ["taxExclusiveAmount"] = 100.00m,
        ["taxInclusiveAmount"] = 115.00m,
        ["prepaidAmount"] = 0.00m,
        ["payableAmount"] = 115.00m,
        ["allowanceTotalAmount"] = 0.00m
    },

    ["invoiceLines"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["id"] = "1",
            ["quantity"] = 2.0m,
            ["unitCode"] = "PCE",
            ["lineExtensionAmount"] = 100.00m,
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
                ["amount"] = 50.00m,
                ["unitCode"] = "PCE"
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 15.00m,
                ["roundingAmount"] = 115.00m
            }
        }
    }
};

var generator = new InvoiceGenerator();
string xml = generator.Generate(invoiceData);
```
