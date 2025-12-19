---
layout: default
title: Signing
nav_order: 5
description: "Digital signature and QR code generation for ZATCA e-invoices"
---

# Signing

The signing module provides comprehensive functionality for digitally signing ZATCA e-invoices and generating QR codes. This is essential for Category E (digital signing) invoices that require cryptographic signatures.

## Table of Contents

- [Overview](#overview)
- [InvoiceSigner Class](#invoicesigner-class)
- [SignatureBuilder Class](#signaturebuilder-class)
- [QrCodeGenerator Class](#qrcodegenerator-class)
- [SignedInvoiceResult Class](#signedinvoiceresult-class)
- [The Signing Process](#the-signing-process)
- [Code Examples](#code-examples)

## Overview

The signing process involves several key steps:

1. **Hash Computation**: Computing SHA-256 hash of the canonicalized invoice XML
2. **Digital Signature**: Creating an ECDSA-SHA256 signature using the certificate's private key
3. **UBL Extension**: Building the XML signature structure with XAdES properties
4. **QR Code Generation**: Creating a TLV-encoded QR code with invoice data
5. **XML Assembly**: Inserting the signature and QR code into the original invoice

All signing operations use ECDSA (Elliptic Curve Digital Signature Algorithm) with SHA-256, as required by ZATCA specifications.

## InvoiceSigner Class

The `InvoiceSigner` class is the main orchestrator for signing ZATCA e-invoices. It implements the Category E digital signing requirements.

### Namespace

```csharp
using Zatca.EInvoice.Signing;
```

### Main Methods

#### Sign

Signs an invoice XML with the provided certificate.

```csharp
public static SignedInvoiceResult Sign(string xmlInvoice, X509Certificate2 certificate)
```

**Parameters:**
- `xmlInvoice` - The unsigned invoice XML string
- `certificate` - The X509Certificate2 with private key for signing

**Returns:** `SignedInvoiceResult` containing the signed XML, hash, digital signature, and QR code

**Exceptions:**
- `ArgumentNullException` - When required parameters are null or empty
- `ArgumentException` - When the certificate doesn't have a private key
- `InvalidOperationException` - When the certificate doesn't contain an ECDSA private key

**Example:**

```csharp
using System.Security.Cryptography.X509Certificates;
using Zatca.EInvoice.Signing;

// Load your certificate with private key
var certificate = new X509Certificate2("path/to/cert.pfx", "password");

// Load or generate your invoice XML
string invoiceXml = /* your invoice XML */;

// Sign the invoice
var result = InvoiceSigner.Sign(invoiceXml, certificate);

// Access the results
Console.WriteLine($"Hash: {result.Hash}");
Console.WriteLine($"QR Code: {result.QrCode}");
Console.WriteLine($"Signature: {result.DigitalSignature}");

// Save the signed XML
File.WriteAllText("signed_invoice.xml", result.SignedXml);
```

#### GetHash

Computes the SHA-256 hash of an invoice XML.

```csharp
public static string GetHash(string xmlInvoice)
```

This method is useful when you need to compute only the hash without signing the invoice.

**Parameters:**
- `xmlInvoice` - The invoice XML string

**Returns:** Base64-encoded SHA-256 hash (32 bytes)

**Example:**

```csharp
string hash = InvoiceSigner.GetHash(invoiceXml);
Console.WriteLine($"Invoice Hash: {hash}");
```

#### GetQrCode

Generates a QR code for an already-signed invoice.

```csharp
public static string GetQrCode(
    string signedXmlInvoice,
    X509Certificate2 certificate,
    string hash,
    string digitalSignature)
```

This method is useful when you need to regenerate the QR code separately.

**Parameters:**
- `signedXmlInvoice` - The signed invoice XML
- `certificate` - The certificate used for signing
- `hash` - The invoice hash
- `digitalSignature` - The digital signature

**Returns:** Base64-encoded QR code

## SignatureBuilder Class

The `SignatureBuilder` class builds UBL signature XML structures with XAdES signatures for ZATCA e-invoices.

### Namespace

```csharp
using Zatca.EInvoice.Signing;
```

### Usage Pattern

The `SignatureBuilder` uses a fluent builder pattern:

```csharp
var signatureBuilder = new SignatureBuilder()
    .SetCertificate(certificate)
    .SetInvoiceDigest(hash)
    .SetSignatureValue(digitalSignature);

var ublExtensionXml = signatureBuilder.BuildSignatureXml();
```

### Methods

#### SetCertificate

Sets the certificate to use for building the signature.

```csharp
public SignatureBuilder SetCertificate(X509Certificate2 certificate)
```

**Returns:** The current instance for method chaining

#### SetInvoiceDigest

Sets the invoice digest (hash).

```csharp
public SignatureBuilder SetInvoiceDigest(string invoiceDigest)
```

**Parameters:**
- `invoiceDigest` - The base64-encoded SHA-256 hash of the invoice

**Returns:** The current instance for method chaining

#### SetSignatureValue

Sets the signature value.

```csharp
public SignatureBuilder SetSignatureValue(string signatureValue)
```

**Parameters:**
- `signatureValue` - The base64-encoded signature value

**Returns:** The current instance for method chaining

#### BuildSignatureXml

Builds and returns the UBL signature XML as a formatted string.

```csharp
public string BuildSignatureXml()
```

**Returns:** The formatted UBL signature XML

**Exceptions:**
- `InvalidOperationException` - When required properties (certificate, digest, or signature value) are not set

### Signature Structure

The signature builder creates a complete UBL signature structure that includes:

1. **UBLExtension** - The outer wrapper
2. **UBLDocumentSignatures** - Contains the signature information
3. **ds:Signature** - The XML Digital Signature
   - **SignedInfo** - References and algorithms
     - Invoice reference with XPath transforms
     - Signed properties reference
   - **SignatureValue** - The actual signature bytes
   - **KeyInfo** - Contains the X509 certificate
   - **ds:Object** - Contains XAdES qualifying properties
     - **SignedProperties** - Signing time and certificate info

### XPath Transforms

The signature uses XPath transforms to exclude certain elements from the hash:

- `not(//ancestor-or-self::ext:UBLExtensions)` - Excludes UBL extensions
- `not(//ancestor-or-self::cac:Signature)` - Excludes signature element
- `not(//ancestor-or-self::cac:AdditionalDocumentReference[cbc:ID='QR'])` - Excludes QR code

### Algorithms

The signature builder uses the following cryptographic algorithms:

- **Canonicalization**: `http://www.w3.org/2006/12/xml-c14n11` (Canonical XML 1.1)
- **Signature Method**: `http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256` (ECDSA-SHA256)
- **Digest Method**: `http://www.w3.org/2001/04/xmlenc#sha256` (SHA-256)

## QrCodeGenerator Class

The `QrCodeGenerator` class generates QR codes for ZATCA e-invoices using TLV (Tag-Length-Value) encoding.

### Namespace

```csharp
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Tags;
```

### Creating a QR Code Generator

#### From Tag Array

```csharp
public static QrCodeGenerator CreateFromTags(params Tag[] tags)
```

#### From Tag List

```csharp
public static QrCodeGenerator CreateFromTags(IEnumerable<Tag> tags)
```

### Methods

#### EncodeTlv

Encodes the tags into a TLV (Tag-Length-Value) formatted byte array.

```csharp
public byte[] EncodeTlv()
```

**Returns:** TLV encoded byte array

#### EncodeBase64

Encodes the TLV data into a base64 string.

```csharp
public string EncodeBase64()
```

**Returns:** Base64 encoded TLV string suitable for QR code generation

### TLV Encoding Format

Each tag is encoded in the following format:

```
[Tag Number: 1 byte][Length: 1 byte][Value: N bytes]
```

For example:
- Tag 1 (Seller Name): `01 0C "Test Company"` (tag=1, length=12, value="Test Company")
- Tag 2 (VAT Number): `02 0F "300000000000003"` (tag=2, length=15)

### ZATCA QR Code Tags

The QR code typically contains the following tags (as per ZATCA specifications):

1. **Tag 1**: Seller Name
2. **Tag 2**: VAT Registration Number
3. **Tag 3**: Invoice Timestamp
4. **Tag 4**: Invoice Total (with VAT)
5. **Tag 5**: VAT Total
6. **Tag 6**: Invoice Hash (SHA-256)
7. **Tag 7**: Digital Signature
8. **Tag 8**: Public Key
9. **Tag 9**: Certificate Signature

### Example

```csharp
using Zatca.EInvoice.Tags;
using Zatca.EInvoice.Signing;

// Create tags for the QR code
var tags = new Tag[]
{
    new SellerNameTag("Test Company"),
    new VatRegistrationNumberTag("300000000000003"),
    new InvoiceTimestampTag("2024-01-01T12:00:00Z"),
    new InvoiceTotalTag("115.00"),
    new VatTotalTag("15.00"),
    new InvoiceHashTag(hash),
    new DigitalSignatureTag(digitalSignature),
    new PublicKeyTag(publicKeyBytes),
    new CertificateSignatureTag(certSignatureBytes)
};

// Create QR code generator
var qrGenerator = QrCodeGenerator.CreateFromTags(tags);

// Get base64-encoded QR code
string qrCode = qrGenerator.EncodeBase64();

// Or get raw TLV bytes
byte[] tlvBytes = qrGenerator.EncodeTlv();
```

## SignedInvoiceResult Class

The `SignedInvoiceResult` class represents the result of the invoice signing operation.

### Namespace

```csharp
using Zatca.EInvoice.Signing;
```

### Properties

#### SignedXml

The signed invoice XML as a string.

```csharp
public string SignedXml { get; set; }
```

This property contains the complete invoice XML with the digital signature and QR code embedded.

#### Hash

The invoice hash (SHA-256, base64 encoded).

```csharp
public string Hash { get; set; }
```

A 32-byte SHA-256 hash encoded as base64 string. This hash is computed from the canonicalized invoice XML with certain elements excluded (UBL extensions, signature, and QR code).

#### QrCode

The QR code (TLV encoded, base64 encoded).

```csharp
public string QrCode { get; set; }
```

The base64-encoded TLV data that can be used to generate a QR code image for display on the invoice.

#### DigitalSignature

The digital signature (base64 encoded).

```csharp
public string DigitalSignature { get; set; }
```

The ECDSA-SHA256 signature of the invoice hash, encoded as base64 string.

### Example Usage

```csharp
var result = InvoiceSigner.Sign(invoiceXml, certificate);

// Access individual components
string signedXml = result.SignedXml;
string hash = result.Hash;
string qrCode = result.QrCode;
string signature = result.DigitalSignature;

// Save the signed XML
File.WriteAllText("signed_invoice.xml", result.SignedXml);

// Use the QR code (you would typically render this as a QR code image)
Console.WriteLine($"QR Code Data: {result.QrCode}");

// Store the hash for later verification
Console.WriteLine($"Invoice Hash: {result.Hash}");
```

## The Signing Process

The `InvoiceSigner.Sign()` method performs the following steps:

### Step 1: Parse the Invoice XML

The invoice XML is parsed into a document structure for manipulation.

### Step 2: Remove Elements for Hashing

Elements that should not be included in the hash are removed:
- UBL Extensions
- Signature elements
- QR Code reference

### Step 3: Compute the Invoice Hash

The canonicalized XML (without the excluded elements) is hashed using SHA-256.

```csharp
var hash = invoiceExtension.ComputeHash();
var hashBytes = Convert.FromBase64String(hash);
```

### Step 4: Create the Digital Signature

The hash is signed using the certificate's ECDSA private key.

```csharp
using var ecdsa = certificate.GetECDsaPrivateKey();
var signatureBytes = ecdsa.SignData(hashBytes, HashAlgorithmName.SHA256);
var digitalSignature = Convert.ToBase64String(signatureBytes);
```

### Step 5: Extract Certificate Information

The public key and certificate signature are extracted for inclusion in the QR code.

```csharp
var publicKeyBytes = ExtractPublicKey(certificate);
var certificateSignatureBytes = ExtractCertificateSignature(certificate);
```

### Step 6: Build the UBL Signature XML

The signature builder creates the complete UBL signature structure.

```csharp
var signatureBuilder = new SignatureBuilder()
    .SetCertificate(certificate)
    .SetInvoiceDigest(hash)
    .SetSignatureValue(digitalSignature);

var ublExtensionXml = signatureBuilder.BuildSignatureXml();
```

### Step 7: Generate the QR Code

The QR code is generated with all required tags.

```csharp
var qrTags = invoiceExtension.GenerateQrTags(
    certificate,
    hash,
    digitalSignature,
    publicKeyBytes,
    certificateSignatureBytes
);

var qrCodeGenerator = QrCodeGenerator.CreateFromTags(qrTags);
var qrCode = qrCodeGenerator.EncodeBase64();
```

### Step 8: Insert Signature and QR Code

The UBL extension and QR code are inserted into the original XML at the appropriate locations.

### Step 9: Clean Up and Return

Extra blank lines are removed, and the complete signed invoice is returned as a `SignedInvoiceResult`.

## Code Examples

### Complete Signing Example

```csharp
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Zatca.EInvoice.Signing;

class Program
{
    static void Main()
    {
        // Load certificate with private key
        var certificate = new X509Certificate2(
            "path/to/certificate.pfx",
            "password",
            X509KeyStorageFlags.Exportable);

        // Verify certificate has private key
        if (!certificate.HasPrivateKey)
        {
            throw new Exception("Certificate must have a private key");
        }

        // Load the unsigned invoice XML
        string invoiceXml = File.ReadAllText("unsigned_invoice.xml");

        try
        {
            // Sign the invoice
            var result = InvoiceSigner.Sign(invoiceXml, certificate);

            // Save the signed invoice
            File.WriteAllText("signed_invoice.xml", result.SignedXml);

            // Display results
            Console.WriteLine("Invoice signed successfully!");
            Console.WriteLine($"Hash: {result.Hash}");
            Console.WriteLine($"QR Code: {result.QrCode}");
            Console.WriteLine($"Digital Signature: {result.DigitalSignature}");

            // Optionally, you can generate a QR code image from result.QrCode
            // using a QR code library
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error signing invoice: {ex.Message}");
        }
        finally
        {
            certificate.Dispose();
        }
    }
}
```

### Hash-Only Example

```csharp
using System;
using System.IO;
using Zatca.EInvoice.Signing;

// Compute hash without signing
string invoiceXml = File.ReadAllText("invoice.xml");
string hash = InvoiceSigner.GetHash(invoiceXml);

Console.WriteLine($"Invoice Hash: {hash}");

// Verify hash is correct length (32 bytes for SHA-256)
byte[] hashBytes = Convert.FromBase64String(hash);
Console.WriteLine($"Hash length: {hashBytes.Length} bytes");
```

### Regenerate QR Code Example

```csharp
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Zatca.EInvoice.Signing;

// Load signed invoice
string signedXml = File.ReadAllText("signed_invoice.xml");

// Load certificate
var certificate = new X509Certificate2("path/to/cert.pfx", "password");

// You need the original hash and signature
string hash = "..."; // from original signing
string digitalSignature = "..."; // from original signing

// Regenerate QR code
string qrCode = InvoiceSigner.GetQrCode(
    signedXml,
    certificate,
    hash,
    digitalSignature);

Console.WriteLine($"Regenerated QR Code: {qrCode}");
```

### Batch Signing Example

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Zatca.EInvoice.Signing;

// Load certificate once
using var certificate = new X509Certificate2("cert.pfx", "password");

// Sign multiple invoices
var invoiceFiles = Directory.GetFiles("unsigned_invoices", "*.xml");
var results = new List<SignedInvoiceResult>();

foreach (var file in invoiceFiles)
{
    Console.WriteLine($"Signing {Path.GetFileName(file)}...");

    string invoiceXml = File.ReadAllText(file);
    var result = InvoiceSigner.Sign(invoiceXml, certificate);

    // Save signed invoice
    string outputFile = Path.Combine(
        "signed_invoices",
        Path.GetFileName(file));
    File.WriteAllText(outputFile, result.SignedXml);

    results.Add(result);
    Console.WriteLine($"  Hash: {result.Hash}");
}

Console.WriteLine($"Signed {results.Count} invoices successfully");
```

### Error Handling Example

```csharp
using System;
using System.Security.Cryptography.X509Certificates;
using Zatca.EInvoice.Signing;

try
{
    var certificate = new X509Certificate2("cert.pfx", "password");
    string invoiceXml = /* ... */;

    var result = InvoiceSigner.Sign(invoiceXml, certificate);

    // Success
    Console.WriteLine("Invoice signed successfully");
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Missing required parameter: {ex.ParamName}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid argument: {ex.Message}");
    // Likely: certificate without private key
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Operation failed: {ex.Message}");
    // Likely: certificate doesn't contain ECDSA key
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Best Practices

1. **Certificate Management**: Always ensure your certificate has a private key and is properly secured
2. **Dispose Certificates**: Use `using` statements or call `Dispose()` on X509Certificate2 instances
3. **Validate Input**: Ensure invoice XML is well-formed before signing
4. **Store Results**: Keep the hash and digital signature for later verification
5. **Error Handling**: Implement proper error handling for signing operations
6. **Performance**: Reuse the same certificate instance when signing multiple invoices
7. **Security**: Never expose private keys; keep certificate files secure
8. **Hash Verification**: The hash should always be 32 bytes (SHA-256)
9. **QR Code Display**: Use a QR code library to render the base64 QR code data as an image
10. **Thread Safety**: The signing methods are thread-safe and can be used concurrently

## See Also

- [Certificates](certificates.md) - Certificate generation and management
- [Invoice Generation](invoice-generation.md) - Creating invoices
- [API Reference](api-reference.md) - Complete API documentation
- [Getting Started](getting-started.md) - Quick start guide
