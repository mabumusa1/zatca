# Zatca.EInvoice

A comprehensive .NET library for generating **ZATCA-compliant** e-invoices in Saudi Arabia. This library provides everything you need to create, sign, validate, and submit electronic invoices to ZATCA's Fatoora platform.

## Features

- **Certificate Management** - Generate CSRs, obtain compliance/production certificates, and renew certificates
- **Invoice Generation** - Create standard (B2B) and simplified (B2C) invoices in UBL 2.1 XML format
- **Digital Signing** - Sign invoices with ECDSA signatures using your ZATCA certificate
- **QR Code Generation** - Automatically generate TLV-encoded QR codes for invoices
- **Invoice Validation** - Validate invoices before submission
- **ZATCA API Integration** - Submit invoices for clearance, reporting, and compliance checks
- **Full Environment Support** - Works with Sandbox, Simulation, and Production environments

## Requirements

- **.NET 8.0** or higher
- **BouncyCastle.Cryptography** (automatically installed via NuGet)

## Installation

### NuGet Package Manager

```bash
dotnet add package Zatca.EInvoice
```

### Package Reference

```xml
<PackageReference Include="Zatca.EInvoice" Version="1.0.0" />
```

## Quick Start

```csharp
using Zatca.EInvoice.Api;
using Zatca.EInvoice.Certificates;
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Xml;

// 1. Generate CSR and get compliance certificate
var builder = new CertificateBuilder()
    .SetOrganizationIdentifier("312345678901233")
    .SetSerialNumber("MyApp", "1.0", "DEVICE001")
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

// 2. Get compliance certificate from ZATCA
var apiClient = new ZatcaApiClient(ZatcaEnvironment.Sandbox);
var certResult = await apiClient.RequestComplianceCertificateAsync(csr, "123456");

// 3. Create and sign invoice
var certificate = new CertificateInfo(
    certResult.BinarySecurityToken,
    privateKey,
    certResult.Secret
);

var invoiceData = CreateInvoiceData(); // Your invoice data
var generator = new InvoiceGenerator();
string xml = generator.Generate(invoiceData);

var signedResult = InvoiceSigner.Sign(xml, certificate);

// 4. Submit to ZATCA
var result = await apiClient.ValidateInvoiceComplianceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceData["uuid"].ToString(),
    certificate.RawCertificate,
    certificate.Secret
);

Console.WriteLine($"Status: {result.ClearanceStatus}");
```

---

## Integration Guide

### Step 1: Certificate Setup (One-Time)

Before you can submit invoices, you need to obtain certificates from ZATCA.

#### Generate a Certificate Signing Request (CSR)

```csharp
using Zatca.EInvoice.Certificates;

var builder = new CertificateBuilder()
    // VAT Registration Number (15 digits, starts and ends with 3)
    .SetOrganizationIdentifier("312345678901233")

    // Device/Solution identification
    .SetSerialNumber("YourERPName", "1.0", "POS-001")

    // Organization details
    .SetCommonName("Your Company Name")
    .SetCountryName("SA")
    .SetOrganizationName("Your Company Ltd")
    .SetOrganizationalUnitName("IT Department")
    .SetAddress("Riyadh, King Fahd Road, Building 123")

    // Invoice types: 1100 = Standard + Simplified
    .SetInvoiceType("1100")

    // Environment: false = Sandbox, true = Production
    .SetProduction(false)

    .SetBusinessCategory("Retail");

// Generate CSR and private key
string csr = builder.GetCsr();
string privateKey = builder.GetPrivateKey();

// IMPORTANT: Store the private key securely!
File.WriteAllText("private.pem", privateKey);
File.WriteAllText("csr.pem", csr);
```

#### Obtain Compliance Certificate

```csharp
using Zatca.EInvoice.Api;

var apiClient = new ZatcaApiClient(ZatcaEnvironment.Sandbox);

try
{
    // OTP is provided by ZATCA through their portal
    var result = await apiClient.RequestComplianceCertificateAsync(csr, "123456");

    if (result.IsSuccess)
    {
        Console.WriteLine("Certificate obtained successfully!");

        // Store these securely
        var credentials = new
        {
            Certificate = result.BinarySecurityToken,
            Secret = result.Secret,
            RequestId = result.RequestId
        };

        File.WriteAllText("compliance_cert.json",
            JsonSerializer.Serialize(credentials, new JsonSerializerOptions { WriteIndented = true }));
    }
}
catch (ZatcaApiException ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
}
```

#### Obtain Production Certificate

After passing all compliance checks:

```csharp
var productionResult = await apiClient.RequestProductionCertificateAsync(
    complianceResult.RequestId,
    complianceResult.BinarySecurityToken,
    complianceResult.Secret
);

if (productionResult.IsSuccess)
{
    // Store production credentials securely
    File.WriteAllText("production_cert.pem", productionResult.BinarySecurityToken);
}
```

---

### Step 2: Create Invoice Data

Prepare your invoice data as a dictionary structure:

```csharp
var invoiceData = new Dictionary<string, object>
{
    // Unique identifiers
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "INV-2024-00001",

    // Date and time
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),

    // Currency
    ["currencyCode"] = "SAR",
    ["taxCurrencyCode"] = "SAR",

    // Invoice type configuration
    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "simplified",  // or "standard" for B2B
        ["type"] = "388",            // 388=Invoice, 381=Credit Note, 383=Debit Note
        ["isThirdParty"] = false,
        ["isNominal"] = false,
        ["isExport"] = false,
        ["isSummary"] = false,
        ["isSelfBilled"] = false
    },

    // Required document references
    ["additionalDocuments"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["id"] = "ICV",
            ["uuid"] = "1"  // Invoice Counter Value (increment for each invoice)
        },
        new Dictionary<string, object>
        {
            ["id"] = "PIH",
            ["attachment"] = new Dictionary<string, object>
            {
                // Previous Invoice Hash (use zeros for first invoice)
                ["content"] = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYmYxNGI1N2JkNjVmMDQ1NDIwMzhjNzY2Y2E5ZQ==",
                ["mimeCode"] = "text/plain"
            }
        },
        new Dictionary<string, object>
        {
            ["id"] = "QR"  // QR code placeholder (filled during signing)
        }
    },

    // Supplier (seller) information
    ["supplier"] = new Dictionary<string, object>
    {
        ["registrationName"] = "Your Company Ltd",
        ["taxId"] = "312345678901233",
        ["identificationId"] = "1010101010",
        ["identificationType"] = "CRN",  // Commercial Registration Number
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

    // Customer (buyer) information
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

    // Payment information
    ["paymentMeans"] = new Dictionary<string, object>
    {
        ["code"] = "10"  // 10=Cash, 30=Credit, 42=Bank Account, 48=Card
    },

    // Delivery date
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

    // Monetary totals
    ["legalMonetaryTotal"] = new Dictionary<string, object>
    {
        ["lineExtensionAmount"] = 100.00m,
        ["taxExclusiveAmount"] = 100.00m,
        ["taxInclusiveAmount"] = 115.00m,
        ["prepaidAmount"] = 0.00m,
        ["payableAmount"] = 115.00m,
        ["allowanceTotalAmount"] = 0.00m
    },

    // Invoice lines (items)
    ["invoiceLines"] = new List<object>
    {
        new Dictionary<string, object>
        {
            ["id"] = "1",
            ["quantity"] = 2.0m,
            ["unitCode"] = "PCE",  // PCE=Piece, EA=Each, KGM=Kilogram, etc.
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

---

### Step 3: Generate and Sign Invoice

```csharp
using Zatca.EInvoice.Xml;
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Certificates;

// Load your certificate
var certificate = new CertificateInfo(
    File.ReadAllText("compliance_cert.pem"),  // or production cert
    File.ReadAllText("private.pem"),
    "your-secret-from-zatca"
);

// Generate XML invoice
var generator = new InvoiceGenerator();
string xmlInvoice = generator.Generate(invoiceData);

// Sign the invoice
var signedResult = InvoiceSigner.Sign(xmlInvoice, certificate);

// Access the results
string signedXml = signedResult.SignedXml;      // Signed invoice XML
string invoiceHash = signedResult.InvoiceHash;  // Hash for submission
string qrCode = signedResult.QrCode;            // Base64 QR code data

// Save the signed invoice
File.WriteAllText("signed_invoice.xml", signedXml);
```

---

### Step 4: Submit to ZATCA

#### Compliance Check (Testing Phase)

```csharp
var apiClient = new ZatcaApiClient(ZatcaEnvironment.Sandbox);

var result = await apiClient.ValidateInvoiceComplianceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceData["uuid"].ToString(),
    certificate.RawCertificate,
    certificate.Secret
);

if (result.IsSuccess)
{
    Console.WriteLine("Invoice passed compliance check!");
}
else
{
    Console.WriteLine("Validation failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  [{error.Code}] {error.Message}");
    }
}

// Check for warnings
if (result.HasWarnings)
{
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"Warning: {warning.Message}");
    }
}
```

#### Production: Report Simplified Invoices (B2C)

```csharp
var apiClient = new ZatcaApiClient(ZatcaEnvironment.Production);

var result = await apiClient.SubmitReportingInvoiceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceUuid,
    productionCertificate,
    productionSecret
);

if (result.IsSuccess)
{
    Console.WriteLine($"Reporting Status: {result.ReportingStatus}");
}
```

#### Production: Clear Standard Invoices (B2B)

```csharp
var result = await apiClient.SubmitClearanceInvoiceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceUuid,
    productionCertificate,
    productionSecret
);

if (result.IsSuccess)
{
    Console.WriteLine($"Clearance Status: {result.ClearanceStatus}");

    // For clearance invoices, use the returned cleared invoice
    string clearedInvoice = result.ClearedInvoice;
}
```

---

### Step 5: Certificate Renewal

Production certificates expire and must be renewed:

```csharp
// Generate new CSR with same organization details
var builder = new CertificateBuilder()
    .SetOrganizationIdentifier("312345678901233")
    // ... same settings as original
    .SetProduction(true);

string newCsr = builder.GetCsr();
string newPrivateKey = builder.GetPrivateKey();

// Renew certificate
var renewalResult = await apiClient.RenewProductionCertificateAsync(
    "123456",  // New OTP from ZATCA portal
    newCsr,
    currentCertificate,
    currentSecret
);

if (renewalResult.IsSuccess)
{
    // Save new credentials
    Console.WriteLine("Certificate renewed successfully!");
}
```

---

## Complete Integration Example

Here's a complete example of integrating the library into an existing application:

```csharp
using System.Text.Json;
using Zatca.EInvoice.Api;
using Zatca.EInvoice.Certificates;
using Zatca.EInvoice.Exceptions;
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Xml;

public class ZatcaInvoiceService
{
    private readonly ZatcaApiClient _apiClient;
    private readonly CertificateInfo _certificate;
    private readonly InvoiceGenerator _generator;
    private int _invoiceCounter = 0;
    private string _previousInvoiceHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYmYxNGI1N2JkNjVmMDQ1NDIwMzhjNzY2Y2E5ZQ==";

    public ZatcaInvoiceService(
        ZatcaEnvironment environment,
        string certificatePath,
        string privateKeyPath,
        string secret)
    {
        _apiClient = new ZatcaApiClient(environment);
        _certificate = new CertificateInfo(
            File.ReadAllText(certificatePath),
            File.ReadAllText(privateKeyPath),
            secret
        );
        _generator = new InvoiceGenerator();
    }

    public async Task<InvoiceResult> CreateAndSubmitInvoiceAsync(
        string invoiceId,
        string customerName,
        string customerTaxId,
        List<InvoiceLineItem> items,
        bool isSimplified = true)
    {
        // Increment counter
        _invoiceCounter++;
        var uuid = Guid.NewGuid().ToString();

        // Calculate totals
        decimal subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
        decimal taxAmount = subtotal * 0.15m;
        decimal total = subtotal + taxAmount;

        // Build invoice data
        var invoiceData = BuildInvoiceData(
            uuid, invoiceId, customerName, customerTaxId,
            items, subtotal, taxAmount, total, isSimplified);

        // Generate XML
        string xml = _generator.Generate(invoiceData);

        // Sign
        var signedResult = InvoiceSigner.Sign(xml, _certificate);

        // Submit
        InvoiceSubmissionResult result;

        try
        {
            if (isSimplified)
            {
                result = await _apiClient.SubmitReportingInvoiceAsync(
                    signedResult.SignedXml,
                    signedResult.InvoiceHash,
                    uuid,
                    _certificate.RawCertificate,
                    _certificate.Secret
                );
            }
            else
            {
                result = await _apiClient.SubmitClearanceInvoiceAsync(
                    signedResult.SignedXml,
                    signedResult.InvoiceHash,
                    uuid,
                    _certificate.RawCertificate,
                    _certificate.Secret
                );
            }

            // Update previous hash for next invoice
            _previousInvoiceHash = signedResult.InvoiceHash;

            return new InvoiceResult
            {
                Success = result.IsSuccess,
                Uuid = uuid,
                InvoiceId = invoiceId,
                SignedXml = signedResult.SignedXml,
                QrCode = signedResult.QrCode,
                ClearanceStatus = result.ClearanceStatus,
                ReportingStatus = result.ReportingStatus,
                Errors = result.Errors.Select(e => e.Message).ToList(),
                Warnings = result.Warnings.Select(w => w.Message).ToList()
            };
        }
        catch (ZatcaApiException ex)
        {
            return new InvoiceResult
            {
                Success = false,
                Uuid = uuid,
                InvoiceId = invoiceId,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private Dictionary<string, object> BuildInvoiceData(
        string uuid, string invoiceId, string customerName, string customerTaxId,
        List<InvoiceLineItem> items, decimal subtotal, decimal taxAmount,
        decimal total, bool isSimplified)
    {
        return new Dictionary<string, object>
        {
            ["uuid"] = uuid,
            ["id"] = invoiceId,
            ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
            ["currencyCode"] = "SAR",
            ["taxCurrencyCode"] = "SAR",
            ["invoiceType"] = new Dictionary<string, object>
            {
                ["invoice"] = isSimplified ? "simplified" : "standard",
                ["type"] = "388"
            },
            ["additionalDocuments"] = new List<object>
            {
                new Dictionary<string, object> { ["id"] = "ICV", ["uuid"] = _invoiceCounter.ToString() },
                new Dictionary<string, object>
                {
                    ["id"] = "PIH",
                    ["attachment"] = new Dictionary<string, object>
                    {
                        ["content"] = _previousInvoiceHash,
                        ["mimeCode"] = "text/plain"
                    }
                },
                new Dictionary<string, object> { ["id"] = "QR" }
            },
            ["supplier"] = new Dictionary<string, object>
            {
                ["registrationName"] = "Your Company Ltd",
                ["taxId"] = "312345678901233",
                ["identificationId"] = "1010101010",
                ["identificationType"] = "CRN",
                ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
                ["address"] = new Dictionary<string, object>
                {
                    ["street"] = "King Fahd Road",
                    ["buildingNumber"] = "1234",
                    ["city"] = "Riyadh",
                    ["postalZone"] = "12345",
                    ["country"] = "SA"
                }
            },
            ["customer"] = new Dictionary<string, object>
            {
                ["registrationName"] = customerName,
                ["taxId"] = customerTaxId,
                ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
                ["address"] = new Dictionary<string, object>
                {
                    ["street"] = "Customer Street",
                    ["buildingNumber"] = "5678",
                    ["city"] = "Riyadh",
                    ["postalZone"] = "54321",
                    ["country"] = "SA"
                }
            },
            ["paymentMeans"] = new Dictionary<string, object> { ["code"] = "10" },
            ["delivery"] = new Dictionary<string, object>
            {
                ["actualDeliveryDate"] = DateTime.Now.ToString("yyyy-MM-dd")
            },
            ["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = taxAmount,
                ["subTotals"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["taxableAmount"] = subtotal,
                        ["taxAmount"] = taxAmount,
                        ["taxCategory"] = new Dictionary<string, object>
                        {
                            ["percent"] = 15,
                            ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                        }
                    }
                }
            },
            ["legalMonetaryTotal"] = new Dictionary<string, object>
            {
                ["lineExtensionAmount"] = subtotal,
                ["taxExclusiveAmount"] = subtotal,
                ["taxInclusiveAmount"] = total,
                ["payableAmount"] = total,
                ["allowanceTotalAmount"] = 0.00m
            },
            ["invoiceLines"] = items.Select((item, index) => new Dictionary<string, object>
            {
                ["id"] = (index + 1).ToString(),
                ["quantity"] = item.Quantity,
                ["unitCode"] = "PCE",
                ["lineExtensionAmount"] = item.Quantity * item.UnitPrice,
                ["item"] = new Dictionary<string, object>
                {
                    ["name"] = item.Name,
                    ["classifiedTaxCategory"] = new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            ["percent"] = 15,
                            ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                        }
                    }
                },
                ["price"] = new Dictionary<string, object>
                {
                    ["amount"] = item.UnitPrice,
                    ["unitCode"] = "PCE"
                },
                ["taxTotal"] = new Dictionary<string, object>
                {
                    ["taxAmount"] = item.Quantity * item.UnitPrice * 0.15m,
                    ["roundingAmount"] = item.Quantity * item.UnitPrice * 1.15m
                }
            }).ToList<object>()
        };
    }
}

// Supporting classes
public class InvoiceLineItem
{
    public string Name { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class InvoiceResult
{
    public bool Success { get; set; }
    public string Uuid { get; set; }
    public string InvoiceId { get; set; }
    public string SignedXml { get; set; }
    public string QrCode { get; set; }
    public string ClearanceStatus { get; set; }
    public string ReportingStatus { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

// Usage
var service = new ZatcaInvoiceService(
    ZatcaEnvironment.Sandbox,
    "production_cert.pem",
    "private.pem",
    "your-secret"
);

var items = new List<InvoiceLineItem>
{
    new() { Name = "Product A", Quantity = 2, UnitPrice = 50.00m },
    new() { Name = "Product B", Quantity = 1, UnitPrice = 100.00m }
};

var result = await service.CreateAndSubmitInvoiceAsync(
    "INV-2024-001",
    "Customer Corp",
    "399999999900003",
    items
);

if (result.Success)
{
    Console.WriteLine($"Invoice {result.InvoiceId} submitted successfully!");
    Console.WriteLine($"QR Code: {result.QrCode}");
}
```

---

## Invoice Types Reference

| Type Code | Name | Description |
|-----------|------|-------------|
| 388 | Tax Invoice | Standard invoice for goods/services |
| 381 | Credit Note | Refund or return adjustment |
| 383 | Debit Note | Additional charges adjustment |

## Invoice Categories

| Category | Description | ZATCA Process |
|----------|-------------|---------------|
| Standard | B2B invoices | Clearance (real-time approval) |
| Simplified | B2C invoices | Reporting (batch submission) |

## Error Handling

```csharp
try
{
    var result = await apiClient.ValidateInvoiceComplianceAsync(...);
}
catch (ZatcaApiException ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Response: {ex.ResponseBody}");
}
catch (ZatcaValidationException ex)
{
    Console.WriteLine($"Validation Error: {ex.Message}");
}
catch (CertificateBuilderException ex)
{
    Console.WriteLine($"Certificate Error: {ex.Message}");
}
```

---

## Documentation

For detailed documentation, see the [docs](docs/) folder:

- [Getting Started](docs/getting-started.md) - Complete setup guide
- [API Reference](docs/api-reference.md) - ZATCA API client documentation
- [Certificates](docs/certificates.md) - Certificate generation and management
- [Invoice Generation](docs/invoice-generation.md) - Creating invoices
- [Signing](docs/signing.md) - Digital signatures and QR codes
- [Validation](docs/validation.md) - Invoice validation
- [Models Reference](docs/models.md) - Data models documentation
- [Examples](docs/examples.md) - More code examples

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

This library is a .NET port inspired by the [php-zatca-xml](https://github.com/Saleh7/php-zatca-xml) library.
