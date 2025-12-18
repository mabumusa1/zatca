---
layout: default
title: Getting Started
---

# Getting Started

This guide will walk you through the complete process of integrating ZATCA e-invoicing into your .NET application.

## Prerequisites

Before you begin, ensure you have:

- **.NET 8.0 SDK** or higher installed
- A ZATCA account for obtaining OTP codes
- Basic understanding of e-invoicing concepts

## Installation

Add the package to your project:

```bash
dotnet add package Zatca.EInvoice
```

Or add it to your `.csproj` file:

```xml
<PackageReference Include="Zatca.EInvoice" Version="1.0.0" />
```

## Step 1: Generate a Certificate Signing Request (CSR)

The first step is to generate a CSR that will be submitted to ZATCA to obtain a compliance certificate.

```csharp
using Zatca.EInvoice.Certificates;

// Create the certificate builder
var builder = new CertificateBuilder()
    // Organization Identifier: 15 digits, must start and end with 3
    .SetOrganizationIdentifier("312345678901233")

    // Serial Number: solution name, model, and device serial
    .SetSerialNumber("MySolution", "1.0", "DEVICE001")

    // Certificate details
    .SetCommonName("My Organization Name")
    .SetCountryName("SA")
    .SetOrganizationName("My Company Ltd")
    .SetOrganizationalUnitName("IT Department")
    .SetAddress("Riyadh, King Fahd Road, Building 123")

    // Invoice type: 4 digits (Standard, Simplified, Future, Future)
    // 1100 = Standard + Simplified enabled
    .SetInvoiceType("1100")

    // Environment: false = Sandbox, true = Production
    .SetProduction(false)

    // Business category
    .SetBusinessCategory("Technology");

// Get the CSR and private key
string csr = builder.GetCsr();
string privateKey = builder.GetPrivateKey();

// Save them for later use
File.WriteAllText("certificate.csr", csr);
File.WriteAllText("private.pem", privateKey);
```

### Organization Identifier Format

The organization identifier must be exactly 15 digits:
- First digit: Must be `3`
- Last digit: Must be `3`
- Middle 13 digits: Your VAT registration number

Example: `312345678901233`

### Invoice Type Codes

The invoice type is a 4-digit string where each digit is either `0` or `1`:

| Position | Type | Description |
|----------|------|-------------|
| 1st | Standard | B2B invoices requiring clearance |
| 2nd | Simplified | B2C invoices for reporting |
| 3rd | Future | Reserved for future use |
| 4th | Future | Reserved for future use |

Common values:
- `1000` - Standard invoices only
- `0100` - Simplified invoices only
- `1100` - Both standard and simplified

## Step 2: Request Compliance Certificate from ZATCA

After generating the CSR, submit it to ZATCA to obtain a compliance certificate.

```csharp
using Zatca.EInvoice.Api;

// Create API client (Sandbox or Production)
var apiClient = new ZatcaApiClient(ZatcaEnvironment.Sandbox);

try
{
    // Read the CSR
    string csr = File.ReadAllText("certificate.csr");

    // OTP is provided by ZATCA through their portal
    string otp = "123456";

    // Request the compliance certificate
    var result = await apiClient.RequestComplianceCertificateAsync(csr, otp);

    // Save the certificate data
    var certificateData = new
    {
        Certificate = result.BinarySecurityToken,
        Secret = result.Secret,
        RequestId = result.RequestId
    };

    string json = JsonSerializer.Serialize(certificateData, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    File.WriteAllText("zatca_certificate.json", json);

    Console.WriteLine("Certificate obtained successfully!");
}
catch (ZatcaApiException ex)
{
    Console.WriteLine($"ZATCA API Error: {ex.Message}");
}
```

## Step 3: Create Invoice Data

Prepare your invoice data as a dictionary structure:

```csharp
var invoiceData = new Dictionary<string, object>
{
    // Unique identifier
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "INV-2024-00001",

    // Date and time
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),

    // Currency
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    // Invoice type
    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "simplified",  // or "standard"
        ["type"] = "388",            // Invoice type code
        ["isThirdParty"] = false,
        ["isNominal"] = false,
        ["isExport"] = false,
        ["isSummary"] = false,
        ["isSelfBilled"] = false
    },

    // Additional document references
    ["additionalDocuments"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["id"] = "ICV",
            ["uuid"] = "1"  // Invoice Counter Value
        },
        new Dictionary<string, object>
        {
            ["id"] = "PIH",
            ["attachment"] = new Dictionary<string, object>
            {
                ["content"] = "NWZlY2ViNjZmZmM4...",  // Previous Invoice Hash
                ["mimeCode"] = "text/plain"
            }
        },
        new Dictionary<string, object>
        {
            ["id"] = "QR"  // QR code placeholder
        }
    },

    // Supplier information
    ["supplier"] = new Dictionary<string, object>
    {
        ["registrationName"] = "My Company Ltd",
        ["taxId"] = "312345678901233",
        ["identificationId"] = "1010101010",
        ["identificationType"] = "CRN",
        ["taxScheme"] = new Dictionary<string, object>
        {
            ["id"] = "VAT"
        },
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

    // Customer information
    ["customer"] = new Dictionary<string, object>
    {
        ["registrationName"] = "Customer Company",
        ["taxId"] = "399999999900003",
        ["taxScheme"] = new Dictionary<string, object>
        {
            ["id"] = "VAT"
        },
        ["address"] = new Dictionary<string, object>
        {
            ["street"] = "Prince Sultan Street",
            ["buildingNumber"] = "5678",
            ["subdivision"] = "Al-Sulaymaniyah",
            ["city"] = "Riyadh",
            ["postalZone"] = "54321",
            ["country"] = "SA"
        }
    },

    // Payment means
    ["paymentMeans"] = new Dictionary<string, object>
    {
        ["code"] = "10"  // Cash
    },

    // Delivery
    ["delivery"] = new Dictionary<string, object>
    {
        ["actualDeliveryDate"] = DateTime.Now.ToString("yyyy-MM-dd")
    },

    // Tax totals
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
                    ["percent"] = 15,
                    ["taxScheme"] = new Dictionary<string, object>
                    {
                        ["id"] = "VAT"
                    }
                }
            }
        }
    },

    // Legal monetary total
    ["legalMonetaryTotal"] = new Dictionary<string, object>
    {
        ["lineExtensionAmount"] = 100.00m,
        ["taxExclusiveAmount"] = 100.00m,
        ["taxInclusiveAmount"] = 115.00m,
        ["prepaidAmount"] = 0.00m,
        ["payableAmount"] = 115.00m,
        ["allowanceTotalAmount"] = 0.00m
    },

    // Invoice lines
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
                ["name"] = "Product Name",
                ["classifiedTaxCategory"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["percent"] = 15,
                        ["taxScheme"] = new Dictionary<string, object>
                        {
                            ["id"] = "VAT"
                        }
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
```

## Step 4: Generate XML Invoice

```csharp
using Zatca.EInvoice.Xml;

var generator = new InvoiceGenerator();
string xmlInvoice = generator.Generate(invoiceData);

// Optionally save the unsigned invoice
File.WriteAllText("unsigned_invoice.xml", xmlInvoice);
```

## Step 5: Sign the Invoice

```csharp
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Certificates;

// Load certificate data
string certificateJson = File.ReadAllText("zatca_certificate.json");
var certData = JsonSerializer.Deserialize<Dictionary<string, string>>(certificateJson);

string privateKey = File.ReadAllText("private.pem");

// Create certificate info
var certificate = new CertificateInfo(
    certData["Certificate"],
    privateKey,
    certData["Secret"]
);

// Sign the invoice
var signedResult = InvoiceSigner.Sign(xmlInvoice, certificate);

// Access the results
string signedXml = signedResult.SignedXml;
string invoiceHash = signedResult.InvoiceHash;
string qrCode = signedResult.QrCode;

// Save the signed invoice
File.WriteAllText("signed_invoice.xml", signedXml);
```

## Step 6: Submit to ZATCA

### Compliance Check (Testing)

```csharp
var submissionResult = await apiClient.ValidateInvoiceComplianceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceData["uuid"].ToString(),
    certificate.RawCertificate,
    certificate.Secret
);

if (submissionResult.ClearanceStatus == "CLEARED")
{
    Console.WriteLine("Invoice passed compliance check!");
}
else
{
    Console.WriteLine($"Validation failed: {submissionResult.ClearanceStatus}");
    foreach (var error in submissionResult.ValidationResults?.ErrorMessages ?? new List<string>())
    {
        Console.WriteLine($"  - {error}");
    }
}
```

### Production Submission

Once you have a production certificate:

```csharp
// For simplified invoices (reporting)
var reportResult = await apiClient.ReportInvoiceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceData["uuid"].ToString(),
    certificate.RawCertificate,
    certificate.Secret
);

// For standard invoices (clearance)
var clearanceResult = await apiClient.ClearInvoiceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceData["uuid"].ToString(),
    certificate.RawCertificate,
    certificate.Secret
);
```

## Complete Example

Here's a complete working example:

```csharp
using System.Text.Json;
using Zatca.EInvoice.Api;
using Zatca.EInvoice.Certificates;
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Xml;

// Step 1: Generate CSR (one-time setup)
var builder = new CertificateBuilder()
    .SetOrganizationIdentifier("312345678901233")
    .SetSerialNumber("MyApp", "1.0", "DEV001")
    .SetCommonName("My Company")
    .SetCountryName("SA")
    .SetOrganizationName("My Company Ltd")
    .SetOrganizationalUnitName("IT")
    .SetAddress("Riyadh")
    .SetInvoiceType("1100")
    .SetProduction(false)
    .SetBusinessCategory("Retail");

string csr = builder.GetCsr();
string privateKey = builder.GetPrivateKey();

// Step 2: Get compliance certificate from ZATCA
var apiClient = new ZatcaApiClient(ZatcaEnvironment.Sandbox);
var certResult = await apiClient.RequestComplianceCertificateAsync(csr, "123456");

// Step 3: Create and generate invoice
var invoiceData = CreateInvoiceData(); // Your invoice data
var generator = new InvoiceGenerator();
string xml = generator.Generate(invoiceData);

// Step 4: Sign the invoice
var certificate = new CertificateInfo(
    certResult.BinarySecurityToken,
    privateKey,
    certResult.Secret
);

var signedResult = InvoiceSigner.Sign(xml, certificate);

// Step 5: Submit to ZATCA
var result = await apiClient.ValidateInvoiceComplianceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceData["uuid"].ToString(),
    certificate.RawCertificate,
    certificate.Secret
);

Console.WriteLine($"Status: {result.ClearanceStatus}");
```

## Next Steps

- [API Reference](api-reference.md) - Learn about all API methods
- [Certificate Management](certificates.md) - Advanced certificate options
- [Invoice Generation](invoice-generation.md) - Detailed invoice creation guide
- [Signing](signing.md) - Understanding digital signatures
- [Validation](validation.md) - Pre-submission validation
