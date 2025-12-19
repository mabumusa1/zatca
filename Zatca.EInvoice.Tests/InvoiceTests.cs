using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.Party;
using Zatca.EInvoice.Models.References;
using Zatca.EInvoice.Models.Signature;
using Zatca.EInvoice.Tests.TestFixtures;

namespace Zatca.EInvoice.Tests;

/// <summary>
/// Tests for the Invoice model and XML generation functionality.
/// </summary>
public class InvoiceTests
{
    /// <summary>
    /// Test that an Invoice object can be created with valid data.
    /// </summary>
    [Fact]
    public void TestInvoiceCreation()
    {
        // Arrange & Act
        var invoice = new Invoice
        {
            UUID = "3cf5ee18-ee25-44ea-a444-2c37ba7f28be",
            Id = "SME00023",
            IssueDate = new DateOnly(2024, 9, 7),
            IssueTime = new TimeOnly(17, 41, 8),
            InvoiceCurrencyCode = "SAR",
            TaxCurrencyCode = "SAR"
        };

        // Assert
        invoice.Should().NotBeNull();
        invoice.UUID.Should().Be("3cf5ee18-ee25-44ea-a444-2c37ba7f28be");
        invoice.Id.Should().Be("SME00023");
        invoice.IssueDate.Should().Be(new DateOnly(2024, 9, 7));
        invoice.IssueTime.Should().Be(new TimeOnly(17, 41, 8));
        invoice.InvoiceCurrencyCode.Should().Be("SAR");
        invoice.TaxCurrencyCode.Should().Be("SAR");
    }

    /// <summary>
    /// Test that Invoice validation detects missing required fields.
    /// </summary>
    [Fact]
    public void TestInvoiceValidationMissingId()
    {
        // Arrange
        var invoice = new Invoice
        {
            IssueDate = new DateOnly(2024, 9, 7),
            IssueTime = new TimeOnly(17, 41, 8)
        };

        // Act & Assert
        Action act = () => invoice.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*invoice id*");
    }

    /// <summary>
    /// Test that Invoice validation detects missing issue date.
    /// </summary>
    [Fact]
    public void TestInvoiceValidationMissingIssueDate()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = "SME00023",
            IssueTime = new TimeOnly(17, 41, 8)
        };

        // Act & Assert
        Action act = () => invoice.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*issueDate*");
    }

    /// <summary>
    /// Test that Invoice validation detects missing issue time.
    /// </summary>
    [Fact]
    public void TestInvoiceValidationMissingIssueTime()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = "SME00023",
            IssueDate = new DateOnly(2024, 9, 7)
        };

        // Act & Assert
        Action act = () => invoice.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*issueTime*");
    }

    /// <summary>
    /// Test that Invoice validation detects missing supplier.
    /// </summary>
    [Fact]
    public void TestInvoiceValidationMissingSupplier()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = "SME00023",
            IssueDate = new DateOnly(2024, 9, 7),
            IssueTime = new TimeOnly(17, 41, 8),
            AccountingCustomerParty = new Party(),
            AdditionalDocumentReferences = new List<AdditionalDocumentReference> { new AdditionalDocumentReference() },
            InvoiceLines = new List<InvoiceLine> { new InvoiceLine() },
            LegalMonetaryTotal = new LegalMonetaryTotal()
        };

        // Act & Assert
        Action act = () => invoice.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*accountingSupplierParty*");
    }

    /// <summary>
    /// Test that Invoice validation detects missing customer.
    /// </summary>
    [Fact]
    public void TestInvoiceValidationMissingCustomer()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = "SME00023",
            IssueDate = new DateOnly(2024, 9, 7),
            IssueTime = new TimeOnly(17, 41, 8),
            AccountingSupplierParty = new Party(),
            AdditionalDocumentReferences = new List<AdditionalDocumentReference> { new AdditionalDocumentReference() },
            InvoiceLines = new List<InvoiceLine> { new InvoiceLine() },
            LegalMonetaryTotal = new LegalMonetaryTotal()
        };

        // Act & Assert
        Action act = () => invoice.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*accountingCustomerParty*");
    }

    /// <summary>
    /// Test that Invoice validation detects missing invoice lines.
    /// </summary>
    [Fact]
    public void TestInvoiceValidationMissingInvoiceLines()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = "SME00023",
            IssueDate = new DateOnly(2024, 9, 7),
            IssueTime = new TimeOnly(17, 41, 8),
            AccountingSupplierParty = new Party(),
            AccountingCustomerParty = new Party(),
            AdditionalDocumentReferences = new List<AdditionalDocumentReference> { new AdditionalDocumentReference() },
            LegalMonetaryTotal = new LegalMonetaryTotal()
        };

        // Act & Assert
        Action act = () => invoice.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*invoice lines*");
    }

    /// <summary>
    /// Test that Invoice validation detects missing legal monetary total.
    /// </summary>
    [Fact]
    public void TestInvoiceValidationMissingLegalMonetaryTotal()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = "SME00023",
            IssueDate = new DateOnly(2024, 9, 7),
            IssueTime = new TimeOnly(17, 41, 8),
            AccountingSupplierParty = new Party(),
            AccountingCustomerParty = new Party(),
            AdditionalDocumentReferences = new List<AdditionalDocumentReference> { new AdditionalDocumentReference() },
            InvoiceLines = new List<InvoiceLine> { new InvoiceLine() }
        };

        // Act & Assert
        Action act = () => invoice.Validate();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*LegalMonetaryTotal*");
    }

    /// <summary>
    /// Test that a complete Invoice passes validation.
    /// </summary>
    [Fact]
    public void TestInvoiceValidation()
    {
        // Arrange
        var invoice = CreateCompleteInvoice();

        // Act & Assert - Should not throw
        Action act = () => invoice.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Test that empty UUID throws exception.
    /// </summary>
    [Fact]
    public void TestEmptyUuidThrowsException()
    {
        // Arrange
        var invoice = new Invoice();

        // Act & Assert
        Action act = () => invoice.UUID = "";
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UUID*");
    }

    /// <summary>
    /// Test that empty ID throws exception.
    /// </summary>
    [Fact]
    public void TestEmptyIdThrowsException()
    {
        // Arrange
        var invoice = new Invoice();

        // Act & Assert
        Action act = () => invoice.Id = "";
        act.Should().Throw<ArgumentException>()
            .WithMessage("*invoice id*");
    }

    /// <summary>
    /// Test that empty language ID throws exception.
    /// </summary>
    [Fact]
    public void TestEmptyLanguageIdThrowsException()
    {
        // Arrange
        var invoice = new Invoice();

        // Act & Assert
        Action act = () => invoice.LanguageID = "";
        act.Should().Throw<ArgumentException>()
            .WithMessage("*languageID*");
    }

    /// <summary>
    /// Test that empty currency codes throw exceptions.
    /// </summary>
    [Fact]
    public void TestEmptyCurrencyCodesThrowException()
    {
        // Arrange
        var invoice = new Invoice();

        // Act & Assert
        Action actInvoiceCurrency = () => invoice.InvoiceCurrencyCode = "";
        actInvoiceCurrency.Should().Throw<ArgumentException>()
            .WithMessage("*Invoice currency code*");

        Action actTaxCurrency = () => invoice.TaxCurrencyCode = "";
        actTaxCurrency.Should().Throw<ArgumentException>()
            .WithMessage("*Tax currency code*");

        Action actDocCurrency = () => invoice.DocumentCurrencyCode = "";
        actDocCurrency.Should().Throw<ArgumentException>()
            .WithMessage("*Document currency code*");
    }

    /// <summary>
    /// Test that default values are set correctly.
    /// </summary>
    [Fact]
    public void TestDefaultValues()
    {
        // Arrange & Act
        var invoice = new Invoice();

        // Assert
        invoice.ProfileID.Should().Be("reporting:1.0");
        invoice.LanguageID.Should().Be("en");
        invoice.InvoiceCurrencyCode.Should().Be("SAR");
        invoice.TaxCurrencyCode.Should().Be("SAR");
        invoice.DocumentCurrencyCode.Should().Be("SAR");
    }

    /// <summary>
    /// Test that Invoice object built from mapper can be validated.
    /// </summary>
    [Fact]
    public void TestInvoiceFromMapperValidates()
    {
        // Arrange
        var mapper = new InvoiceMapper();
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var invoice = mapper.MapToInvoice(invoiceData);

        // Assert - Should not throw
        Action act = () => invoice.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Test that Invoice has correct structure for XML generation.
    /// Verifies all necessary components are present.
    /// </summary>
    [Fact]
    public void TestInvoiceXmlGenerationStructure()
    {
        // Arrange
        var invoice = CreateCompleteInvoice();

        // Assert - Verify all key elements are present
        invoice.UUID.Should().NotBeNullOrEmpty();
        invoice.Id.Should().NotBeNullOrEmpty();
        invoice.IssueDate.Should().NotBeNull();
        invoice.IssueTime.Should().NotBeNull();
        invoice.InvoiceType.Should().NotBeNull();
        invoice.AccountingSupplierParty.Should().NotBeNull();
        invoice.AccountingCustomerParty.Should().NotBeNull();
        invoice.LegalMonetaryTotal.Should().NotBeNull();
        invoice.InvoiceLines.Should().NotBeNull();
        invoice.InvoiceLines.Should().HaveCountGreaterThan(0);
        invoice.AdditionalDocumentReferences.Should().NotBeNull();
        invoice.AdditionalDocumentReferences.Should().HaveCountGreaterThan(0);
    }

    /// <summary>
    /// Test that Invoice properties can be set and retrieved correctly.
    /// </summary>
    [Fact]
    public void TestInvoiceProperties()
    {
        // Arrange
        var invoice = new Invoice();
        var testDate = new DateOnly(2024, 9, 7);
        var testTime = new TimeOnly(17, 41, 8);

        // Act
        invoice.UUID = "test-uuid";
        invoice.Id = "INV-001";
        invoice.IssueDate = testDate;
        invoice.IssueTime = testTime;
        invoice.Note = "Test Note";
        invoice.ProfileID = "reporting:1.0";
        invoice.LanguageID = "ar";
        invoice.InvoiceCurrencyCode = "SAR";

        // Assert
        invoice.UUID.Should().Be("test-uuid");
        invoice.Id.Should().Be("INV-001");
        invoice.IssueDate.Should().Be(testDate);
        invoice.IssueTime.Should().Be(testTime);
        invoice.Note.Should().Be("Test Note");
        invoice.ProfileID.Should().Be("reporting:1.0");
        invoice.LanguageID.Should().Be("ar");
        invoice.InvoiceCurrencyCode.Should().Be("SAR");
    }

    /// <summary>
    /// Helper method to create a complete valid invoice for testing.
    /// </summary>
    private static Invoice CreateCompleteInvoice()
    {
        var invoice = new Invoice
        {
            UUID = "3cf5ee18-ee25-44ea-a444-2c37ba7f28be",
            Id = "SME00023",
            IssueDate = new DateOnly(2024, 9, 7),
            IssueTime = new TimeOnly(17, 41, 8),
            InvoiceType = new InvoiceType
            {
                Invoice = "standard",
                InvoiceSubType = "invoice"
            },
            AccountingSupplierParty = new Party
            {
                LegalEntity = new LegalEntity
                {
                    RegistrationName = "Maximum Speed Tech Supply"
                },
                PartyTaxScheme = new PartyTaxScheme
                {
                    CompanyId = "399999999900003",
                    TaxScheme = new TaxScheme { Id = "VAT" }
                },
                PostalAddress = new Address
                {
                    StreetName = "Prince Sultan",
                    BuildingNumber = "2322",
                    CityName = "Riyadh",
                    PostalZone = "23333",
                    Country = "SA"
                }
            },
            AccountingCustomerParty = new Party
            {
                LegalEntity = new LegalEntity
                {
                    RegistrationName = "Fatoora Samples"
                },
                PartyTaxScheme = new PartyTaxScheme
                {
                    CompanyId = "399999999800003",
                    TaxScheme = new TaxScheme { Id = "VAT" }
                },
                PostalAddress = new Address
                {
                    StreetName = "Salah Al-Din",
                    BuildingNumber = "1111",
                    CityName = "Riyadh",
                    PostalZone = "12222",
                    Country = "SA"
                }
            },
            AdditionalDocumentReferences = new List<AdditionalDocumentReference>
            {
                new AdditionalDocumentReference { Id = "ICV", UUID = "10" },
                new AdditionalDocumentReference
                {
                    Id = "PIH",
                    Attachment = new Attachment
                    {
                        Base64Content = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ=="
                    }
                }
            },
            InvoiceLines = new List<InvoiceLine>
            {
                new InvoiceLine
                {
                    Id = "1",
                    InvoicedQuantity = 2,
                    LineExtensionAmount = 4,
                    Item = new Item
                    {
                        Name = "Product"
                    },
                    Price = new Price
                    {
                        PriceAmount = 2
                    }
                }
            },
            LegalMonetaryTotal = new LegalMonetaryTotal
            {
                LineExtensionAmount = 4,
                TaxExclusiveAmount = 4,
                TaxInclusiveAmount = 4.60m,
                PayableAmount = 4.60m
            }
        };

        return invoice;
    }
}
