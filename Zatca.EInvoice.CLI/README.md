# ZATCA E-Invoice CLI Testing Tool

A comprehensive command-line interface for testing all features of the Zatca.EInvoice library.

## Features

This CLI tool provides extensive testing coverage for:

### Invoice Types
- ✅ Standard Invoice (B2B)
- ✅ Simplified Invoice (B2C)
- ✅ Debit Notes
- ✅ Credit Notes
- ✅ Export Invoices
- ✅ Third Party Invoices
- ✅ Nominal Invoices
- ✅ Multi-line Invoices
- ✅ Invoices with Allowances/Charges

### Core Functionality
- ✅ Certificate Generation (CSR)
- ✅ Invoice Signing
- ✅ QR Code Generation
- ✅ Invoice Validation
- ✅ XML Generation
- ✅ API Client Integration

### Testing Features
- ✅ Edge Case Testing
- ✅ Performance Benchmarking
- ✅ Comprehensive Validation
- ✅ All Scenarios Runner

## Usage

### Interactive Mode

Run the CLI without arguments to enter interactive mode:

```bash
dotnet run
```

This will display a menu with all available test options.

### Command-Line Mode

Run specific tests directly from the command line:

```bash
# Test standard invoice
dotnet run -- standard

# Test simplified invoice
dotnet run -- simplified

# Test debit note
dotnet run -- debit

# Test credit note
dotnet run -- credit

# Test export invoice
dotnet run -- export

# Test certificate generation
dotnet run -- cert

# Test invoice signing
dotnet run -- sign

# Test validation
dotnet run -- validate

# Run all tests
dotnet run -- all
```

## Test Scenarios

### 1. Standard Invoice (B2B)
Tests a complete business-to-business invoice with:
- Full supplier and customer details
- Tax calculations (15% VAT)
- Payment means
- Single invoice line

### 2. Simplified Invoice (B2C)
Tests a simplified business-to-consumer invoice:
- Supplier details only
- Simplified tax structure
- Lower complexity requirements

### 3. Debit Note
Tests debit note functionality:
- Reference to original invoice
- Additional charges
- Billing reference structure

### 4. Credit Note
Tests credit note functionality:
- Negative amounts
- Return scenarios
- Reference to original invoice

### 5. Export Invoice
Tests export invoice scenarios:
- International customer
- Zero or reduced VAT
- Export-specific requirements

### 6. Third Party Invoice
Tests third-party transaction handling:
- Third party flag enabled
- Multiple party involvement

### 7. Nominal Invoice
Tests nominal invoice structure:
- Summary purposes
- Nominal flag enabled

### 8. Multi-Line Invoice
Tests invoices with multiple items:
- 3+ invoice lines
- Different products/services
- Aggregated tax calculations

### 9. Allowances and Charges
Tests discount and additional charge scenarios:
- Volume discounts
- Delivery fees
- Line-level and document-level adjustments

### 10. Certificate Generation
Tests CSR generation:
- Organization identifier validation
- Serial number format
- Private key generation
- Production vs. test mode

### 11. Invoice Signing
Tests digital signature functionality:
- XML canonicalization
- Hash generation
- Digital signature creation
- QR code generation

### 12. QR Code Generation
Tests QR code data structure:
- Tag encoding
- Data serialization
- Base64 encoding

### 13. Invoice Validation
Tests validation rules:
- Required field checks
- Data integrity validation
- Business rule compliance

### 14. XML Generation
Tests UBL 2.1 XML generation:
- Namespace handling
- Element structure
- Data serialization

### 15. API Client
Tests ZATCA API client:
- Environment configuration
- Authentication structure
- Endpoint methods

### 16. All Scenarios
Runs all test cases sequentially:
- Comprehensive coverage
- Success/failure reporting
- Summary statistics

### 17. Edge Cases
Tests boundary conditions:
- Empty/null values
- Negative amounts
- Large numbers
- Special characters
- Invalid data formats
- Future dates

### 18. Performance Testing
Benchmarks library performance:
- Invoice creation speed
- XML generation performance
- Validation throughput
- Memory usage patterns

## Output Examples

### Successful Test
```
▶ Testing Standard Invoice (B2B)...

Invoice Details:
  ID: INV-2025-001
  UUID: 12345678-1234-1234-1234-123456789abc
  Issue Date: 2025-12-18
  Issue Time: 14:30:00
  Type: standard - invoice
  Currency: SAR
  Supplier: Test Company LLC
  Customer: Customer Company Ltd
  Tax Amount: 15.00
  Total Amount: 115.00
  Lines: 1

Validation:
  ✓ Basic validation passed

XML Generation:
  ✓ XML generated successfully (2847 characters)
```

### Test Summary
```
════════════════════════════════════════
Test Summary
════════════════════════════════════════
Total Tests: 12
Passed: 11
Failed: 1
```

## Requirements

- .NET 6.0 or higher
- Zatca.EInvoice library reference
- Valid certificate (for signing tests)

## Project Structure

```
Zatca.EInvoice.CLI/
├── Program.cs              # Main CLI application
├── README.md              # This file
└── Zatca.EInvoice.CLI.csproj
```

## Building

```bash
cd Zatca.EInvoice.CLI
dotnet build
```

## Running Tests

```bash
# Interactive mode
dotnet run

# Specific test
dotnet run -- standard

# All tests
dotnet run -- all
```

## Notes

- Some tests (like invoice signing) require a valid X509 certificate with a private key
- API tests are demonstrations and don't make actual API calls without proper credentials
- Performance tests may take a few seconds to complete
- XML output is validated against UBL 2.1 standards

## Error Handling

The CLI includes comprehensive error handling:
- Validation errors are displayed in red
- Warnings are displayed in yellow
- Success messages are displayed in green
- Stack traces are shown for debugging

## Customization

You can extend the CLI by:
1. Adding new test scenarios in the `ExecuteOption` method
2. Creating helper methods for custom invoice structures
3. Adding command-line arguments for additional options
4. Integrating with actual ZATCA APIs

## License

This tool is part of the Zatca.EInvoice library project.
