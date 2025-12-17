using Zatca.EInvoice.Tests.TestFixtures;

namespace Zatca.EInvoice.Tests.Mappers;

/// <summary>
/// Tests for the InvoiceAmountValidator class.
/// Ensures that invoice amount calculations and validations are correct.
/// </summary>
public class InvoiceAmountValidatorTests
{
    private readonly InvoiceAmountValidator _validator;

    public InvoiceAmountValidatorTests()
    {
        _validator = new InvoiceAmountValidator();
    }

    /// <summary>
    /// Test that valid monetary totals pass validation.
    /// </summary>
    [Fact]
    public void TestValidAmounts()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();

        // Act
        var result = _validator.ValidateMonetaryTotals(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Test that valid invoice lines pass validation.
    /// </summary>
    [Fact]
    public void TestValidInvoiceLines()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;

        // Act
        var result = _validator.ValidateInvoiceLines(invoiceLines!);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Test that incorrect line extension amount calculation throws exception.
    /// Line extension should equal quantity * price amount.
    /// </summary>
    [Fact]
    public void TestInvalidLineExtensionCalculationThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;
        var firstLine = invoiceLines![0] as Dictionary<string, object>;
        firstLine!["lineExtensionAmount"] = 100m; // Wrong amount (should be 2 * 2 = 4)

        // Act & Assert
        Action act = () => _validator.ValidateInvoiceLinesAndThrow(invoiceLines);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*lineExtensionAmount is incorrect*");
    }

    /// <summary>
    /// Test that incorrect rounding amount calculation throws exception.
    /// Rounding amount should equal line extension + tax amount.
    /// </summary>
    [Fact]
    public void TestInvalidRoundingAmountCalculationThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;
        var firstLine = invoiceLines![0] as Dictionary<string, object>;
        var taxTotal = firstLine!["taxTotal"] as Dictionary<string, object>;
        taxTotal!["roundingAmount"] = 100m; // Wrong amount (should be 4 + 0.60 = 4.60)

        // Act & Assert
        Action act = () => _validator.ValidateInvoiceLinesAndThrow(invoiceLines);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*roundingAmount is incorrect*");
    }

    /// <summary>
    /// Test that incorrect tax inclusive amount throws exception.
    /// Tax inclusive should equal tax exclusive + tax total.
    /// </summary>
    [Fact]
    public void TestInvalidTaxInclusiveAmountThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var legalMonetaryTotal = invoiceData["legalMonetaryTotal"] as Dictionary<string, object>;
        legalMonetaryTotal!["taxInclusiveAmount"] = 100m; // Wrong amount (should be 4 + 0.6 = 4.60)

        // Act & Assert
        Action act = () => _validator.ValidateMonetaryTotalsAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*taxInclusiveAmount*does not equal*");
    }

    /// <summary>
    /// Test that negative line extension amount throws exception.
    /// </summary>
    [Fact]
    public void TestNegativeLineExtensionAmountThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;
        var firstLine = invoiceLines![0] as Dictionary<string, object>;
        firstLine!["lineExtensionAmount"] = -10m;

        // Act & Assert
        Action act = () => _validator.ValidateInvoiceLinesAndThrow(invoiceLines);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    /// <summary>
    /// Test that negative price amount throws exception.
    /// </summary>
    [Fact]
    public void TestNegativePriceAmountThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;
        var firstLine = invoiceLines![0] as Dictionary<string, object>;
        var price = firstLine!["price"] as Dictionary<string, object>;
        price!["amount"] = -5m;

        // Act & Assert
        Action act = () => _validator.ValidateInvoiceLinesAndThrow(invoiceLines);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    /// <summary>
    /// Test that negative tax amount throws exception.
    /// </summary>
    [Fact]
    public void TestNegativeTaxAmountThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;
        var firstLine = invoiceLines![0] as Dictionary<string, object>;
        var taxTotal = firstLine!["taxTotal"] as Dictionary<string, object>;
        taxTotal!["taxAmount"] = -1m;

        // Act & Assert
        Action act = () => _validator.ValidateInvoiceLinesAndThrow(invoiceLines);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
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
        Action act = () => _validator.ValidateMonetaryTotalsAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Legal Monetary Total*missing*");
    }

    /// <summary>
    /// Test that missing required monetary total field throws exception.
    /// </summary>
    [Fact]
    public void TestMissingRequiredMonetaryFieldThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var legalMonetaryTotal = invoiceData["legalMonetaryTotal"] as Dictionary<string, object>;
        legalMonetaryTotal!.Remove("lineExtensionAmount");

        // Act & Assert
        Action act = () => _validator.ValidateMonetaryTotalsAndThrow(invoiceData);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*lineExtensionAmount*");
    }

    /// <summary>
    /// Test that missing price object throws exception.
    /// </summary>
    [Fact]
    public void TestMissingPriceObjectThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;
        var firstLine = invoiceLines![0] as Dictionary<string, object>;
        firstLine!.Remove("price");

        // Act & Assert
        Action act = () => _validator.ValidateInvoiceLinesAndThrow(invoiceLines);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*price object*");
    }

    /// <summary>
    /// Test that missing tax total object throws exception.
    /// </summary>
    [Fact]
    public void TestMissingTaxTotalObjectThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;
        var firstLine = invoiceLines![0] as Dictionary<string, object>;
        firstLine!.Remove("taxTotal");

        // Act & Assert
        Action act = () => _validator.ValidateInvoiceLinesAndThrow(invoiceLines);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*taxTotal object*");
    }

    /// <summary>
    /// Test that non-numeric quantity throws exception.
    /// </summary>
    [Fact]
    public void TestNonNumericQuantityThrowsException()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var invoiceLines = invoiceData["invoiceLines"] as List<object>;
        var firstLine = invoiceLines![0] as Dictionary<string, object>;
        firstLine!["quantity"] = "not a number";

        // Act & Assert
        Action act = () => _validator.ValidateInvoiceLinesAndThrow(invoiceLines);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be a numeric value*");
    }

    /// <summary>
    /// Test that validation result contains specific errors.
    /// </summary>
    [Fact]
    public void TestValidationResultContainsSpecificErrors()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var legalMonetaryTotal = invoiceData["legalMonetaryTotal"] as Dictionary<string, object>;
        legalMonetaryTotal!["taxInclusiveAmount"] = 100m; // Wrong amount

        // Act
        var result = _validator.ValidateMonetaryTotals(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Contains("taxInclusiveAmount"));
    }

    /// <summary>
    /// Test that correct calculations with rounding tolerance pass validation.
    /// The validator allows a tolerance of 0.01 for rounding differences.
    /// </summary>
    [Fact]
    public void TestRoundingToleranceIsAccepted()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var legalMonetaryTotal = invoiceData["legalMonetaryTotal"] as Dictionary<string, object>;
        // Set amount slightly off but within tolerance (0.01)
        legalMonetaryTotal!["taxInclusiveAmount"] = 4.605m; // Should be 4.60, but 4.605 is within tolerance

        // Act
        var result = _validator.ValidateMonetaryTotals(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Test that amount beyond rounding tolerance fails validation.
    /// </summary>
    [Fact]
    public void TestAmountBeyondToleranceFails()
    {
        // Arrange
        var invoiceData = InvoiceTestData.GetValidInvoiceData();
        var legalMonetaryTotal = invoiceData["legalMonetaryTotal"] as Dictionary<string, object>;
        // Set amount beyond tolerance (> 0.01)
        legalMonetaryTotal!["taxInclusiveAmount"] = 4.62m; // Should be 4.60, 4.62 is beyond tolerance

        // Act
        var result = _validator.ValidateMonetaryTotals(invoiceData);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("taxInclusiveAmount"));
    }
}
