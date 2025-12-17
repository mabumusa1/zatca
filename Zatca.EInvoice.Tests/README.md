# Zatca.EInvoice.Tests

Advanced test suite for the Zatca.EInvoice .NET 8 library.

## Test Structure

This test project contains comprehensive unit and integration tests covering all major components of the ZATCA e-invoicing library.

### Test Files Created

#### 1. **Certificates/CertificateBuilderTests.cs**
Tests for the `CertificateBuilder` class that handles CSR and private key generation.

**Test Cases:**
- `TestGenerateAndSave()` - Verifies CSR and private key generation, ensures files are created with correct PEM format
- `TestMissingRequiredParameterThrowsException()` - Validates that missing `businessCategory` throws `CertificateBuilderException`
- `TestInvalidOrganizationIdentifierThrowsException()` - Tests validation of organization identifier format (must be 15 digits, start and end with 3)
- `TestInvalidCountryCodeThrowsException()` - Validates country code must be exactly 2 characters
- `TestGetCsrReturnsValidString()` - Verifies CSR content format
- `TestGetPrivateKeyReturnsValidString()` - Verifies private key content format
- `TestGetCsrBeforeGenerateThrowsException()` - Ensures proper error handling before generation
- `TestGetPrivateKeyBeforeGenerateThrowsException()` - Ensures proper error handling before generation
- `TestProductionFlagAffectsCsr()` - Validates production vs testing environment differentiation

**Features:**
- Uses temporary files for testing, with automatic cleanup via `IDisposable`
- Validates PEM format of generated certificates
- Tests all required parameters and validation rules

---

#### 2. **Signing/InvoiceSignerTests.cs**
Tests for the `InvoiceSigner` class that handles digital signing and QR code generation.

**Test Cases:**
- `TestSignInvoiceProducesValidOutput()` - Verifies signed XML contains UBL extensions, signature, and QR code elements
- `TestGetHash()` - Validates SHA-256 hash generation (32 bytes)
- `TestGetQrCode()` - Verifies QR code generation returns base64-encoded TLV data
- `TestSignThrowsExceptionForNullInvoiceXml()` - Tests null parameter validation
- `TestSignThrowsExceptionForEmptyInvoiceXml()` - Tests empty parameter validation
- `TestSignThrowsExceptionForNullCertificate()` - Tests certificate requirement
- `TestSignThrowsExceptionForCertificateWithoutPrivateKey()` - Validates certificate must have private key
- `TestGetHashConsistency()` - Ensures hash is deterministic
- `TestGetHashDifferentForDifferentInputs()` - Ensures hash changes with input
- `TestSignedInvoiceContainsQrTags()` - Verifies QR code embedded in AdditionalDocumentReference

**Features:**
- Uses mock X509Certificate2 with ECDSA private key (nistP256 curve)
- Validates signed XML structure and namespaces
- Tests SHA-256 hashing and base64 encoding
- Automatic certificate cleanup

---

#### 3. **Api/ZatcaApiClientTests.cs**
Tests for the `ZatcaApiClient` class with mocked HTTP responses using Moq.

**Test Cases:**
- `TestValidEnvironmentDoesNotThrow()` - Validates all environment enum values
- `TestRequestComplianceCertificateSuccess()` - Mocks successful compliance certificate request
- `TestValidateInvoiceComplianceSuccess()` - Mocks successful compliance validation response
- `TestRequestProductionCertificateSuccess()` - Mocks successful production certificate request
- `TestSubmitClearanceInvoiceSuccess()` - Mocks successful clearance invoice submission with cleared invoice response
- `TestSendRequestFailure()` - Tests HTTP 400 error handling
- `TestInternalServerErrorThrowsException()` - Tests HTTP 500 error handling
- `TestNullParametersThrowException()` - Validates all required parameters across all methods
- `TestWarningHandling()` - Tests HTTP 202 (Accepted) with warnings when warning handling is enabled
- `TestResponseWithValidationErrors()` - Validates parsing of error messages from API responses

**Features:**
- Uses Moq to mock `HttpMessageHandler` for full HTTP client control
- Helper method `CreateMockHttpClient()` for creating mock HTTP clients
- Tests all API methods (compliance cert, production cert, validation, clearance, reporting)
- Validates JSON serialization/deserialization
- Tests base64 encoding of requests and responses
- Automatic client disposal

---

#### 4. **Integration/InvoiceIntegrationTests.cs**
End-to-end integration tests covering the complete invoice generation workflow.

**Test Cases:**
- `TestCompleteInvoiceGenerationFlow()` - Full workflow: data → InvoiceMapper → Invoice → InvoiceGenerator → XML
  - Creates comprehensive invoice with 2 line items
  - Validates mapped Invoice object properties
  - Generates XML and validates structure
  - Verifies all UBL 2.1 elements and namespaces
- `TestMinimalInvoiceGeneration()` - Tests minimal required fields
- `TestInvoiceGenerationWithBillingReferences()` - Tests billing reference mapping and XML generation
- `TestInvoiceGenerationWithAllowanceCharges()` - Tests discount/charge mapping

**Features:**
- Tests integration between `InvoiceMapper` and `InvoiceGenerator`
- Validates XML namespaces and structure
- Tests comprehensive invoice with:
  - Supplier and customer parties
  - Multiple invoice lines
  - Tax calculations (VAT 15%)
  - Payment means
  - Monetary totals
- Helper method `CreateTestInvoiceData()` provides realistic test data

---

## Running the Tests

### Build the test project:
```bash
dotnet build Zatca.EInvoice.Tests/Zatca.EInvoice.Tests.csproj
```

### Run all tests:
```bash
dotnet test Zatca.EInvoice.Tests/Zatca.EInvoice.Tests.csproj
```

### Run tests with detailed output:
```bash
dotnet test Zatca.EInvoice.Tests/Zatca.EInvoice.Tests.csproj --logger "console;verbosity=detailed"
```

### Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~CertificateBuilderTests"
dotnet test --filter "FullyQualifiedName~InvoiceSignerTests"
dotnet test --filter "FullyQualifiedName~ZatcaApiClientTests"
dotnet test --filter "FullyQualifiedName~InvoiceIntegrationTests"
```

### Run tests with code coverage:
```bash
dotnet test Zatca.EInvoice.Tests/Zatca.EInvoice.Tests.csproj /p:CollectCoverage=true
```

---

## Test Dependencies

The test project uses:
- **xUnit** 2.6.2 - Test framework
- **Moq** 4.20.70 - Mocking framework for HTTP client
- **FluentAssertions** 6.12.0 - Fluent assertion library
- **Microsoft.NET.Test.Sdk** 17.8.0 - Test SDK
- **coverlet.collector** 6.0.0 - Code coverage

---

## Test Patterns

### Mocking HTTP Requests
All API tests use the `CreateMockHttpClient()` helper method:

```csharp
private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string responseContent)
{
    var mockHandler = new Mock<HttpMessageHandler>();
    mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(responseContent)
        });
    return new HttpClient(mockHandler.Object);
}
```

### Certificate Generation for Testing
The `InvoiceSignerTests` creates a self-signed ECDSA certificate:

```csharp
private X509Certificate2 CreateMockCertificate()
{
    using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    var certRequest = new CertificateRequest(
        "CN=Test Certificate, O=Test Org, C=SA",
        ecdsa,
        HashAlgorithmName.SHA256);
    // ... certificate creation logic
}
```

### Cleanup Pattern
All test classes that use disposable resources implement `IDisposable`:

```csharp
public class TestClass : IDisposable
{
    public void Dispose()
    {
        // Cleanup logic
    }
}
```

---

## Coverage Summary

### Classes Tested
- ✅ `CertificateBuilder` - Certificate and CSR generation
- ✅ `InvoiceSigner` - Digital signing and QR code generation
- ✅ `ZatcaApiClient` - API communication with ZATCA
- ✅ `InvoiceMapper` - Invoice data mapping
- ✅ `InvoiceGenerator` - XML generation

### Namespaces Tested
- `Zatca.EInvoice.Certificates`
- `Zatca.EInvoice.Signing`
- `Zatca.EInvoice.Api`
- `Zatca.EInvoice.Mappers`
- `Zatca.EInvoice.Xml`

---

## Notes

1. **Certificate Builder Tests**: Use temporary files that are automatically cleaned up after each test run.

2. **API Client Tests**: Mock all HTTP requests, so no actual network calls are made during testing.

3. **Invoice Signer Tests**: Use self-signed certificates generated at runtime, eliminating the need for test certificate files.

4. **Integration Tests**: Validate the complete workflow from invoice data to XML generation, ensuring all components work together correctly.

5. **Test Data**: The integration tests use comprehensive, realistic invoice data that follows ZATCA specifications.

---

## Future Enhancements

Potential areas for additional test coverage:
- Additional edge cases for invoice validation
- Performance/load testing for API client
- More complex invoice scenarios (multi-currency, advanced tax scenarios)
- Certificate chain validation
- QR code decoding validation
- XML schema validation against UBL 2.1 XSD

---

## License

This test project is part of the Zatca.EInvoice library.
