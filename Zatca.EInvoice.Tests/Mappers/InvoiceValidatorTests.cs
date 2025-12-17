using Zatca.EInvoice.Tests.TestFixtures;

namespace Zatca.EInvoice.Tests.Mappers;

/// <summary>
/// Tests for the InvoiceValidator class.
/// Ensures that invoice validation rules are correctly applied.
/// </summary>
public class InvoiceValidatorTests
{
    private readonly InvoiceValidator _validator;

    public InvoiceValidatorTests()
    {
        _validator = new InvoiceValidator();
    }

    /// <summary>
    /// Test that valid invoice data passes validation.
    /// </summary>
    [Fact]
    public void TestValidInvoiceData()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var result = _validator.Validate(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Test that minimal valid invoice data passes validation.
    /// </summary>
    [Fact]
    public void TestMinimalValidInvoiceData()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetMinimalInvoiceData();

        // Act
        var result = _validator.Validate(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Test that missing UUID field throws exception.
    /// </summary>
    [Fact]
    public void TestMissingRequiredFieldThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        invoiceData.Remove("uuid");

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UUID*");
    }

    /// <summary>
    /// Test that missing invoice ID throws exception.
    /// </summary>
    [Fact]
    public void TestMissingInvoiceIdThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        invoiceData.Remove("id");

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invoice ID*");
    }

    /// <summary>
    /// Test that missing issue date throws exception.
    /// </summary>
    [Fact]
    public void TestMissingIssueDateThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        invoiceData.Remove("issueDate");

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Issue Date*");
    }

    /// <summary>
    /// Test that missing currency code throws exception.
    /// </summary>
    [Fact]
    public void TestMissingCurrencyCodeThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        invoiceData.Remove("currencyCode");

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Currency Code*");
    }

    /// <summary>
    /// Test that missing supplier data throws exception.
    /// </summary>
    [Fact]
    public void TestMissingSupplierThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        invoiceData.Remove("supplier");

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Supplier*");
    }

    /// <summary>
    /// Test that missing customer data for standard invoice throws exception.
    /// </summary>
    [Fact]
    public void TestMissingCustomerForStandardInvoiceThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        invoiceData.Remove("customer");

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Customer*");
    }

    /// <summary>
    /// Test that simplified invoice without customer data is valid.
    /// </summary>
    [Fact]
    public void TestSimplifiedInvoiceWithoutCustomerIsValid()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetMinimalInvoiceData();
        // Ensure it's simplified
        var invoiceType = invoiceData["invoiceType"] as Dictionary<string, object>;
        invoiceType!["invoice"] = "simplified";

        // Act
        var result = _validator.Validate(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Test that missing legal monetary total throws exception.
    /// </summary>
    [Fact]
    public void TestMissingLegalMonetaryTotalThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        invoiceData.Remove("legalMonetaryTotal");

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Legal Monetary Total*");
    }

    /// <summary>
    /// Test that missing invoice lines throws exception.
    /// </summary>
    [Fact]
    public void TestMissingInvoiceLinesThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        invoiceData.Remove("invoiceLines");

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*invoice line*");
    }

    /// <summary>
    /// Test that empty invoice lines throws exception.
    /// </summary>
    [Fact]
    public void TestEmptyInvoiceLinesThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        invoiceData["invoiceLines"] = new List<object>();

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*invoice line*");
    }

    /// <summary>
    /// Test that invalid supplier address throws exception.
    /// </summary>
    [Fact]
    public void TestInvalidSupplierAddressThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var supplier = invoiceData["supplier"] as Dictionary<string, object>;
        var address = supplier!["address"] as Dictionary<string, object>;
        address!.Remove("street");

        // Act & Assert
        Action act = () => _validator.ValidateAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*street*");
    }

    /// <summary>
    /// Test that validation result contains errors for invalid data.
    /// </summary>
    [Fact]
    public void TestValidationResultContainsErrors()
    {
        // Arrange
        var invoiceData = new Dictionary<string, object>();

        // Act
        var result = _validator.Validate(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Contains("UUID"));
    }

    /// <summary>
    /// Test that invoice type validation works correctly.
    /// </summary>
    [Fact]
    public void TestInvoiceTypeValidation()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceType = invoiceData["invoiceType"] as Dictionary<string, object>;
        invoiceType!.Remove("type");

        // Act
        var result = _validator.Validate(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invoice Type"));
    }

    /// <summary>
    /// Test that payment means validation works correctly.
    /// </summary>
    [Fact]
    public void TestPaymentMeansValidation()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var paymentMeans = invoiceData["paymentMeans"] as Dictionary<string, object>;
        paymentMeans!.Remove("code");

        // Act
        var result = _validator.Validate(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Payment Means"));
    }

    /// <summary>
    /// Test that invoice line item validation works correctly.
    /// </summary>
    [Fact]
    public void TestInvoiceLineItemValidation()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;
        var firstLine = invoiceLines![0] as Dictionary<string, object>;
        var item = firstLine!["item"] as Dictionary<string, object>;
        item!.Remove("name");

        // Act
        var result = _validator.Validate(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Item name"));
    }
}
