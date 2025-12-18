# ZATCA Certificate Renewal Implementation Plan

## Overview

This document outlines the implementation plan for adding the missing `production/csids/renewal` endpoint to the Zatca.EInvoice library. This endpoint is used to renew a production CSID (Cryptographic Stamp Identifier) certificate before it expires.

---

## Current State

### Implemented Endpoints (5/6)

| Endpoint | Path | Method | File Location |
|----------|------|--------|---------------|
| Compliance Certificate | `compliance` | POST | `ZatcaApiClient.cs:74-106` |
| Compliance Invoice Check | `compliance/invoices` | POST | `ZatcaApiClient.cs:108-138` |
| Production Certificate | `production/csids` | POST | `ZatcaApiClient.cs:140-170` |
| Clearance Invoice | `invoices/clearance/single` | POST | `ZatcaApiClient.cs:172-202` |
| Reporting Invoice | `invoices/reporting/single` | POST | `ZatcaApiClient.cs:204-233` |

### Missing Endpoint (1/6)

| Endpoint | Path | Method | Purpose |
|----------|------|--------|---------|
| Renew Production CSID | `production/csids/renewal` | PATCH | Renew expiring production certificate |

---

## Implementation Details

### 1. Add Endpoint Constant

**File:** `/workspace/Zatca.EInvoice/Api/ZatcaApiEndpoints.cs`

**Action:** Add new constant after line 59

```csharp
/// <summary>
/// Production certificate renewal endpoint.
/// </summary>
public const string ProductionCertificateRenewal = "production/csids/renewal";
```

---

### 2. Create Result Class

**File:** `/workspace/Zatca.EInvoice/Api/RenewalCertificateResult.cs` (NEW FILE)

**Action:** Create new result class for renewal response

```csharp
using System.Collections.Generic;

namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// Result of a production certificate renewal request.
    /// </summary>
    public class RenewalCertificateResult
    {
        /// <summary>
        /// Gets the renewed binary security token (base64-encoded certificate).
        /// </summary>
        public string BinarySecurityToken { get; }

        /// <summary>
        /// Gets the new secret for API authentication.
        /// </summary>
        public string Secret { get; }

        /// <summary>
        /// Gets the new request ID.
        /// </summary>
        public string RequestId { get; }

        /// <summary>
        /// Gets the disposition message from ZATCA.
        /// </summary>
        public string DispositionMessage { get; }

        /// <summary>
        /// Gets the list of error messages.
        /// </summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>
        /// Gets the list of warning messages.
        /// </summary>
        public IReadOnlyList<string> Warnings { get; }

        /// <summary>
        /// Gets a value indicating whether the renewal was successful.
        /// </summary>
        public bool IsSuccess => Errors == null || Errors.Count == 0;

        /// <summary>
        /// Gets a value indicating whether the result contains warnings.
        /// </summary>
        public bool HasWarnings => Warnings != null && Warnings.Count > 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenewalCertificateResult"/> class.
        /// </summary>
        public RenewalCertificateResult(
            string binarySecurityToken,
            string secret,
            string requestId,
            string dispositionMessage,
            IReadOnlyList<string> errors,
            IReadOnlyList<string> warnings)
        {
            BinarySecurityToken = binarySecurityToken ?? string.Empty;
            Secret = secret ?? string.Empty;
            RequestId = requestId ?? string.Empty;
            DispositionMessage = dispositionMessage ?? string.Empty;
            Errors = errors ?? new List<string>();
            Warnings = warnings ?? new List<string>();
        }
    }
}
```

**Note:** Alternatively, you can reuse `ProductionCertificateResult` since the response structure is identical.

---

### 3. Update Interface

**File:** `/workspace/Zatca.EInvoice/Api/IZatcaApiClient.cs`

**Action:** Add new method signature to the interface

```csharp
/// <summary>
/// Renews a production certificate (PCSID) before expiration.
/// </summary>
/// <param name="otp">One-time password from ZATCA portal.</param>
/// <param name="csr">Certificate Signing Request (CSR) in PEM format.</param>
/// <param name="certificate">Current production certificate (base64-encoded).</param>
/// <param name="secret">Current production secret.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The renewal result containing the new certificate and secret.</returns>
Task<ProductionCertificateResult> RenewProductionCertificateAsync(
    string otp,
    string csr,
    string certificate,
    string secret,
    CancellationToken cancellationToken = default);
```

---

### 4. Implement API Method

**File:** `/workspace/Zatca.EInvoice/Api/ZatcaApiClient.cs`

**Action:** Add new method after `RequestProductionCertificateAsync` (around line 170)

```csharp
/// <inheritdoc/>
public async Task<ProductionCertificateResult> RenewProductionCertificateAsync(
    string otp,
    string csr,
    string certificate,
    string secret,
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(otp))
        throw new ArgumentNullException(nameof(otp));
    if (string.IsNullOrWhiteSpace(csr))
        throw new ArgumentNullException(nameof(csr));
    if (string.IsNullOrWhiteSpace(certificate))
        throw new ArgumentNullException(nameof(certificate));
    if (string.IsNullOrWhiteSpace(secret))
        throw new ArgumentNullException(nameof(secret));

    // ZATCA API expects the entire PEM file to be base64-encoded
    var csrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(csr));

    var payload = new
    {
        csr = csrBase64
    };

    var headers = CreateAuthHeaders(certificate, secret);
    headers["OTP"] = otp;
    headers["Accept-Version"] = "V2";
    headers["Content-Type"] = "application/json";

    var response = await SendPatchRequestAsync<Dictionary<string, object>>(
        ZatcaApiEndpoints.ProductionCertificateRenewal,
        payload,
        headers,
        cancellationToken);

    return ParseProductionCertificateResult(response);
}
```

---

### 5. Add PATCH Request Support

**File:** `/workspace/Zatca.EInvoice/Api/ZatcaApiClient.cs`

**Action:** Add new method to support PATCH HTTP method (the current `SendRequestAsync` can be extended or a new method created)

**Option A:** Modify existing `SendRequestAsync` to accept `HttpMethod` parameter (already supports this)

**Option B:** Create dedicated PATCH method for clarity

```csharp
private async Task<T> SendPatchRequestAsync<T>(
    string endpoint,
    object payload,
    Dictionary<string, string> headers,
    CancellationToken cancellationToken)
{
    return await SendRequestAsync<T>(
        new HttpMethod("PATCH"),
        endpoint,
        payload,
        headers,
        cancellationToken);
}
```

**Note:** The existing `SendRequestAsync` already accepts `HttpMethod` as a parameter, so you can call it directly with `new HttpMethod("PATCH")` or `HttpMethod.Patch` (.NET 5+).

---

### 6. Add CLI Command (Optional)

**File:** `/workspace/Zatca.EInvoice.CLI/Commands/ApiCommands.cs` (or equivalent)

**Action:** Add CLI command for certificate renewal

```csharp
[Command("renew-cert", Description = "Renew a production certificate")]
public class RenewCertCommand : ICommand
{
    [CommandOption("--csr", 'c', Description = "Path to CSR file")]
    public string CsrPath { get; set; }

    [CommandOption("--otp", 'o', Description = "One-time password from ZATCA")]
    public string Otp { get; set; }

    [CommandOption("--cert", Description = "Path to current production certificate")]
    public string CertPath { get; set; }

    [CommandOption("--secret", 's', Description = "Current production secret")]
    public string Secret { get; set; }

    [CommandOption("--env", 'e', Description = "Environment (sandbox/simulation/production)")]
    public string Environment { get; set; } = "sandbox";

    [CommandOption("--output", Description = "Output directory for new certificate")]
    public string OutputPath { get; set; }

    // Implementation...
}
```

---

## API Request/Response Specification

### Request

```http
PATCH /e-invoicing/developer-portal/production/csids/renewal HTTP/1.1
Host: gw-fatoora.zatca.gov.sa
Authorization: Basic {base64(certificate:secret)}
OTP: {otp}
Accept-Version: V2
Content-Type: application/json
Accept: application/json

{
    "csr": "{base64-encoded-csr}"
}
```

### Request Headers

| Header | Value | Required |
|--------|-------|----------|
| Authorization | `Basic {base64(current_cert:current_secret)}` | Yes |
| OTP | One-time password from ZATCA portal | Yes |
| Accept-Version | `V2` | Yes |
| Content-Type | `application/json` | Yes |
| Accept | `application/json` | Yes |

### Request Body

| Field | Type | Description |
|-------|------|-------------|
| csr | string | Base64-encoded CSR (the entire PEM file content, then base64 encoded) |

### Response (Success - 200 OK)

```json
{
    "requestID": "string",
    "dispositionMessage": "string",
    "binarySecurityToken": "string",
    "secret": "string",
    "errors": [],
    "warnings": []
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| requestID | string | New request identifier |
| dispositionMessage | string | Status message from ZATCA |
| binarySecurityToken | string | Base64-encoded renewed certificate |
| secret | string | New secret for API authentication |
| errors | array | List of error messages (empty on success) |
| warnings | array | List of warning messages |

---

## Testing Plan

### Unit Tests

**File:** `/workspace/Zatca.EInvoice.Tests/Api/ZatcaApiClientTests.cs`

1. Test successful certificate renewal
2. Test renewal with invalid OTP
3. Test renewal with expired certificate
4. Test renewal with invalid CSR
5. Test renewal with missing parameters (null checks)

### Integration Tests

**File:** `/workspace/Zatca.EInvoice.CLI/TestsData/renewal-workflow.sh` (NEW)

```bash
#!/bin/bash
# Test certificate renewal workflow

# 1. Use existing production certificate that's close to expiration
# 2. Generate new CSR with same organization details
# 3. Request OTP from ZATCA portal
# 4. Call renewal endpoint
# 5. Verify new certificate is valid
```

### Manual Testing Steps

1. Complete full compliance workflow to get production certificate
2. Wait for certificate to approach expiration (or use test certificate)
3. Generate new CSR with same details
4. Request new OTP from ZATCA portal
5. Call `RenewProductionCertificateAsync`
6. Verify new certificate and secret are returned
7. Verify new certificate can be used for invoice submission

---

## Implementation Checklist

- [ ] Add `ProductionCertificateRenewal` constant to `ZatcaApiEndpoints.cs`
- [ ] Add `RenewProductionCertificateAsync` method signature to `IZatcaApiClient.cs`
- [ ] Implement `RenewProductionCertificateAsync` in `ZatcaApiClient.cs`
- [ ] Verify PATCH method works with existing `SendRequestAsync`
- [ ] Add CLI command for certificate renewal (optional)
- [ ] Write unit tests
- [ ] Write integration test script
- [ ] Update documentation/README
- [ ] Test against ZATCA sandbox

---

## Dependencies

No new NuGet packages required. Uses existing:
- `System.Net.Http` (for PATCH support)
- `System.Text.Json` (for JSON serialization)

---

## Estimated Effort

| Task | Complexity | Notes |
|------|------------|-------|
| Add endpoint constant | Low | Single line addition |
| Update interface | Low | Single method signature |
| Implement API method | Medium | Similar to existing methods |
| Add CLI command | Medium | Follow existing pattern |
| Unit tests | Medium | Mock HTTP responses |
| Integration tests | High | Requires valid certificates |

---

## References

- ZATCA E-Invoicing Developer Portal: https://sandbox.zatca.gov.sa/IntegrationSandbox
- ZATCA Technical Guidelines: https://zatca.gov.sa/en/E-Invoicing/SystemsDevelopers/Pages/TechnicalRequirementsSpec.aspx
- Existing implementation: `/workspace/Zatca.EInvoice/Api/ZatcaApiClient.cs`

---

## Notes

1. The renewal endpoint uses **PATCH** method (not POST like other endpoints)
2. Renewal requires a **new OTP** from the ZATCA portal
3. Renewal requires a **new CSR** (can use same private key or generate new one)
4. Authentication uses the **current** production certificate and secret
5. Response structure is identical to initial production certificate request
6. Certificate renewal should be initiated before the current certificate expires
