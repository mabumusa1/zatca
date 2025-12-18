---
layout: default
title: Zatca.EInvoice
---

# Zatca.EInvoice

A .NET library for generating **ZATCA-compliant** e-invoices in Saudi Arabia. This library simplifies the process of creating compliant e-invoices, generating QR codes, handling certificates, and submitting invoices to ZATCA's servers.

## Features

- **ZATCA-Compliant** - Generate valid e-invoices that meet ZATCA regulations
- **Invoice Creation** - Generate standard and simplified invoices in XML format
- **Digital Signing** - Sign invoices securely to ensure compliance
- **QR Code Generation** - Automatically generate QR codes for invoices
- **Direct Submission to ZATCA** - Send invoices directly to ZATCA's servers
- **Certificate Management** - Generate CSRs and manage compliance certificates
- **Validation** - Validate invoices before submission

## Requirements

- **.NET 8.0** or higher
- **BouncyCastle.Cryptography** for cryptographic operations

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

### 1. Generate a Certificate Signing Request (CSR)

```csharp
using Zatca.EInvoice.Certificates;

var builder = new CertificateBuilder()
    .SetOrganizationIdentifier("312345678901233")
    .SetSerialNumber("Saleh", "1n", "SME00023")
    .SetCommonName("My Organization")
    .SetCountryName("SA")
    .SetOrganizationName("My Company")
    .SetOrganizationalUnitName("IT Department")
    .SetAddress("Riyadh 1234 Street")
    .SetInvoiceType("1100")
    .SetProduction(false)
    .SetBusinessCategory("Technology");

var csr = builder.GetCsr();
var privateKey = builder.GetPrivateKey();
```

### 2. Request Compliance Certificate from ZATCA

```csharp
using Zatca.EInvoice.Api;

var apiClient = new ZatcaApiClient(ZatcaEnvironment.Sandbox);

var result = await apiClient.RequestComplianceCertificateAsync(csr, "123456"); // OTP from ZATCA

Console.WriteLine($"Certificate: {result.BinarySecurityToken}");
Console.WriteLine($"Secret: {result.Secret}");
```

### 3. Generate an Invoice

```csharp
using Zatca.EInvoice.Xml;
using Zatca.EInvoice.Mappers;

var invoiceData = new Dictionary<string, object>
{
    ["uuid"] = Guid.NewGuid().ToString(),
    ["id"] = "INV-001",
    ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
    ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
    ["currencyCode"] = "SAR",
    ["invoiceType"] = new Dictionary<string, object>
    {
        ["invoice"] = "simplified",
        ["type"] = "388"
    },
    ["supplier"] = new Dictionary<string, object>
    {
        ["registrationName"] = "My Company",
        ["taxId"] = "312345678901233",
        ["address"] = new Dictionary<string, object>
        {
            ["street"] = "Main Street",
            ["buildingNumber"] = "1234",
            ["city"] = "Riyadh",
            ["postalZone"] = "12345",
            ["country"] = "SA"
        }
    },
    // ... more invoice data
};

var generator = new InvoiceGenerator();
var xml = generator.Generate(invoiceData);
```

### 4. Sign the Invoice

```csharp
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Certificates;

var certificate = new CertificateInfo(
    certificateContent,
    privateKeyContent,
    secret
);

var signedResult = InvoiceSigner.Sign(xmlInvoice, certificate);

Console.WriteLine($"Signed XML: {signedResult.SignedXml}");
Console.WriteLine($"Invoice Hash: {signedResult.InvoiceHash}");
Console.WriteLine($"QR Code: {signedResult.QrCode}");
```

### 5. Submit to ZATCA

```csharp
var submissionResult = await apiClient.ValidateInvoiceComplianceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceUuid,
    certificate.RawCertificate,
    certificate.Secret
);

Console.WriteLine($"Status: {submissionResult.ClearanceStatus}");
```

## Documentation

- [Getting Started](getting-started.md) - Complete setup guide
- [API Reference](api-reference.md) - ZATCA API client documentation
- [Certificates](certificates.md) - Certificate generation and management
- [Invoice Generation](invoice-generation.md) - Creating invoices
- [Signing](signing.md) - Digital signature and QR codes
- [Validation](validation.md) - Invoice validation
- [Models Reference](models.md) - Data models documentation

## Invoice Types

| Type | Code | Description |
|------|------|-------------|
| Tax Invoice | 388 | Standard tax invoice |
| Credit Note | 381 | Credit note for returns/refunds |
| Debit Note | 383 | Debit note for additional charges |

## Invoice Categories

| Category | Description |
|----------|-------------|
| Standard | B2B invoices requiring clearance |
| Simplified | B2C invoices for immediate reporting |

## Project Structure

```
Zatca.EInvoice/
├── Api/                    # ZATCA API client
├── Certificates/           # Certificate generation
├── Exceptions/             # Custom exceptions
├── Helpers/                # Utility classes
├── Mappers/                # Data mappers
├── Models/                 # Data models
├── Signing/                # Invoice signing
├── Tags/                   # QR code tags
├── Validation/             # Invoice validation
└── Xml/                    # XML generation
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the [MIT License](LICENSE).

## Acknowledgments

This library is a .NET port of the [php-zatca-xml](https://github.com/Saleh7/php-zatca-xml) library by Saleh7.
