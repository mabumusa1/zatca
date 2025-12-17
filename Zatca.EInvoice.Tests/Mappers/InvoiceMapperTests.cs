using Zatca.EInvoice.Tests.TestFixtures;

namespace Zatca.EInvoice.Tests.Mappers;

/// <summary>
/// Tests for the InvoiceMapper class.
/// Ensures that invoice data is correctly mapped to an Invoice object.
/// </summary>
public class InvoiceMapperTests
{
    private readonly InvoiceMapper _mapper;

    public InvoiceMapperTests()
    {
        _mapper = new InvoiceMapper();
    }

    /// <summary>
    /// Test that valid invoice data is correctly mapped to an Invoice object.
    /// Uses complete test data with all required fields.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceWithValidData()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var invoice = _mapper.MapToInvoice(invoiceData);

        // Assert
        invoice.Should().NotBeNull();
        invoice.Should().BeOfType<Invoice>();
        invoice.UUID.Should().Be("3cf5ee18-ee25-44ea-a444-2c37ba7f28be");
        invoice.Id.Should().Be("SME00023");
        invoice.InvoiceCurrencyCode.Should().Be("SAR");
        invoice.TaxCurrencyCode.Should().Be("SAR");
        invoice.AccountingSupplierParty.Should().NotBeNull();
        invoice.AccountingCustomerParty.Should().NotBeNull();
        invoice.InvoiceLines.Should().NotBeNull();
        invoice.InvoiceLines.Should().HaveCount(1);
        invoice.LegalMonetaryTotal.Should().NotBeNull();
        invoice.LegalMonetaryTotal!.LineExtensionAmount.Should().Be(4m);
        invoice.LegalMonetaryTotal!.TaxInclusiveAmount.Should().Be(4.60m);
    }

    /// <summary>
    /// Test that mapping with minimal valid data creates an Invoice object.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceWithMinimalData()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetMinimalInvoiceData();

        // Act
        var invoice = _mapper.MapToInvoice(invoiceData);

        // Assert
        invoice.Should().NotBeNull();
        invoice.UUID.Should().Be("3cf5ee18-ee25-44ea-a444-2c37ba7f28be");
        invoice.Id.Should().Be("SME00023");
        invoice.AccountingSupplierParty.Should().NotBeNull();
        invoice.InvoiceLines.Should().NotBeNull();
        invoice.InvoiceLines.Should().HaveCount(1);
    }

    /// <summary>
    /// Test that mapping with null data throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceWithNullDataThrowsException()
    {
        // Act & Assert
        Action act = () => _mapper.MapToInvoice((Dictionary<string, object>)null!);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*data*");
    }

    /// <summary>
    /// Test that mapping with empty data throws ArgumentException.
    /// Validation should fail when required fields are missing.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceWithInvalidDataThrowsException()
    {
        // Arrange
        var emptyData = new Dictionary<string, object>();

        // Act & Assert
        Action act = () => _mapper.MapToInvoice(emptyData);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Test that mapping correctly handles supplier data.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceMapsSupplierCorrectly()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var invoice = _mapper.MapToInvoice(invoiceData);

        // Assert
        invoice.AccountingSupplierParty.Should().NotBeNull();
        invoice.AccountingSupplierParty!.LegalEntity.Should().NotBeNull();
        invoice.AccountingSupplierParty!.LegalEntity!.RegistrationName.Should().Be("Maximum Speed Tech Supply");
        invoice.AccountingSupplierParty!.PartyTaxScheme.Should().NotBeNull();
        invoice.AccountingSupplierParty!.PartyTaxScheme!.CompanyId.Should().Be("399999999900003");
    }

    /// <summary>
    /// Test that mapping correctly handles customer data.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceMapsCustomerCorrectly()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var invoice = _mapper.MapToInvoice(invoiceData);

        // Assert
        invoice.AccountingCustomerParty.Should().NotBeNull();
        invoice.AccountingCustomerParty!.LegalEntity.Should().NotBeNull();
        invoice.AccountingCustomerParty!.LegalEntity!.RegistrationName.Should().Be("Fatoora Samples");
        invoice.AccountingCustomerParty!.PartyTaxScheme.Should().NotBeNull();
        invoice.AccountingCustomerParty!.PartyTaxScheme!.CompanyId.Should().Be("399999999800003");
    }

    /// <summary>
    /// Test that mapping correctly handles invoice lines.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceMapsInvoiceLinesCorrectly()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var invoice = _mapper.MapToInvoice(invoiceData);

        // Assert
        invoice.InvoiceLines.Should().NotBeNull();
        invoice.InvoiceLines.Should().HaveCount(1);

        var line = invoice.InvoiceLines![0];
        line.Id.Should().Be("1");
        line.InvoicedQuantity.Should().Be(2m);
        line.LineExtensionAmount.Should().Be(4m);
        line.Item.Should().NotBeNull();
        line.Item!.Name.Should().Be("Product");
        line.Price.Should().NotBeNull();
        line.Price!.PriceAmount.Should().Be(2m);
    }

    /// <summary>
    /// Test that mapping correctly handles tax totals.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceMapsTaxTotalCorrectly()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var invoice = _mapper.MapToInvoice(invoiceData);

        // Assert
        invoice.TaxTotal.Should().NotBeNull();
        invoice.TaxTotal!.TaxAmount.Should().Be(0.6m);
        invoice.TaxTotal!.TaxSubTotals.Should().NotBeNull();
        invoice.TaxTotal!.TaxSubTotals.Should().HaveCount(1);
        invoice.TaxTotal!.TaxSubTotals![0].TaxableAmount.Should().Be(4m);
        invoice.TaxTotal!.TaxSubTotals![0].TaxAmount.Should().Be(0.6m);
    }

    /// <summary>
    /// Test that mapping correctly handles legal monetary total.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceMapsLegalMonetaryTotalCorrectly()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var invoice = _mapper.MapToInvoice(invoiceData);

        // Assert
        invoice.LegalMonetaryTotal.Should().NotBeNull();
        invoice.LegalMonetaryTotal!.LineExtensionAmount.Should().Be(4m);
        invoice.LegalMonetaryTotal!.TaxExclusiveAmount.Should().Be(4m);
        invoice.LegalMonetaryTotal!.TaxInclusiveAmount.Should().Be(4.60m);
        invoice.LegalMonetaryTotal!.PrepaidAmount.Should().Be(0m);
        invoice.LegalMonetaryTotal!.PayableAmount.Should().Be(4.60m);
        invoice.LegalMonetaryTotal!.AllowanceTotalAmount.Should().Be(0m);
    }

    /// <summary>
    /// Test that mapping correctly handles additional document references.
    /// </summary>
    [Fact]
    public void TestMapToInvoiceMapsAdditionalDocumentsCorrectly()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var invoice = _mapper.MapToInvoice(invoiceData);

        // Assert
        invoice.AdditionalDocumentReferences.Should().NotBeNull();
        invoice.AdditionalDocumentReferences.Should().HaveCountGreaterOrEqualTo(2);

        var icvDoc = invoice.AdditionalDocumentReferences!.FirstOrDefault(d => d.Id == "ICV");
        icvDoc.Should().NotBeNull();
        icvDoc!.UUID.Should().Be("10");

        var pihDoc = invoice.AdditionalDocumentReferences!.FirstOrDefault(d => d.Id == "PIH");
        pihDoc.Should().NotBeNull();
        pihDoc!.Attachment.Should().NotBeNull();
    }
}
