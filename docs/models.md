---
layout: default
title: Models Reference
nav_order: 8
description: "Data models documentation for Zatca.EInvoice"
---

# Models Reference

This document provides a comprehensive reference for all data models in the Zatca.EInvoice library. The models are organized into logical categories to help you understand their relationships and usage.

## Table of Contents

1. [Invoice Models](#invoice-models)
2. [Party Models](#party-models)
3. [Financial Models](#financial-models)
4. [Item Models](#item-models)
5. [Reference Models](#reference-models)
6. [Enumerations](#enumerations)

---

## Invoice Models

### Invoice

The main invoice model that represents a complete ZATCA-compliant e-invoice.

**Namespace:** `Zatca.EInvoice.Models`

#### Properties

| Property | Type | Description | Default Value |
|----------|------|-------------|---------------|
| `UblExtensions` | `UblExtensions?` | UBL extensions for signature information | `null` |
| `ProfileID` | `string` | Profile identifier for the invoice | `"reporting:1.0"` |
| `Id` | `string?` | Unique invoice identifier (required) | `null` |
| `UUID` | `string?` | Universal unique identifier for the invoice | `null` |
| `IssueDate` | `DateOnly?` | Date when the invoice was issued (required) | `null` |
| `IssueTime` | `TimeOnly?` | Time when the invoice was issued (required) | `null` |
| `InvoiceType` | `InvoiceType?` | Type and category of the invoice | `null` |
| `Note` | `string?` | Additional notes or comments | `null` |
| `LanguageID` | `string` | Language code for the invoice | `"en"` |
| `InvoiceCurrencyCode` | `string` | Currency code for invoice amounts | `"SAR"` |
| `TaxCurrencyCode` | `string` | Currency code for tax amounts | `"SAR"` |
| `DocumentCurrencyCode` | `string` | Currency code for the document | `"SAR"` |
| `OrderReference` | `OrderReference?` | Reference to the purchase order | `null` |
| `BillingReferences` | `List<BillingReference>?` | References to previous billing documents | `null` |
| `Contract` | `Contract?` | Contract reference | `null` |
| `AdditionalDocumentReferences` | `List<AdditionalDocumentReference>?` | Additional document references (required) | `null` |
| `AccountingSupplierParty` | `Party?` | Supplier party details (required) | `null` |
| `AccountingCustomerParty` | `Party?` | Customer party details (required) | `null` |
| `Delivery` | `Delivery?` | Delivery information | `null` |
| `PaymentMeans` | `PaymentMeans?` | Payment method details | `null` |
| `AllowanceCharges` | `List<AllowanceCharge>?` | Document-level allowances or charges | `null` |
| `TaxTotal` | `TaxTotal?` | Total tax information | `null` |
| `LegalMonetaryTotal` | `LegalMonetaryTotal?` | Legal monetary totals (required) | `null` |
| `InvoiceLines` | `List<InvoiceLine>?` | Line items in the invoice (required) | `null` |
| `Signature` | `Signature?` | Digital signature information | `null` |

#### Usage Example

```csharp
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Party;

var invoice = new Invoice
{
    Id = "INV-001",
    UUID = "6f4d20e0-6bfe-4a80-9389-7c5e4d659fa3",
    IssueDate = new DateOnly(2024, 1, 15),
    IssueTime = new TimeOnly(10, 30, 0),
    InvoiceType = new InvoiceType
    {
        Invoice = "standard",
        InvoiceSubType = "invoice"
    },
    AccountingSupplierParty = new Party
    {
        PartyIdentification = "123456789",
        LegalEntity = new LegalEntity { RegistrationName = "My Company" }
    },
    AccountingCustomerParty = new Party
    {
        PartyIdentification = "987654321",
        LegalEntity = new LegalEntity { RegistrationName = "Customer Company" }
    }
};

invoice.Validate(); // Throws exception if required fields are missing
```

#### Validation

Call the `Validate()` method to ensure all required fields are set before processing:

```csharp
invoice.Validate();
```

Required fields:
- `Id`
- `IssueDate`
- `IssueTime`
- `AccountingSupplierParty`
- `AccountingCustomerParty`
- `AdditionalDocumentReferences` (at least one)
- `InvoiceLines` (at least one)
- `LegalMonetaryTotal`

---

### InvoiceLine

Represents a line item in an invoice.

**Namespace:** `Zatca.EInvoice.Models`

#### Properties

| Property | Type | Description | Default Value |
|----------|------|-------------|---------------|
| `Id` | `string?` | Line item identifier | `null` |
| `InvoicedQuantity` | `decimal?` | Quantity of items invoiced | `null` |
| `LineExtensionAmount` | `decimal?` | Total amount for the line (before tax) | `null` |
| `AllowanceCharges` | `List<AllowanceCharge>?` | Line-level allowances or charges | `null` |
| `DocumentReference` | `DocumentReference?` | Reference to related documents | `null` |
| `UnitCode` | `string` | Unit of measure code | `"MON"` |
| `TaxTotal` | `TaxTotal?` | Tax details for the line | `null` |
| `InvoicePeriod` | `InvoicePeriod?` | Period covered by this line | `null` |
| `Note` | `string?` | Additional notes for the line | `null` |
| `Item` | `Item?` | Item details | `null` |
| `Price` | `Price?` | Pricing information | `null` |
| `AccountingCostCode` | `string?` | Accounting cost code | `null` |
| `AccountingCost` | `string?` | Accounting cost description | `null` |

#### Usage Example

```csharp
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.Enums;

var invoiceLine = new InvoiceLine
{
    Id = "1",
    InvoicedQuantity = 10,
    LineExtensionAmount = 1000.00m,
    UnitCode = UnitCode.UNIT,
    Item = new Item
    {
        Name = "Product ABC",
        Description = "High-quality product"
    },
    Price = new Price
    {
        PriceAmount = 100.00m,
        BaseQuantity = 1
    }
};
```

#### Relationships

- Contains an `Item` (product/service details)
- Contains a `Price` (pricing information)
- May reference a `TaxTotal` for line-level taxes
- May have multiple `AllowanceCharge` objects

---

### InvoiceType

Defines the type and category of an invoice.

**Namespace:** `Zatca.EInvoice.Models`

#### Properties

| Property | Type | Description | Default Value |
|----------|------|-------------|---------------|
| `Invoice` | `string?` | Main invoice category ("standard" or "simplified") | `null` |
| `InvoiceSubType` | `string?` | Sub-type ("invoice", "debit", "credit", or "prepayment") | `null` |
| `IsExportInvoice` | `bool` | Indicates if this is an export invoice | `false` |
| `IsThirdParty` | `bool` | Indicates third-party transaction | `false` |
| `IsNominal` | `bool` | Indicates nominal transaction | `false` |
| `IsSummary` | `bool` | Indicates summary invoice | `false` |
| `IsSelfBilled` | `bool` | Indicates self-billed invoice | `false` |

#### Methods

- `GetInvoiceTypeCode()`: Returns the numeric invoice type code (388, 381, 383, or 386)
- `GetInvoiceTypeValue()`: Returns the complete invoice type value with flags (e.g., "0100000", "0200000")
- `Validate()`: Validates that invoice type is properly configured

#### Usage Example

```csharp
using Zatca.EInvoice.Models;

var invoiceType = new InvoiceType
{
    Invoice = "standard",
    InvoiceSubType = "invoice",
    IsExportInvoice = false,
    IsThirdParty = false
};

int typeCode = invoiceType.GetInvoiceTypeCode(); // Returns 388
string typeValue = invoiceType.GetInvoiceTypeValue(); // Returns "0100000"
```

#### Type Code Mapping

| Sub-Type | Code |
|----------|------|
| invoice | 388 |
| debit | 383 |
| credit | 381 |
| prepayment | 386 |

#### Type Value Format

The type value is a 7-character string: `[XX][P][N][E][S][B]`

- `XX`: "01" for standard, "02" for simplified
- `P`: Third-party flag (0 or 1)
- `N`: Nominal flag (0 or 1)
- `E`: Export flag (0 or 1)
- `S`: Summary flag (0 or 1)
- `B`: Self-billed flag (0 or 1)

---

## Party Models

### Party

Represents a party (supplier or customer) in an invoice.

**Namespace:** `Zatca.EInvoice.Models.Party`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `PartyIdentification` | `string?` | Party identification value |
| `PartyIdentificationId` | `string?` | Party identification scheme identifier |
| `PostalAddress` | `Address?` | Postal address details |
| `PartyTaxScheme` | `PartyTaxScheme?` | Tax scheme information |
| `LegalEntity` | `LegalEntity?` | Legal entity details |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Party;
using Zatca.EInvoice.Models.Financial;

var party = new Party
{
    PartyIdentification = "123456789",
    PartyIdentificationId = "CRN",
    PostalAddress = new Address
    {
        StreetName = "King Fahd Road",
        BuildingNumber = "1234",
        CityName = "Riyadh",
        PostalZone = "12345",
        Country = "SA"
    },
    PartyTaxScheme = new PartyTaxScheme
    {
        CompanyId = "300000000000003",
        TaxScheme = new TaxScheme { Id = "VAT" }
    },
    LegalEntity = new LegalEntity
    {
        RegistrationName = "My Company Ltd"
    }
};
```

#### Relationships

- Contains an `Address` (postal address)
- Contains a `PartyTaxScheme` (tax registration)
- Contains a `LegalEntity` (legal name)

---

### Address

Represents a postal address.

**Namespace:** `Zatca.EInvoice.Models.Party`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `StreetName` | `string?` | Street name |
| `AdditionalStreetName` | `string?` | Additional street information |
| `BuildingNumber` | `string?` | Building number |
| `PlotIdentification` | `string?` | Plot or parcel identification |
| `CityName` | `string?` | City name |
| `PostalZone` | `string?` | Postal code |
| `Country` | `string?` | Country code (ISO 3166-1 alpha-2) |
| `CountrySubentity` | `string?` | State or province |
| `CitySubdivisionName` | `string?` | District or neighborhood |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Party;

var address = new Address
{
    StreetName = "King Fahd Road",
    AdditionalStreetName = "Near City Center",
    BuildingNumber = "1234",
    PlotIdentification = "5678",
    CityName = "Riyadh",
    PostalZone = "12345",
    CountrySubentity = "Riyadh Region",
    CitySubdivisionName = "Al Olaya",
    Country = "SA"
};
```

---

### LegalEntity

Represents legal entity information.

**Namespace:** `Zatca.EInvoice.Models.Party`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `RegistrationName` | `string?` | Legal registration name of the entity |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Party;

var legalEntity = new LegalEntity
{
    RegistrationName = "My Company Limited"
};
```

---

### PartyTaxScheme

Represents party tax scheme information.

**Namespace:** `Zatca.EInvoice.Models.Party`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CompanyId` | `string?` | Tax registration number (VAT number) |
| `TaxScheme` | `TaxScheme?` | Tax scheme details |

#### Methods

- `Validate()`: Validates that TaxScheme is set

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Party;
using Zatca.EInvoice.Models.Financial;

var partyTaxScheme = new PartyTaxScheme
{
    CompanyId = "300000000000003",
    TaxScheme = new TaxScheme
    {
        Id = "VAT",
        Name = "Value Added Tax"
    }
};

partyTaxScheme.Validate();
```

---

## Financial Models

### TaxTotal

Represents the total tax details for an invoice or line.

**Namespace:** `Zatca.EInvoice.Models.Financial`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TaxAmount` | `decimal?` | Total tax amount (required) |
| `RoundingAmount` | `decimal?` | Rounding adjustment amount |
| `TaxSubTotals` | `List<TaxSubTotal>` | List of tax subtotals by category |

#### Methods

- `AddTaxSubTotal(TaxSubTotal)`: Adds a tax subtotal to the list
- `Validate()`: Validates that TaxAmount is set

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Financial;

var taxTotal = new TaxTotal
{
    TaxAmount = 150.00m,
    RoundingAmount = 0.00m
};

taxTotal.AddTaxSubTotal(new TaxSubTotal
{
    TaxableAmount = 1000.00m,
    TaxAmount = 150.00m,
    Percent = 15.00m,
    TaxCategory = new TaxCategory
    {
        Id = "S",
        Percent = 15.00m,
        TaxScheme = new TaxScheme { Id = "VAT" }
    }
});

taxTotal.Validate();
```

---

### TaxSubTotal

Represents a tax subtotal for a specific tax category.

**Namespace:** `Zatca.EInvoice.Models.Financial`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TaxableAmount` | `decimal?` | Amount subject to tax (required) |
| `TaxAmount` | `decimal?` | Calculated tax amount (required) |
| `TaxCategory` | `TaxCategory?` | Tax category details (required) |
| `Percent` | `decimal?` | Tax percentage rate |

#### Methods

- `Validate()`: Validates required fields

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Financial;

var taxSubTotal = new TaxSubTotal
{
    TaxableAmount = 1000.00m,
    TaxAmount = 150.00m,
    Percent = 15.00m,
    TaxCategory = new TaxCategory
    {
        Id = "S",
        Percent = 15.00m,
        TaxScheme = new TaxScheme { Id = "VAT" }
    }
};

taxSubTotal.Validate();
```

---

### TaxCategory

Represents a tax category with rate and exemption information.

**Namespace:** `Zatca.EInvoice.Models.Financial`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string?` | Tax category code (auto-derived from Percent if not set) |
| `IdAttributes` | `Dictionary<string, string>` | ID attributes (schemeID, schemeAgencyID) |
| `Name` | `string?` | Tax category name |
| `Percent` | `decimal?` | Tax percentage rate |
| `TaxScheme` | `TaxScheme?` | Tax scheme details |
| `TaxSchemeAttributes` | `Dictionary<string, string>` | Tax scheme attributes |
| `TaxExemptionReason` | `string?` | Reason for tax exemption |
| `TaxExemptionReasonCode` | `string?` | Code for tax exemption reason |

#### Constants

- `UNCL5305`: "UN/ECE 5305"
- `UNCL5153`: "UN/ECE 5153"

#### Methods

- `Validate()`: Validates required fields

#### Auto-Derived ID

If `Id` is not explicitly set, it is automatically derived from `Percent`:
- 15% or higher: "S" (Standard rate)
- 6% to <15%: "AA" (Reduced rate)
- Below 6%: "Z" (Zero rate)

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Financial;

var taxCategory = new TaxCategory
{
    Percent = 15.00m, // Id will be auto-derived as "S"
    TaxScheme = new TaxScheme
    {
        Id = "VAT",
        Name = "Value Added Tax"
    }
};

Console.WriteLine(taxCategory.Id); // Outputs: "S"
taxCategory.Validate();
```

#### Exemption Example

```csharp
var exemptCategory = new TaxCategory
{
    Id = "E",
    Percent = 0.00m,
    TaxExemptionReasonCode = "VATEX-SA-29",
    TaxExemptionReason = "Financial services",
    TaxScheme = new TaxScheme { Id = "VAT" }
};
```

---

### TaxScheme

Represents a tax scheme identifier.

**Namespace:** `Zatca.EInvoice.Models.Financial`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string?` | Tax scheme identifier (e.g., "VAT") |
| `TaxTypeCode` | `string?` | Tax type code |
| `Name` | `string?` | Tax scheme name |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Financial;

var taxScheme = new TaxScheme
{
    Id = "VAT",
    Name = "Value Added Tax"
};
```

---

### LegalMonetaryTotal

Represents the monetary totals for an invoice.

**Namespace:** `Zatca.EInvoice.Models.Financial`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `LineExtensionAmount` | `decimal?` | Sum of line amounts (before allowances/charges) |
| `TaxExclusiveAmount` | `decimal?` | Total excluding tax |
| `TaxInclusiveAmount` | `decimal?` | Total including tax |
| `AllowanceTotalAmount` | `decimal?` | Total allowances (defaults to 0.0) |
| `ChargeTotalAmount` | `decimal?` | Total charges (defaults to 0.0) |
| `PrepaidAmount` | `decimal?` | Amount already paid |
| `PayableAmount` | `decimal?` | Amount due for payment |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Financial;

var monetaryTotal = new LegalMonetaryTotal
{
    LineExtensionAmount = 1000.00m,
    TaxExclusiveAmount = 1000.00m,
    TaxInclusiveAmount = 1150.00m,
    AllowanceTotalAmount = 0.00m,
    ChargeTotalAmount = 0.00m,
    PrepaidAmount = 0.00m,
    PayableAmount = 1150.00m
};
```

#### Calculation Example

```csharp
// Basic calculation
decimal lineTotal = 1000.00m;
decimal allowances = 50.00m;
decimal charges = 30.00m;
decimal taxExclusive = lineTotal - allowances + charges; // 980.00
decimal tax = taxExclusive * 0.15m; // 147.00
decimal taxInclusive = taxExclusive + tax; // 1127.00

var monetaryTotal = new LegalMonetaryTotal
{
    LineExtensionAmount = lineTotal,
    AllowanceTotalAmount = allowances,
    ChargeTotalAmount = charges,
    TaxExclusiveAmount = taxExclusive,
    TaxInclusiveAmount = taxInclusive,
    PayableAmount = taxInclusive
};
```

---

### AllowanceCharge

Represents an allowance (discount) or charge (additional fee).

**Namespace:** `Zatca.EInvoice.Models.Financial`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ChargeIndicator` | `bool` | True for charge, false for allowance |
| `AllowanceChargeReasonCode` | `string?` | Code for the reason |
| `AllowanceChargeReason` | `string?` | Description of the reason |
| `MultiplierFactorNumeric` | `int?` | Multiplier factor (for percentage-based) |
| `BaseAmount` | `decimal?` | Base amount for calculation |
| `Amount` | `decimal?` | Allowance or charge amount |
| `TaxTotal` | `TaxTotal?` | Tax information for the allowance/charge |
| `TaxCategories` | `List<TaxCategory>?` | Applicable tax categories |

#### Usage Example - Discount

```csharp
using Zatca.EInvoice.Models.Financial;

var discount = new AllowanceCharge
{
    ChargeIndicator = false, // This is an allowance (discount)
    AllowanceChargeReasonCode = "95",
    AllowanceChargeReason = "Volume discount",
    MultiplierFactorNumeric = 10, // 10%
    BaseAmount = 1000.00m,
    Amount = 100.00m // 10% of 1000
};
```

#### Usage Example - Charge

```csharp
var shippingCharge = new AllowanceCharge
{
    ChargeIndicator = true, // This is a charge
    AllowanceChargeReasonCode = "FC",
    AllowanceChargeReason = "Shipping and handling",
    Amount = 50.00m
};
```

---

## Item Models

### Item

Represents an item (product or service) in an invoice line.

**Namespace:** `Zatca.EInvoice.Models.Items`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Description` | `string?` | Item description |
| `Name` | `string?` | Item name (required) |
| `StandardItemIdentification` | `string?` | Standard product code (e.g., GTIN, EAN) |
| `BuyersItemIdentification` | `string?` | Buyer's item code |
| `SellersItemIdentification` | `string?` | Seller's item code |
| `ClassifiedTaxCategories` | `List<ClassifiedTaxCategory>?` | Tax categories for the item |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.Financial;

var item = new Item
{
    Name = "Laptop Computer",
    Description = "15-inch business laptop with 16GB RAM",
    StandardItemIdentification = "0123456789012", // EAN-13
    SellersItemIdentification = "LAP-001",
    ClassifiedTaxCategories = new List<ClassifiedTaxCategory>
    {
        new ClassifiedTaxCategory
        {
            Percent = 15.00m,
            TaxScheme = new TaxScheme { Id = "VAT" }
        }
    }
};
```

#### Relationships

- May have multiple `ClassifiedTaxCategory` objects
- Used within `InvoiceLine`

---

### Price

Represents pricing information for an invoice line.

**Namespace:** `Zatca.EInvoice.Models.Items`

#### Properties

| Property | Type | Description | Default Value |
|----------|------|-------------|---------------|
| `PriceAmount` | `decimal?` | Unit price (required) | `null` |
| `BaseQuantity` | `decimal?` | Base quantity for price | `null` |
| `UnitCode` | `string` | Unit of measure code | `UnitCode.UNIT` |
| `AllowanceCharges` | `List<AllowanceCharge>?` | Price-level allowances/charges | `null` |

#### Methods

- `Validate()`: Validates that PriceAmount is set

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.Enums;

var price = new Price
{
    PriceAmount = 100.00m,
    BaseQuantity = 1,
    UnitCode = UnitCode.UNIT
};

price.Validate();
```

#### With Allowance Example

```csharp
using Zatca.EInvoice.Models.Financial;

var price = new Price
{
    PriceAmount = 90.00m, // Price after discount
    BaseQuantity = 1,
    AllowanceCharges = new List<AllowanceCharge>
    {
        new AllowanceCharge
        {
            ChargeIndicator = false,
            Amount = 10.00m,
            AllowanceChargeReason = "Unit price discount"
        }
    }
};
```

---

### ClassifiedTaxCategory

Represents a tax category classification for an item.

**Namespace:** `Zatca.EInvoice.Models.Items`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string?` | Tax category code (auto-derived from Percent) |
| `Name` | `string?` | Tax category name |
| `Percent` | `decimal?` | Tax percentage rate |
| `TaxScheme` | `TaxScheme?` | Tax scheme details |
| `SchemeID` | `string?` | Scheme identifier |
| `SchemeName` | `string?` | Scheme name |
| `TaxExemptionReason` | `string?` | Exemption reason |
| `TaxExemptionReasonCode` | `string?` | Exemption reason code |

#### Constants

- `UNCL5305`: "UNCL5305"

#### Methods

- `Validate()`: Validates required fields

#### Auto-Derived ID

Similar to `TaxCategory`, the `Id` is auto-derived from `Percent`:
- 15% or higher: "S"
- 6% to <15%: "AA"
- Below 6%: "Z"

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.Financial;

var classifiedTaxCategory = new ClassifiedTaxCategory
{
    Percent = 15.00m, // Id will be "S"
    TaxScheme = new TaxScheme
    {
        Id = "VAT",
        Name = "Value Added Tax"
    }
};

classifiedTaxCategory.Validate();
```

---

## Reference Models

### AdditionalDocumentReference

Represents an additional document reference (e.g., QR code, invoice hash).

**Namespace:** `Zatca.EInvoice.Models.References`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string?` | Document reference identifier |
| `UUID` | `string?` | Document UUID |
| `DocumentType` | `string?` | Type of document |
| `DocumentTypeCode` | `int?` | Document type code |
| `DocumentDescription` | `string?` | Document description |
| `Attachment` | `Attachment?` | Attached file or content |

#### Usage Example - QR Code

```csharp
using Zatca.EInvoice.Models.References;

var qrCodeReference = new AdditionalDocumentReference
{
    Id = "QR",
    UUID = "6f4d20e0-6bfe-4a80-9389-7c5e4d659fa3",
    Attachment = new Attachment
    {
        Base64Content = "VGhpcyBpcyBhIHNhbXBsZSBRUiBjb2Rl",
        MimeType = "text/plain",
        FileName = "qrcode.txt"
    }
};
```

#### Usage Example - Invoice Hash

```csharp
var hashReference = new AdditionalDocumentReference
{
    Id = "ICV",
    UUID = "123",
    DocumentType = "Invoice Counter Value"
};
```

---

### BillingReference

Represents a reference to a previous billing document.

**Namespace:** `Zatca.EInvoice.Models.References`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string?` | Billing reference identifier |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.References;

var billingReference = new BillingReference
{
    Id = "INV-2024-001"
};
```

---

### OrderReference

Represents a purchase order reference.

**Namespace:** `Zatca.EInvoice.Models.References`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string?` | Order reference identifier |
| `SalesOrderId` | `string?` | Sales order identifier |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.References;

var orderReference = new OrderReference
{
    Id = "PO-2024-12345",
    SalesOrderId = "SO-2024-67890"
};
```

---

### Delivery

Represents delivery information.

**Namespace:** `Zatca.EInvoice.Models.References`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ActualDeliveryDate` | `DateOnly?` | Actual delivery date |
| `LatestDeliveryDate` | `DateOnly?` | Latest possible delivery date |
| `DeliveryLocation` | `Address?` | Delivery address |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.References;
using Zatca.EInvoice.Models.Party;

var delivery = new Delivery
{
    ActualDeliveryDate = new DateOnly(2024, 1, 20),
    DeliveryLocation = new Address
    {
        StreetName = "Main Street",
        BuildingNumber = "100",
        CityName = "Jeddah",
        PostalZone = "23000",
        Country = "SA"
    }
};
```

---

### PaymentMeans

Represents payment method information.

**Namespace:** `Zatca.EInvoice.Models.References`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `PaymentMeansCode` | `string?` | Payment method code (e.g., "10" for cash, "30" for credit transfer) |
| `InstructionNote` | `string?` | Payment instructions |
| `PaymentId` | `string?` | Payment identifier |
| `PayeeFinancialAccount` | `object?` | Financial account details |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.References;

var paymentMeans = new PaymentMeans
{
    PaymentMeansCode = "30", // Credit transfer
    InstructionNote = "Payment within 30 days",
    PaymentId = "PAY-2024-001"
};
```

#### Common Payment Means Codes

| Code | Description |
|------|-------------|
| 1 | Instrument not defined |
| 10 | Cash |
| 30 | Credit transfer |
| 42 | Payment to bank account |
| 48 | Bank card |

---

### Contract

Represents a contract reference.

**Namespace:** `Zatca.EInvoice.Models.References`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string?` | Contract identifier |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.References;

var contract = new Contract
{
    Id = "CONTRACT-2024-001"
};
```

---

### DocumentReference

Represents a general document reference.

**Namespace:** `Zatca.EInvoice.Models.References`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string?` | Document reference identifier |
| `UUID` | `string?` | Document UUID |
| `IssueDate` | `DateOnly?` | Document issue date |
| `DocumentTypeCode` | `string?` | Document type code |
| `DocumentDescription` | `string?` | Document description |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.References;

var documentRef = new DocumentReference
{
    Id = "DOC-2024-001",
    UUID = "6f4d20e0-6bfe-4a80-9389-7c5e4d659fa3",
    IssueDate = new DateOnly(2024, 1, 1),
    DocumentTypeCode = "130",
    DocumentDescription = "Proforma Invoice"
};
```

---

### InvoicePeriod

Represents a billing period.

**Namespace:** `Zatca.EInvoice.Models.References`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `StartDate` | `DateOnly?` | Period start date |
| `EndDate` | `DateOnly?` | Period end date |
| `Description` | `string?` | Period description |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.References;

var invoicePeriod = new InvoicePeriod
{
    StartDate = new DateOnly(2024, 1, 1),
    EndDate = new DateOnly(2024, 1, 31),
    Description = "January 2024 billing period"
};
```

---

### Attachment

Represents a file attachment.

**Namespace:** `Zatca.EInvoice.Models.References`

#### Properties

| Property | Type | Description | Default Value |
|----------|------|-------------|---------------|
| `FilePath` | `string?` | Path to file on disk | `null` |
| `ExternalReference` | `string?` | External URL reference | `null` |
| `Base64Content` | `string?` | Base64-encoded content | `null` |
| `EmbeddedDocumentBinaryObject` | `string?` | Alias for Base64Content | `null` |
| `FileName` | `string?` | File name | `null` |
| `MimeType` | `string?` | MIME type (e.g., "application/pdf") | `null` |
| `MimeCode` | `string` | Encoding type | `"base64"` |

#### Methods

- `SetBase64Content(string base64Content, string fileName, string? mimeType)`: Sets content with metadata
- `Validate()`: Validates attachment data

#### Usage Example - Base64 Content

```csharp
using Zatca.EInvoice.Models.References;

var attachment = new Attachment();
attachment.SetBase64Content(
    "VGhpcyBpcyBhIHNhbXBsZSBQREYgZmlsZQ==",
    "invoice.pdf",
    "application/pdf"
);

attachment.Validate();
```

#### Usage Example - File Path

```csharp
var attachment = new Attachment
{
    FilePath = "/path/to/document.pdf",
    MimeType = "application/pdf"
};

attachment.Validate(); // Checks if file exists
```

#### Usage Example - External Reference

```csharp
var attachment = new Attachment
{
    ExternalReference = "https://example.com/documents/invoice-001.pdf"
};
```

---

## Enumerations

### InvoiceCategory

Invoice category enumeration.

**Namespace:** `Zatca.EInvoice.Models.Enums`

#### Values

| Value | Description |
|-------|-------------|
| `Standard` | Standard invoice (B2B) |
| `Simplified` | Simplified invoice (B2C) |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Enums;

var category = InvoiceCategory.Standard;
```

---

### InvoiceSubType

Invoice sub-type enumeration.

**Namespace:** `Zatca.EInvoice.Models.Enums`

#### Values

| Value | Description |
|-------|-------------|
| `Invoice` | Standard invoice |
| `Debit` | Debit note |
| `Credit` | Credit note |
| `Prepayment` | Prepayment invoice |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Enums;

var subType = InvoiceSubType.Invoice;
```

---

### InvoiceTypeCode

Invoice type code constants.

**Namespace:** `Zatca.EInvoice.Models.Enums`

#### Constants

| Constant | Value | Description |
|----------|-------|-------------|
| `INVOICE` | 388 | Invoice type code |
| `CREDIT_NOTE` | 381 | Credit note type code |
| `DEBIT_NOTE` | 383 | Debit note type code |
| `PREPAYMENT` | 386 | Prepayment type code |
| `STANDARD_INVOICE` | "0100000" | Standard invoice type value |
| `SIMPLIFIED_INVOICE` | "0200000" | Simplified invoice type value |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Enums;

int invoiceCode = InvoiceTypeCode.INVOICE; // 388
string standardType = InvoiceTypeCode.STANDARD_INVOICE; // "0100000"
```

---

### UnitCode

Unit of measure code constants.

**Namespace:** `Zatca.EInvoice.Models.Enums`

#### Constants

| Constant | Value | Description |
|----------|-------|-------------|
| `UNIT` | "C62" | Unit (default) |
| `PIECE` | "H87" | Piece |
| `MON` | "MON" | Month |
| `PCE` | "PCE" | Piece (alternative) |

#### Usage Example

```csharp
using Zatca.EInvoice.Models.Enums;

string unitCode = UnitCode.UNIT; // "C62"
```

#### Additional Units

For more unit codes, refer to:
- [UNECE Recommendation 20](http://tfig.unece.org/contents/recommendation-20.htm)
- [UNECE Document ZIP](http://www.unece.org/fileadmin/DAM/cefact/recommendations/rec20/rec20_Rev7e_2010.zip)

---

## Model Relationships

### Invoice Structure

```
Invoice
├── InvoiceType
├── AccountingSupplierParty (Party)
│   ├── PostalAddress (Address)
│   ├── PartyTaxScheme
│   │   └── TaxScheme
│   └── LegalEntity
├── AccountingCustomerParty (Party)
│   └── [Same structure as supplier]
├── AdditionalDocumentReferences
│   └── Attachment
├── TaxTotal
│   └── TaxSubTotals
│       └── TaxCategory
│           └── TaxScheme
├── LegalMonetaryTotal
├── AllowanceCharges
│   ├── TaxTotal
│   └── TaxCategories
└── InvoiceLines
    ├── Item
    │   └── ClassifiedTaxCategories
    │       └── TaxScheme
    ├── Price
    │   └── AllowanceCharges
    ├── TaxTotal
    └── AllowanceCharges
```

---

## Complete Invoice Example

Here's a complete example showing all major models working together:

```csharp
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Party;
using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.References;
using Zatca.EInvoice.Models.Enums;

// Create supplier party
var supplier = new Party
{
    PartyIdentification = "123456789",
    PartyIdentificationId = "CRN",
    PostalAddress = new Address
    {
        StreetName = "King Fahd Road",
        BuildingNumber = "1234",
        CityName = "Riyadh",
        PostalZone = "12345",
        Country = "SA"
    },
    PartyTaxScheme = new PartyTaxScheme
    {
        CompanyId = "300000000000003",
        TaxScheme = new TaxScheme { Id = "VAT" }
    },
    LegalEntity = new LegalEntity
    {
        RegistrationName = "My Company Ltd"
    }
};

// Create customer party
var customer = new Party
{
    PartyIdentification = "987654321",
    PostalAddress = new Address
    {
        StreetName = "Main Street",
        BuildingNumber = "5678",
        CityName = "Jeddah",
        PostalZone = "23000",
        Country = "SA"
    },
    PartyTaxScheme = new PartyTaxScheme
    {
        CompanyId = "300000000000004",
        TaxScheme = new TaxScheme { Id = "VAT" }
    },
    LegalEntity = new LegalEntity
    {
        RegistrationName = "Customer Company Ltd"
    }
};

// Create invoice line
var invoiceLine = new InvoiceLine
{
    Id = "1",
    InvoicedQuantity = 10,
    LineExtensionAmount = 1000.00m,
    UnitCode = UnitCode.UNIT,
    Item = new Item
    {
        Name = "Laptop Computer",
        Description = "15-inch business laptop",
        ClassifiedTaxCategories = new List<ClassifiedTaxCategory>
        {
            new ClassifiedTaxCategory
            {
                Percent = 15.00m,
                TaxScheme = new TaxScheme { Id = "VAT" }
            }
        }
    },
    Price = new Price
    {
        PriceAmount = 100.00m,
        BaseQuantity = 1,
        UnitCode = UnitCode.UNIT
    },
    TaxTotal = new TaxTotal
    {
        TaxAmount = 150.00m,
        TaxSubTotals = new List<TaxSubTotal>
        {
            new TaxSubTotal
            {
                TaxableAmount = 1000.00m,
                TaxAmount = 150.00m,
                Percent = 15.00m,
                TaxCategory = new TaxCategory
                {
                    Id = "S",
                    Percent = 15.00m,
                    TaxScheme = new TaxScheme { Id = "VAT" }
                }
            }
        }
    }
};

// Create complete invoice
var invoice = new Invoice
{
    Id = "INV-2024-001",
    UUID = Guid.NewGuid().ToString(),
    IssueDate = DateOnly.FromDateTime(DateTime.Now),
    IssueTime = TimeOnly.FromDateTime(DateTime.Now),
    InvoiceType = new InvoiceType
    {
        Invoice = "standard",
        InvoiceSubType = "invoice"
    },
    AccountingSupplierParty = supplier,
    AccountingCustomerParty = customer,
    InvoiceLines = new List<InvoiceLine> { invoiceLine },
    TaxTotal = new TaxTotal
    {
        TaxAmount = 150.00m,
        TaxSubTotals = new List<TaxSubTotal>
        {
            new TaxSubTotal
            {
                TaxableAmount = 1000.00m,
                TaxAmount = 150.00m,
                Percent = 15.00m,
                TaxCategory = new TaxCategory
                {
                    Id = "S",
                    Percent = 15.00m,
                    TaxScheme = new TaxScheme { Id = "VAT" }
                }
            }
        }
    },
    LegalMonetaryTotal = new LegalMonetaryTotal
    {
        LineExtensionAmount = 1000.00m,
        TaxExclusiveAmount = 1000.00m,
        TaxInclusiveAmount = 1150.00m,
        PayableAmount = 1150.00m
    },
    AdditionalDocumentReferences = new List<AdditionalDocumentReference>
    {
        new AdditionalDocumentReference
        {
            Id = "ICV",
            UUID = "1"
        }
    }
};

// Validate the invoice
invoice.Validate();
```

---

## Best Practices

1. **Always Validate**: Call the `Validate()` method on models that support it before processing
2. **Use Auto-Derivation**: Take advantage of auto-derived fields like `TaxCategory.Id` when appropriate
3. **Set Defaults**: Many fields have sensible defaults (like currency codes), but review them for your use case
4. **Check Required Fields**: Pay attention to which fields are required vs. optional
5. **Use Type Safety**: Leverage the strongly-typed models instead of working with raw data
6. **Follow ZATCA Guidelines**: Ensure your model usage complies with ZATCA e-invoicing requirements

---

## Related Documentation

- [Getting Started](getting-started.md) - Learn how to set up and use the library
- [Invoice Generation](invoice-generation.md) - Detailed guide on creating invoices
- [API Reference](api-reference.md) - Complete API documentation
- [Certificates](certificates.md) - Certificate management guide
