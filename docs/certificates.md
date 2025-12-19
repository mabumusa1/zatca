---
layout: default
title: Certificates
nav_order: 3
description: "Certificate generation and management for ZATCA e-invoicing"
---

# Certificate Management

This guide covers generating Certificate Signing Requests (CSR), managing compliance certificates, and working with production certificates.

## Overview

ZATCA e-invoicing requires digital certificates for:
1. **Signing invoices** - Ensuring authenticity and integrity
2. **API authentication** - Authenticating with ZATCA servers
3. **Compliance validation** - Proving device/solution registration

## Certificate Flow

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Generate CSR   │────▶│  Get Compliance │────▶│  Get Production │
│  & Private Key  │     │   Certificate   │     │   Certificate   │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

---

## CertificateBuilder

The `CertificateBuilder` class generates CSRs with all required ZATCA fields.

### Basic Usage

```csharp
using Zatca.EInvoice.Certificates;

var builder = new CertificateBuilder()
    .SetOrganizationIdentifier("312345678901233")
    .SetSerialNumber("MySolution", "1.0", "DEVICE001")
    .SetCommonName("My Company")
    .SetCountryName("SA")
    .SetOrganizationName("My Company Ltd")
    .SetOrganizationalUnitName("IT Department")
    .SetAddress("Riyadh, Main Street 123")
    .SetInvoiceType("1100")
    .SetProduction(false)
    .SetBusinessCategory("Technology");

// Get the CSR
string csr = builder.GetCsr();

// Get the private key (keep this secure!)
string privateKey = builder.GetPrivateKey();
```

### Builder Methods

#### SetOrganizationIdentifier

```csharp
public CertificateBuilder SetOrganizationIdentifier(string identifier)
```

Sets the VAT registration number. Must be exactly 15 digits, starting and ending with `3`.

**Example:**
```csharp
.SetOrganizationIdentifier("312345678901233")
```

**Validation Rules:**
- Must be exactly 15 characters
- Must contain only digits
- Must start with `3`
- Must end with `3`

#### SetSerialNumber

```csharp
public CertificateBuilder SetSerialNumber(
    string solutionName,
    string model,
    string serialNumber)
```

Sets the device/solution serial number.

**Parameters:**
- `solutionName` - Your solution/application name
- `model` - Version or model identifier
- `serialNumber` - Unique device identifier

**Example:**
```csharp
.SetSerialNumber("MyERP", "2.0", "POS-001")
```

#### SetCommonName

```csharp
public CertificateBuilder SetCommonName(string commonName)
```

Sets the common name for the certificate.

#### SetCountryName

```csharp
public CertificateBuilder SetCountryName(string country)
```

Sets the country code. Must be exactly 2 characters.

**Example:**
```csharp
.SetCountryName("SA")
```

#### SetOrganizationName

```csharp
public CertificateBuilder SetOrganizationName(string organizationName)
```

Sets the organization name as registered.

#### SetOrganizationalUnitName

```csharp
public CertificateBuilder SetOrganizationalUnitName(string unitName)
```

Sets the organizational unit (department).

#### SetAddress

```csharp
public CertificateBuilder SetAddress(string address)
```

Sets the registered business address.

#### SetInvoiceType

```csharp
public CertificateBuilder SetInvoiceType(string invoiceType)
```

Sets the invoice types this certificate will be used for.

**Format:** 4-digit string where each digit is `0` or `1`

| Position | Meaning |
|----------|---------|
| 1st | Standard invoices (B2B) |
| 2nd | Simplified invoices (B2C) |
| 3rd | Reserved |
| 4th | Reserved |

**Examples:**
```csharp
.SetInvoiceType("1000")  // Standard only
.SetInvoiceType("0100")  // Simplified only
.SetInvoiceType("1100")  // Both types
```

#### SetProduction

```csharp
public CertificateBuilder SetProduction(bool isProduction)
```

Sets whether this is for production or sandbox.

```csharp
.SetProduction(false)  // Sandbox/testing
.SetProduction(true)   // Production
```

#### SetBusinessCategory

```csharp
public CertificateBuilder SetBusinessCategory(string category)
```

Sets the business category.

---

## CertificateInfo

The `CertificateInfo` class manages loaded certificates for signing operations.

### Constructor

```csharp
public CertificateInfo(
    string certificateContent,
    string privateKeyContent,
    string? secret = null)
```

**Parameters:**
- `certificateContent` - The certificate (base64 or PEM format)
- `privateKeyContent` - The private key (base64 or PEM format)
- `secret` - Optional API secret from ZATCA

### Example

```csharp
using Zatca.EInvoice.Certificates;

// Load from base64 (as returned by ZATCA)
var certificate = new CertificateInfo(
    "MIICAzCCAaqgAwIBAgIGAZT7anBc...",  // Certificate
    "MIGEAgEAMBAGByqGSM49AgEGBSuB...",  // Private key
    "secret-from-zatca"
);

// Or load from PEM files
string certPem = File.ReadAllText("certificate.pem");
string keyPem = File.ReadAllText("private.pem");

var certificate = new CertificateInfo(certPem, keyPem, secret);
```

### Properties

```csharp
// Get the raw certificate content
string rawCert = certificate.RawCertificate;

// Get the secret
string? secret = certificate.Secret;

// Get the X509 certificate object
X509Certificate2 x509 = certificate.Certificate;
```

### Methods

#### GetCertificateHash

```csharp
public string GetCertificateHash()
```

Returns the SHA-256 hash of the certificate (base64 encoded).

```csharp
string hash = certificate.GetCertificateHash();
// Returns: "a1b2c3d4e5f6..."
```

#### GetFormattedIssuer

```csharp
public string GetFormattedIssuer()
```

Returns the certificate issuer in formatted string.

#### GetRawPublicKeyBase64

```csharp
public string GetRawPublicKeyBase64()
```

Returns the public key in base64 format (without PEM headers).

#### GetCertificateSignature

```csharp
public string GetCertificateSignature()
```

Returns the certificate's digital signature.

#### GetAuthorizationHeader

```csharp
public string GetAuthorizationHeader()
```

Returns the Basic authentication header for API calls.

```csharp
string authHeader = certificate.GetAuthorizationHeader();
// Returns: "Basic dXNlcjpwYXNzd29yZA=="
```

---

## Storage

The `Storage` class helps with file operations for certificates.

```csharp
using Zatca.EInvoice.Helpers;

var storage = new Storage();

// Save content to file
storage.Put("path/to/file.pem", content);

// Read content from file
string content = storage.Get("path/to/file.pem");

// Check if file exists
bool exists = storage.Exists("path/to/file.pem");

// Delete file
storage.Delete("path/to/file.pem");
```

---

## Complete Certificate Workflow

### Step 1: Generate CSR

```csharp
var builder = new CertificateBuilder()
    .SetOrganizationIdentifier("312345678901233")
    .SetSerialNumber("MyApp", "1.0", "DEVICE001")
    .SetCommonName("My Company Ltd")
    .SetCountryName("SA")
    .SetOrganizationName("My Company Ltd")
    .SetOrganizationalUnitName("Sales")
    .SetAddress("Riyadh, King Fahd Road")
    .SetInvoiceType("1100")
    .SetProduction(false)
    .SetBusinessCategory("Retail");

string csr = builder.GetCsr();
string privateKey = builder.GetPrivateKey();

// Store securely
File.WriteAllText("csr.pem", csr);
File.WriteAllText("private.pem", privateKey);
```

### Step 2: Get Compliance Certificate

```csharp
var apiClient = new ZatcaApiClient(ZatcaEnvironment.Sandbox);

// OTP from ZATCA portal
string otp = "123456";

var result = await apiClient.RequestComplianceCertificateAsync(csr, otp);

// Store certificate data
var certData = new
{
    Certificate = result.BinarySecurityToken,
    Secret = result.Secret,
    RequestId = result.RequestId
};

File.WriteAllText("compliance_cert.json",
    JsonSerializer.Serialize(certData));
```

### Step 3: Validate Compliance

```csharp
// Create test invoices and submit for compliance check
var certificate = new CertificateInfo(
    result.BinarySecurityToken,
    privateKey,
    result.Secret
);

// Sign and submit test invoices...
// Once all tests pass, proceed to production
```

### Step 4: Get Production Certificate

```csharp
var prodResult = await apiClient.RequestProductionCertificateAsync(
    result.RequestId,
    result.BinarySecurityToken,
    result.Secret
);

// Store production certificate
var prodCertData = new
{
    Certificate = prodResult.BinarySecurityToken,
    Secret = prodResult.Secret
};

File.WriteAllText("production_cert.json",
    JsonSerializer.Serialize(prodCertData));
```

---

## Certificate Renewal

Production certificates (PCSID) have expiration dates and must be renewed before they expire. ZATCA provides a renewal endpoint that allows you to renew your certificate without going through the full compliance process again.

### When to Renew

- Renew certificates before they expire (recommended: at least 30 days before expiration)
- You'll need a new OTP from the ZATCA portal for renewal
- The renewal process generates a new certificate and secret

### Renewal Process

```csharp
using Zatca.EInvoice.Api;
using Zatca.EInvoice.Certificates;

// Step 1: Generate a new CSR (must use the same organization details)
var builder = new CertificateBuilder()
    .SetOrganizationIdentifier("312345678901233")
    .SetSerialNumber("MyApp", "1.0", "DEVICE001")
    .SetCommonName("My Company Ltd")
    .SetCountryName("SA")
    .SetOrganizationName("My Company Ltd")
    .SetOrganizationalUnitName("IT")
    .SetAddress("Riyadh, King Fahd Road")
    .SetInvoiceType("1100")
    .SetProduction(true)  // Must be true for production renewal
    .SetBusinessCategory("Retail");

string newCsr = builder.GetCsr();
string newPrivateKey = builder.GetPrivateKey();

// Step 2: Load current production certificate credentials
string currentCertificate = File.ReadAllText("production_cert.pem");
string currentSecret = "current-secret-from-zatca";

// Step 3: Request renewal from ZATCA
var apiClient = new ZatcaApiClient(ZatcaEnvironment.Production);

string otp = "123456";  // New OTP from ZATCA portal

var renewalResult = await apiClient.RenewProductionCertificateAsync(
    otp,
    newCsr,
    currentCertificate,
    currentSecret
);

if (renewalResult.IsSuccess)
{
    // Step 4: Save the new certificate and secret
    File.WriteAllText("production_cert_new.pem", renewalResult.BinarySecurityToken);

    var newCertData = new
    {
        Certificate = renewalResult.BinarySecurityToken,
        Secret = renewalResult.Secret,
        PrivateKey = newPrivateKey  // Store securely!
    };

    Console.WriteLine("Certificate renewed successfully!");
    Console.WriteLine($"New Request ID: {renewalResult.RequestId}");
}
else
{
    Console.WriteLine("Renewal failed:");
    foreach (var error in renewalResult.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
}
```

### Renewal Best Practices

1. **Schedule Renewals**: Set up automated reminders to renew certificates before expiration
2. **Test First**: If possible, test the renewal process in sandbox/simulation before production
3. **Backup Old Credentials**: Keep the old certificate and secret until the new ones are verified working
4. **Update Immediately**: Once renewed, update your application to use the new credentials
5. **Secure Storage**: Store the new private key securely (never commit to source control)

---

## Security Best Practices

### 1. Protect Private Keys

```csharp
// Never log or expose private keys
// Store in secure location with restricted access
// Consider using Azure Key Vault, AWS Secrets Manager, etc.
```

### 2. Rotate Certificates

Certificates have expiration dates. Use the `RenewProductionCertificateAsync` method to renew before expiry. See the [Certificate Renewal](#certificate-renewal) section above for details.

### 3. Secure Storage

```csharp
// Use environment variables for secrets
string secret = Environment.GetEnvironmentVariable("ZATCA_SECRET");

// Or use secure configuration
var certificate = configuration.GetSection("Zatca:Certificate").Value;
```

### 4. Backup

Always maintain secure backups of:
- Private keys
- Certificate data
- API secrets

---

## Troubleshooting

### Invalid Organization Identifier

```
Error: Organization Identifier must be 15 digits starting and ending with 3
```

**Solution:** Ensure identifier is exactly 15 digits with format `3XXXXXXXXXXXXX3`

### CSR Generation Failed

```
Error: Failed to generate CSR
```

**Solution:** Check all required fields are set and valid.

### Certificate Loading Failed

```
Error: Failed to load certificate from PEM content
```

**Solution:** Ensure certificate is in correct format (base64 or PEM).
