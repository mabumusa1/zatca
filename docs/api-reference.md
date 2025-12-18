---
layout: default
title: API Reference
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

### ReportInvoiceAsync

Reports a simplified invoice to ZATCA (B2C transactions).

```csharp
public async Task<InvoiceSubmissionResult> ReportInvoiceAsync(
    string signedXmlInvoice,
    string invoiceHash,
    string uuid,
    string certificate,
    string secret,
    CancellationToken cancellationToken = default)
```

**Example:**
```csharp
var result = await client.ReportInvoiceAsync(
    signedResult.SignedXml,
    signedResult.InvoiceHash,
    invoiceUuid,
    certificate.RawCertificate,
    certificate.Secret
);

Console.WriteLine($"Reporting Status: {result.ReportingStatus}");
```

### ClearInvoiceAsync

Submits a standard invoice for clearance (B2B transactions).

```csharp
public async Task<InvoiceSubmissionResult> ClearInvoiceAsync(
    string signedXmlInvoice,
    string invoiceHash,
    string uuid,
    string certificate,
    string secret,
    CancellationToken cancellationToken = default)
```

**Example:**
```csharp
var result = await client.ClearInvoiceAsync(
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

---

## Result Classes

### ComplianceCertificateResult

```csharp
public class ComplianceCertificateResult
{
    public string? RequestId { get; set; }
    public string? DispositionMessage { get; set; }
    public string? BinarySecurityToken { get; set; }
    public string? Secret { get; set; }
    public List<string>? Errors { get; set; }
}
```

### ProductionCertificateResult

```csharp
public class ProductionCertificateResult
{
    public string? RequestId { get; set; }
    public string? DispositionMessage { get; set; }
    public string? BinarySecurityToken { get; set; }
    public string? Secret { get; set; }
    public List<string>? Errors { get; set; }
}
```

### InvoiceSubmissionResult

```csharp
public class InvoiceSubmissionResult
{
    public string? ClearanceStatus { get; set; }
    public string? ReportingStatus { get; set; }
    public string? ClearedInvoice { get; set; }
    public ValidationResults? ValidationResults { get; set; }
    public List<string>? Warnings { get; set; }
    public List<string>? Errors { get; set; }
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
