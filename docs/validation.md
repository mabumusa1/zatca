---
layout: default
title: Validation
nav_order: 6
description: "Invoice validation before ZATCA submission"
---

# Invoice Validation

The Zatca.EInvoice library provides comprehensive validation capabilities to ensure your invoices meet ZATCA's requirements before submission. The validation system includes field validation, amount validation, and business rule verification.

## Table of Contents

- [Overview](#overview)
- [ValidationResult Class](#validationresult-class)
- [IInvoiceValidator Interface](#iinvoicevalidator-interface)
- [InvoiceValidator Class](#invoicevalidator-class)
- [InvoiceAmountValidator Class](#invoiceamountvalidator-class)
- [How to Validate Invoices](#how-to-validate-invoices)
- [Common Validation Errors](#common-validation-errors)
- [Amount Validation Rules](#amount-validation-rules)

## Overview

The validation system consists of four main components:

1. **ValidationResult** - Holds validation results including success status and error messages
2. **IInvoiceValidator** - Interface defining the contract for invoice validators
3. **InvoiceValidator** - Validates required fields and data structure
4. **InvoiceAmountValidator** - Validates financial amounts and calculations

All validators follow ZATCA's validation rules as specified in `20210819_ZATCA_E-invoice_Validation_Rules`.

## ValidationResult Class

The `ValidationResult` class represents the outcome of a validation operation.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsValid` | `bool` | Indicates whether validation was successful |
| `Errors` | `List<string>` | List of validation error messages |

### Methods

#### Constructor
```csharp
public ValidationResult()
```
Creates a new validation result with `IsValid = true` and an empty error list.

#### AddError
```csharp
public void AddError(string error)
```
Adds an error message and sets `IsValid` to `false`.

#### Static Factory Methods

**Success**
```csharp
public static ValidationResult Success()
```
Creates a successful validation result.

**Failure (single error)**
```csharp
public static ValidationResult Failure(string error)
```
Creates a failed validation result with a single error.

**Failure (multiple errors)**
```csharp
public static ValidationResult Failure(IEnumerable<string> errors)
```
Creates a failed validation result with multiple errors.

### Example Usage

```csharp
var result = new ValidationResult();

if (string.IsNullOrEmpty(invoiceId))
{
    result.AddError("Invoice ID is required");
}

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

## IInvoiceValidator Interface

The `IInvoiceValidator` interface defines the contract for invoice validators.

### Methods

#### Validate
```csharp
ValidationResult Validate(Dictionary<string, object> data)
```
Validates the invoice data and returns a `ValidationResult` with any errors found.

#### ValidateAndThrow
```csharp
void ValidateAndThrow(Dictionary<string, object> data)
```
Validates the invoice data and throws an `ArgumentException` if validation fails.

### Implementation

```csharp
public class CustomValidator : IInvoiceValidator
{
    public ValidationResult Validate(Dictionary<string, object> data)
    {
        var result = new ValidationResult();
        // Add validation logic
        return result;
    }

    public void ValidateAndThrow(Dictionary<string, object> data)
    {
        var result = Validate(data);
        if (!result.IsValid)
        {
            throw new ArgumentException(string.Join(", ", result.Errors));
        }
    }
}
```

## InvoiceValidator Class

The `InvoiceValidator` class validates required fields for invoice data according to ZATCA's validation rules.

### Validation Categories

The validator checks the following categories:

1. **Top-Level Fields** - UUID, ID, issue date, currency codes
2. **Invoice Type** - Invoice type and subtype
3. **Supplier Data** - Registration name, tax ID, address
4. **Customer Data** - Required for non-simplified invoices
5. **Payment Means** - Payment method code
6. **Tax Totals** - Tax amounts and categories
7. **Legal Monetary Total** - Financial totals
8. **Invoice Lines** - Line items with all required fields
9. **Additional Documents** - Referenced documents like PIH

### Methods

#### Validate
```csharp
public ValidationResult Validate(Dictionary<string, object> data)
```
Validates all required fields and returns a `ValidationResult`.

**Example:**
```csharp
var validator = new InvoiceValidator();
var invoiceData = new Dictionary<string, object>
{
    { "uuid", "8e6000cf-1a98-4e6f-987c-f694a9e34e3e" },
    { "id", "INV-001" },
    { "issueDate", "2024-01-15" },
    { "currencyCode", "SAR" },
    { "taxCurrencyCode", "SAR" }
    // ... more fields
};

ValidationResult result = validator.Validate(invoiceData);

if (!result.IsValid)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

#### ValidateAndThrow
```csharp
public void ValidateAndThrow(Dictionary<string, object> data)
```
Validates the invoice and throws an `ArgumentException` if validation fails.

**Example:**
```csharp
var validator = new InvoiceValidator();

try
{
    validator.ValidateAndThrow(invoiceData);
    Console.WriteLine("Validation successful!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
```

### Required Fields

#### Top-Level Fields
- `uuid` - Unique identifier for the invoice
- `id` - Invoice number/ID
- `issueDate` - Invoice issue date
- `currencyCode` - Invoice currency code
- `taxCurrencyCode` - Tax currency code

#### Invoice Type
- `invoiceType.invoice` - Invoice category (e.g., "standard", "simplified")
- `invoiceType.type` - Invoice type code

#### Supplier Fields
- `supplier.registrationName` - Supplier's registered name
- `supplier.taxId` - Supplier's tax ID
- `supplier.address.street` - Street name
- `supplier.address.buildingNumber` - Building number
- `supplier.address.city` - City name
- `supplier.address.postalZone` - Postal/ZIP code
- `supplier.address.country` - Country code

#### Customer Fields (Non-Simplified Invoices Only)
- `customer.registrationName` - Customer's registered name
- `customer.taxId` - Customer's tax ID
- `customer.address.street` - Street name
- `customer.address.buildingNumber` - Building number
- `customer.address.city` - City name
- `customer.address.postalZone` - Postal/ZIP code
- `customer.address.country` - Country code

#### Payment Means
- `paymentMeans.code` - Payment method code

#### Tax Total
- `taxTotal.taxAmount` - Total tax amount
- `taxTotal.subTotals[].taxableAmount` - Taxable amount for each category
- `taxTotal.subTotals[].taxCategory` - Tax category details
- `taxTotal.subTotals[].taxCategory.taxScheme.id` - Tax scheme ID

#### Legal Monetary Total
- `legalMonetaryTotal.lineExtensionAmount` - Sum of line amounts
- `legalMonetaryTotal.taxExclusiveAmount` - Amount before tax
- `legalMonetaryTotal.taxInclusiveAmount` - Amount including tax
- `legalMonetaryTotal.payableAmount` - Total payable amount

#### Invoice Lines
Each invoice line requires:
- `id` - Line number/ID
- `unitCode` - Unit of measure code
- `quantity` - Quantity
- `lineExtensionAmount` - Line total amount
- `item.name` - Item/service name
- `item.classifiedTaxCategory[0].taxScheme.id` - Tax scheme ID
- `item.classifiedTaxCategory[0].percent` - Tax percentage
- `price.amount` - Unit price
- `taxTotal.taxAmount` - Line tax amount

#### Additional Documents
- `additionalDocuments[].id` - Document ID
- `additionalDocuments[].attachment` - Required for PIH (Previous Invoice Hash) documents

## InvoiceAmountValidator Class

The `InvoiceAmountValidator` class validates financial amounts and calculations in the invoice.

### Validation Rules

The validator ensures:
1. All monetary values are numeric
2. All monetary values are non-negative
3. Tax inclusive amount = Tax exclusive amount + Tax total
4. Line extension amount = Price × Quantity
5. Line rounding amount = Line extension + Line tax
6. Tax percentages are between 0 and 100

### Tolerance

The validator uses a tolerance of **0.01** for amount comparisons to account for rounding differences.

### Methods

#### ValidateMonetaryTotals
```csharp
public ValidationResult ValidateMonetaryTotals(Dictionary<string, object> data)
```
Validates the legal monetary totals section.

**Example:**
```csharp
var amountValidator = new InvoiceAmountValidator();
var result = amountValidator.ValidateMonetaryTotals(invoiceData);

if (!result.IsValid)
{
    Console.WriteLine("Amount validation failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

#### ValidateMonetaryTotalsAndThrow
```csharp
public void ValidateMonetaryTotalsAndThrow(Dictionary<string, object> data)
```
Validates monetary totals and throws an exception if validation fails.

#### ValidateInvoiceLines
```csharp
public ValidationResult ValidateInvoiceLines(IList<object> invoiceLines)
```
Validates invoice line amounts and calculations.

**Example:**
```csharp
var amountValidator = new InvoiceAmountValidator();
var invoiceLines = (IList<object>)invoiceData["invoiceLines"];
var result = amountValidator.ValidateInvoiceLines(invoiceLines);

if (!result.IsValid)
{
    Console.WriteLine("Line validation failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

#### ValidateInvoiceLinesAndThrow
```csharp
public void ValidateInvoiceLinesAndThrow(IList<object> invoiceLines)
```
Validates invoice lines and throws an exception if validation fails.

## How to Validate Invoices

### Complete Validation Example

Here's a complete example showing how to validate an invoice before submission:

```csharp
using Zatca.EInvoice.Validation;
using System;
using System.Collections.Generic;

public class InvoiceValidationExample
{
    public void ValidateInvoice()
    {
        // Prepare invoice data
        var invoiceData = new Dictionary<string, object>
        {
            { "uuid", "8e6000cf-1a98-4e6f-987c-f694a9e34e3e" },
            { "id", "INV-001" },
            { "issueDate", "2024-01-15" },
            { "currencyCode", "SAR" },
            { "taxCurrencyCode", "SAR" },
            { "invoiceType", new Dictionary<string, object>
                {
                    { "invoice", "standard" },
                    { "type", "388" }
                }
            },
            { "supplier", new Dictionary<string, object>
                {
                    { "registrationName", "ABC Company" },
                    { "taxId", "300000000000003" },
                    { "address", new Dictionary<string, object>
                        {
                            { "street", "King Fahd Road" },
                            { "buildingNumber", "1234" },
                            { "city", "Riyadh" },
                            { "postalZone", "12345" },
                            { "country", "SA" }
                        }
                    }
                }
            },
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 100.00m },
                    { "taxExclusiveAmount", 100.00m },
                    { "taxInclusiveAmount", 115.00m },
                    { "payableAmount", 115.00m }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 15.00m }
                }
            }
            // ... more fields
        };

        // Create validators
        var fieldValidator = new InvoiceValidator();
        var amountValidator = new InvoiceAmountValidator();

        // Validate fields
        ValidationResult fieldResult = fieldValidator.Validate(invoiceData);
        if (!fieldResult.IsValid)
        {
            Console.WriteLine("Field validation failed:");
            foreach (var error in fieldResult.Errors)
            {
                Console.WriteLine($"- {error}");
            }
            return;
        }

        // Validate amounts
        ValidationResult amountResult = amountValidator.ValidateMonetaryTotals(invoiceData);
        if (!amountResult.IsValid)
        {
            Console.WriteLine("Amount validation failed:");
            foreach (var error in amountResult.Errors)
            {
                Console.WriteLine($"- {error}");
            }
            return;
        }

        Console.WriteLine("Invoice validation successful!");
    }
}
```

### Using ValidateAndThrow

For scenarios where you want to fail fast:

```csharp
public void ValidateInvoiceWithExceptions()
{
    var invoiceData = PrepareInvoiceData();
    var fieldValidator = new InvoiceValidator();
    var amountValidator = new InvoiceAmountValidator();

    try
    {
        // Validate fields
        fieldValidator.ValidateAndThrow(invoiceData);

        // Validate amounts
        amountValidator.ValidateMonetaryTotalsAndThrow(invoiceData);

        // Validate invoice lines
        var invoiceLines = (IList<object>)invoiceData["invoiceLines"];
        amountValidator.ValidateInvoiceLinesAndThrow(invoiceLines);

        Console.WriteLine("All validations passed!");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Validation failed: {ex.Message}");
        // Handle validation failure
    }
}
```

## Common Validation Errors

### Missing Required Fields

**Error:** "The field 'UUID' is required and cannot be empty."

**Fix:**
```csharp
invoiceData["uuid"] = Guid.NewGuid().ToString();
```

**Error:** "The field 'Invoice ID' is required and cannot be empty."

**Fix:**
```csharp
invoiceData["id"] = "INV-001";
```

### Invalid Supplier Data

**Error:** "Supplier data is required."

**Fix:**
```csharp
invoiceData["supplier"] = new Dictionary<string, object>
{
    { "registrationName", "My Company" },
    { "taxId", "300000000000003" },
    { "address", new Dictionary<string, object>
        {
            { "street", "Main Street" },
            { "buildingNumber", "123" },
            { "city", "Riyadh" },
            { "postalZone", "12345" },
            { "country", "SA" }
        }
    }
};
```

### Missing Customer Data

**Error:** "Customer data is required for non-simplified invoices."

**Fix:**
```csharp
// For standard invoices, add customer data
invoiceData["customer"] = new Dictionary<string, object>
{
    { "registrationName", "Customer Company" },
    { "taxId", "300000000000004" },
    { "address", new Dictionary<string, object>
        {
            { "street", "Customer Street" },
            { "buildingNumber", "456" },
            { "city", "Jeddah" },
            { "postalZone", "54321" },
            { "country", "SA" }
        }
    }
};

// Or change to simplified invoice
invoiceData["invoiceType"] = new Dictionary<string, object>
{
    { "invoice", "simplified" },
    { "type", "388" }
};
```

### Invalid Invoice Lines

**Error:** "At least one invoice line is required."

**Fix:**
```csharp
invoiceData["invoiceLines"] = new List<object>
{
    new Dictionary<string, object>
    {
        { "id", "1" },
        { "unitCode", "PCE" },
        { "quantity", 1.0m },
        { "lineExtensionAmount", 100.00m },
        { "item", new Dictionary<string, object>
            {
                { "name", "Product Name" },
                { "classifiedTaxCategory", new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            { "percent", 15.0m },
                            { "taxScheme", new Dictionary<string, object>
                                {
                                    { "id", "VAT" }
                                }
                            }
                        }
                    }
                }
            }
        },
        { "price", new Dictionary<string, object>
            {
                { "amount", 100.00m }
            }
        },
        { "taxTotal", new Dictionary<string, object>
            {
                { "taxAmount", 15.00m },
                { "roundingAmount", 115.00m }
            }
        }
    }
};
```

### PIH Document Missing Attachment

**Error:** "The attachment for AdditionalDocuments[0] with id 'PIH' is required."

**Fix:**
```csharp
var pihDocument = additionalDocuments[0] as Dictionary<string, object>;
pihDocument["attachment"] = previousInvoiceHash; // Base64-encoded hash
```

## Amount Validation Rules

### Legal Monetary Total Validation

The validator checks that:

1. **All amounts are numeric and non-negative**
   ```csharp
   legalMonetaryTotal["lineExtensionAmount"] = 100.00m;  // ✓ Valid
   legalMonetaryTotal["lineExtensionAmount"] = -50.00m;  // ✗ Invalid (negative)
   legalMonetaryTotal["lineExtensionAmount"] = "text";   // ✗ Invalid (not numeric)
   ```

2. **Tax inclusive calculation is correct**
   ```
   taxInclusiveAmount = taxExclusiveAmount + taxTotalAmount
   ```

   **Example:**
   ```csharp
   // Valid example
   legalMonetaryTotal["taxExclusiveAmount"] = 100.00m;
   taxTotal["taxAmount"] = 15.00m;
   legalMonetaryTotal["taxInclusiveAmount"] = 115.00m;  // ✓ Correct

   // Invalid example
   legalMonetaryTotal["taxExclusiveAmount"] = 100.00m;
   taxTotal["taxAmount"] = 15.00m;
   legalMonetaryTotal["taxInclusiveAmount"] = 120.00m;  // ✗ Incorrect
   ```

### Invoice Line Validation

The validator checks each invoice line for:

1. **Numeric and non-negative values**
   ```csharp
   line["quantity"] = 5.0m;                    // ✓ Valid
   line["quantity"] = -1.0m;                   // ✗ Invalid (negative)
   line["price"]["amount"] = 100.00m;          // ✓ Valid
   line["price"]["amount"] = -50.00m;          // ✗ Invalid (negative)
   ```

2. **Line extension calculation**
   ```
   lineExtensionAmount = priceAmount × quantity
   ```

   **Example:**
   ```csharp
   // Valid example
   line["price"]["amount"] = 100.00m;
   line["quantity"] = 2.0m;
   line["lineExtensionAmount"] = 200.00m;      // ✓ Correct (100 × 2)

   // Invalid example
   line["price"]["amount"] = 100.00m;
   line["quantity"] = 2.0m;
   line["lineExtensionAmount"] = 150.00m;      // ✗ Incorrect
   ```

3. **Rounding amount calculation**
   ```
   roundingAmount = lineExtensionAmount + taxAmount
   ```

   **Example:**
   ```csharp
   // Valid example
   line["lineExtensionAmount"] = 200.00m;
   line["taxTotal"]["taxAmount"] = 30.00m;
   line["taxTotal"]["roundingAmount"] = 230.00m;  // ✓ Correct (200 + 30)

   // Invalid example
   line["lineExtensionAmount"] = 200.00m;
   line["taxTotal"]["taxAmount"] = 30.00m;
   line["taxTotal"]["roundingAmount"] = 240.00m;  // ✗ Incorrect
   ```

4. **Tax percentage range**
   ```csharp
   item["taxPercent"] = 15.0m;    // ✓ Valid (0-100)
   item["taxPercent"] = 150.0m;   // ✗ Invalid (> 100)
   item["taxPercent"] = -5.0m;    // ✗ Invalid (< 0)
   ```

### Rounding Tolerance

All amount comparisons allow a tolerance of **0.01** to account for rounding differences:

```csharp
// These would all be considered valid:
Expected: 115.00, Actual: 115.00  // ✓ Exact match
Expected: 115.00, Actual: 115.01  // ✓ Within tolerance
Expected: 115.00, Actual: 114.99  // ✓ Within tolerance
Expected: 115.00, Actual: 115.02  // ✗ Outside tolerance
```

### Complete Amount Validation Example

```csharp
using Zatca.EInvoice.Validation;

public void ValidateAmounts()
{
    var invoiceData = new Dictionary<string, object>
    {
        // Legal monetary totals
        { "legalMonetaryTotal", new Dictionary<string, object>
            {
                { "lineExtensionAmount", 200.00m },
                { "taxExclusiveAmount", 200.00m },
                { "taxInclusiveAmount", 230.00m },  // 200 + 30 (tax)
                { "payableAmount", 230.00m }
            }
        },

        // Tax total
        { "taxTotal", new Dictionary<string, object>
            {
                { "taxAmount", 30.00m }  // 15% of 200
            }
        },

        // Invoice lines
        { "invoiceLines", new List<object>
            {
                new Dictionary<string, object>
                {
                    { "id", "1" },
                    { "quantity", 2.0m },
                    { "lineExtensionAmount", 200.00m },  // 100 × 2
                    { "price", new Dictionary<string, object>
                        {
                            { "amount", 100.00m }
                        }
                    },
                    { "taxTotal", new Dictionary<string, object>
                        {
                            { "taxAmount", 30.00m },
                            { "roundingAmount", 230.00m }  // 200 + 30
                        }
                    },
                    { "item", new Dictionary<string, object>
                        {
                            { "name", "Product" },
                            { "taxPercent", 15.0m }  // Valid: 0-100
                        }
                    }
                }
            }
        }
    };

    var validator = new InvoiceAmountValidator();

    // Validate monetary totals
    var totalsResult = validator.ValidateMonetaryTotals(invoiceData);
    if (!totalsResult.IsValid)
    {
        Console.WriteLine("Monetary totals validation failed:");
        totalsResult.Errors.ForEach(e => Console.WriteLine($"- {e}"));
        return;
    }

    // Validate invoice lines
    var lines = (IList<object>)invoiceData["invoiceLines"];
    var linesResult = validator.ValidateInvoiceLines(lines);
    if (!linesResult.IsValid)
    {
        Console.WriteLine("Invoice lines validation failed:");
        linesResult.Errors.ForEach(e => Console.WriteLine($"- {e}"));
        return;
    }

    Console.WriteLine("All amount validations passed!");
}
```

## Best Practices

1. **Always validate before submission** - Use validators before calling ZATCA APIs
2. **Use ValidateAndThrow for critical paths** - Fail fast when validation errors should stop execution
3. **Use Validate for user input** - Collect all errors to show users comprehensive feedback
4. **Validate amounts separately** - Run both field and amount validation for complete coverage
5. **Handle validation results properly** - Check `IsValid` and process all errors in the `Errors` list
6. **Test with edge cases** - Test with zero amounts, maximum values, and boundary conditions
7. **Respect the tolerance** - Remember that amount comparisons use a 0.01 tolerance

## Related Documentation

- [Getting Started Guide](getting-started.md)
- [Invoice Submission](invoice-submission.md)
- [QR Code Generation](qr-code.md)
- [API Reference](api-reference.md)
