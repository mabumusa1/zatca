---
layout: default
title: API Reference
nav_order: 7
description: "ZATCA API client documentation"
---

# API Reference

The `ZatcaApiClient` class provides methods for interacting with ZATCA's e-invoicing API.

## ZatcaApiClient

### Constructor

```csharp
public ZatcaApiClient(ZatcaEnvironment environment, HttpClient? httpClient = null)
```

**Parameters:**
- `environment` - The ZATCA environment (Sandbox, Simulation, or Production)
- `httpClient` - Optional custom HttpClient for testing

**Example:**
```csharp
// Using sandbox environment
var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox);

// Using production environment
var client = new ZatcaApiClient(ZatcaEnvironment.Production);

// With custom HttpClient
var httpClient = new HttpClient();
var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
```

### SetWarningHandling

Configures how the client handles responses with warnings.

```csharp
public void SetWarningHandling(bool allow)
```

**Parameters:**
- `allow` - When `true`, responses with status code 202 (containing warnings) are treated as successful

**Example:**
```csharp
var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox);

// Allow responses with warnings to be treated as successful
client.SetWarningHandling(true);

// Now 202 responses won't throw exceptions
var result = await client.ValidateInvoiceComplianceAsync(...);
if (result.HasWarnings)
{
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"Warning: {warning.Message}");
    }
}
```

## Environments

```csharp
public enum ZatcaEnvironment
{
    Sandbox,      // For development and testing
    Simulation,   // For simulation testing
    Production    // For live invoices
}
```

### Environment URLs

| Environment | Base URL |
|-------------|----------|
| Sandbox | `https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/` |
| Simulation | `https://gw-fatoora.zatca.gov.sa/e-invoicing/simulation/` |
| Production | `https://gw-fatoora.zatca.gov.sa/e-invoicing/core/` |

---

## Certificate Methods

### RequestComplianceCertificateAsync

Requests a compliance certificate from ZATCA using a CSR.

```csharp
public async Task<ComplianceCertificateResult> RequestComplianceCertificateAsync(
    string csr,
    string otp,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `csr` - The Certificate Signing Request (base64 or PEM format)
- `otp` - One-Time Password from ZATCA portal
- `cancellationToken` - Optional cancellation token

**Returns:** `ComplianceCertificateResult`

**Example:**
```csharp
var result = await client.RequestComplianceCertificateAsync(
    csrContent,
    "123456"
);

Console.WriteLine($"Certificate: {result.BinarySecurityToken}");
Console.WriteLine($"Secret: {result.Secret}");
Console.WriteLine($"Request ID: {result.RequestId}");
```

### RequestProductionCertificateAsync

Requests a production certificate after compliance validation.

```csharp
public async Task<ProductionCertificateResult> RequestProductionCertificateAsync(
    string complianceRequestId,
    string certificate,
    string secret,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `complianceRequestId` - The request ID from compliance certificate
- `certificate` - The compliance certificate
- `secret` - The compliance certificate secret

**Example:**
```csharp
var result = await client.RequestProductionCertificateAsync(
    complianceResult.RequestId,
    complianceResult.BinarySecurityToken,
    complianceResult.Secret
);
```

---

## Invoice Submission Methods

### ValidateInvoiceComplianceAsync

Validates an invoice against ZATCA compliance rules.

```csharp
public async Task<InvoiceSubmissionResult> ValidateInvoiceComplianceAsync(
    string signedXmlInvoice,
    string invoiceHash,
    string uuid,
    string certificate,
    string secret,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `signedXmlInvoice` - The signed invoice XML
- `invoiceHash` - The invoice hash (base64)
- `uuid` - The unique invoice identifier
- `certificate` - The compliance/production certificate
- `secret` - The certificate secret

**Example:**
```csharp
var result = await client.ValidateInvoiceComplianceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceUuid,
    certificate.RawCertificate,
    certificate.Secret
);

if (result.ClearanceStatus == "CLEARED")
{
    Console.WriteLine("Compliance check passed!");
}
```

### SubmitReportingInvoiceAsync

Reports a simplified invoice to ZATCA (B2C transactions).

```csharp
public async Task<InvoiceSubmissionResult> SubmitReportingInvoiceAsync(
    string signedXmlInvoice,
    string invoiceHash,
    string uuid,
    string certificate,
    string secret,
    CancellationToken cancellationToken = default)
```

**Example:**
```csharp
var result = await client.SubmitReportingInvoiceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceUuid,
    certificate.RawCertificate,
    certificate.Secret
);

Console.WriteLine($"Reporting Status: {result.ReportingStatus}");
```

### SubmitClearanceInvoiceAsync

Submits a standard invoice for clearance (B2B transactions).

```csharp
public async Task<InvoiceSubmissionResult> SubmitClearanceInvoiceAsync(
    string signedXmlInvoice,
    string invoiceHash,
    string uuid,
    string certificate,
    string secret,
    CancellationToken cancellationToken = default)
```

**Example:**
```csharp
var result = await client.SubmitClearanceInvoiceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceUuid,
    certificate.RawCertificate,
    certificate.Secret
);

if (result.ClearanceStatus == "CLEARED")
{
    // Invoice was cleared, get the cleared invoice
    string clearedXml = result.ClearedInvoice;
}
```

### RenewProductionCertificateAsync

Renews a production certificate (PCSID) before expiration.

```csharp
public async Task<ProductionCertificateResult> RenewProductionCertificateAsync(
    string otp,
    string csr,
    string certificate,
    string secret,
    CancellationToken cancellationToken = default)
```

**Parameters:**
- `otp` - One-Time Password from ZATCA portal
- `csr` - Certificate Signing Request (CSR) in PEM format
- `certificate` - Current production certificate
- `secret` - Current production secret

**Example:**
```csharp
// Generate a new CSR for renewal
var builder = new CertificateBuilder()
    .SetOrganizationIdentifier("312345678901233")
    .SetSerialNumber("MyApp", "1.0", "DEVICE001")
    // ... other settings
    .SetProduction(true);

string newCsr = builder.GetCsr();
string newPrivateKey = builder.GetPrivateKey();

// Renew the certificate
var result = await client.RenewProductionCertificateAsync(
    "123456",  // OTP from ZATCA
    newCsr,
    currentCertificate,
    currentSecret
);

if (result.IsSuccess)
{
    // Save the new certificate and secret
    Console.WriteLine($"New Certificate: {result.BinarySecurityToken}");
    Console.WriteLine($"New Secret: {result.Secret}");
}
```

---

## Result Classes

### ComplianceCertificateResult

```csharp
public sealed class ComplianceCertificateResult
{
    public string RequestId { get; }
    public string DispositionMessage { get; }
    public string BinarySecurityToken { get; }
    public string Secret { get; }
    public List<string> Errors { get; }
    public List<string> Warnings { get; }

    // Helper properties
    public bool IsSuccess => Errors.Count == 0;
    public bool HasWarnings => Warnings.Count > 0;
}
```

### ProductionCertificateResult

```csharp
public sealed class ProductionCertificateResult
{
    public string RequestId { get; }
    public string DispositionMessage { get; }
    public string BinarySecurityToken { get; }
    public string Secret { get; }
    public List<string> Errors { get; }
    public List<string> Warnings { get; }

    // Helper properties
    public bool IsSuccess => Errors.Count == 0;
    public bool HasWarnings => Warnings.Count > 0;
}
```

### InvoiceSubmissionResult

```csharp
public class InvoiceSubmissionResult
{
    public string Status { get; }
    public string ClearanceStatus { get; }
    public string ReportingStatus { get; }
    public string ClearedInvoice { get; }
    public List<ValidationMessage> Errors { get; }
    public List<ValidationMessage> Warnings { get; }
    public List<ValidationMessage> InfoMessages { get; }

    // Helper properties
    public bool IsSuccess => Errors.Count == 0;
    public bool HasWarnings => Warnings.Count > 0;
    public bool IsClearance => !string.IsNullOrEmpty(ClearanceStatus);
    public bool IsReporting => !string.IsNullOrEmpty(ReportingStatus);
}
```

### ValidationMessage

```csharp
public class ValidationMessage
{
    public string Type { get; set; }
    public string Code { get; set; }
    public string Category { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
}
```

**Example:**
```csharp
var result = await client.ValidateInvoiceComplianceAsync(...);

if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"[{error.Code}] {error.Message}");
    }
}

if (result.HasWarnings)
{
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"Warning: {warning.Message}");
    }
}
```

---

## Error Handling

The API client throws `ZatcaApiException` for API-related errors:

```csharp
try
{
    var result = await client.RequestComplianceCertificateAsync(csr, otp);
}
catch (ZatcaApiException ex)
{
    Console.WriteLine($"API Error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Response: {ex.ResponseBody}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network Error: {ex.Message}");
}
```

### Common Error Codes

| Status Code | Description |
|-------------|-------------|
| 400 | Bad Request - Invalid input data |
| 401 | Unauthorized - Invalid credentials |
| 403 | Forbidden - Access denied |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error |

---

## Best Practices

### 1. Reuse HttpClient

```csharp
// Create a single HttpClient instance for the application lifetime
private static readonly HttpClient _httpClient = new HttpClient();

public async Task SubmitInvoice()
{
    var client = new ZatcaApiClient(ZatcaEnvironment.Production, _httpClient);
    // Use client...
}
```

### 2. Implement Retry Logic

```csharp
using Polly;

var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<ZatcaApiException>(ex => ex.StatusCode >= 500)
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

await retryPolicy.ExecuteAsync(async () =>
{
    return await client.ValidateInvoiceComplianceAsync(...);
});
```

### 3. Use Cancellation Tokens

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var result = await client.ValidateInvoiceComplianceAsync(
        signedXml, hash, uuid, cert, secret,
        cts.Token
    );
}
catch (OperationCanceledException)
{
    Console.WriteLine("Request timed out");
}
```

### 4. Dispose Resources

```csharp
using var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox);
// Client will be disposed after use
```
