using Zatca.EInvoice.Validation;

namespace Zatca.EInvoice.Tests.Validation;

public class InvoiceAmountValidatorTests
{
    #region ValidateMonetaryTotals Tests

    [Fact]
    public void ValidateMonetaryTotals_ValidData_ReturnsSuccess()
    {
        var data = CreateValidInvoiceData();
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateMonetaryTotals_MissingLegalMonetaryTotal_ReturnsError()
    {
        var data = new Dictionary<string, object>();
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Legal Monetary Total section is missing"));
    }

    [Fact]
    public void ValidateMonetaryTotals_InvalidLegalMonetaryTotalType_ReturnsError()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", "invalid" }
        };
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Legal Monetary Total must be a valid object"));
    }

    [Fact]
    public void ValidateMonetaryTotals_MissingRequiredFields_ReturnsErrors()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>() }
        };
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("lineExtensionAmount"));
        result.Errors.Should().Contain(e => e.Contains("taxExclusiveAmount"));
        result.Errors.Should().Contain(e => e.Contains("taxInclusiveAmount"));
        result.Errors.Should().Contain(e => e.Contains("payableAmount"));
    }

    [Fact]
    public void ValidateMonetaryTotals_NegativeValues_ReturnsErrors()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", -100m },
                    { "taxExclusiveAmount", 100m },
                    { "taxInclusiveAmount", 115m },
                    { "payableAmount", 115m }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be negative"));
    }

    [Fact]
    public void ValidateMonetaryTotals_TaxInclusiveMismatch_ReturnsError()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 100m },
                    { "taxExclusiveAmount", 100m },
                    { "taxInclusiveAmount", 200m }, // Should be 115 (100 + 15)
                    { "payableAmount", 200m }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 15m }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("taxInclusiveAmount"));
    }

    [Fact]
    public void ValidateMonetaryTotals_StringValues_ParsesCorrectly()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", "100.00" },
                    { "taxExclusiveAmount", "100.00" },
                    { "taxInclusiveAmount", "115.00" },
                    { "payableAmount", "115.00" }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", "15.00" }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ValidateMonetaryTotalsAndThrow Tests

    [Fact]
    public void ValidateMonetaryTotalsAndThrow_ValidData_DoesNotThrow()
    {
        var data = CreateValidInvoiceData();
        Action action = () => InvoiceAmountValidator.ValidateMonetaryTotalsAndThrow(data);
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateMonetaryTotalsAndThrow_MissingLegalMonetaryTotal_ThrowsArgumentException()
    {
        var data = new Dictionary<string, object>();
        Action action = () => InvoiceAmountValidator.ValidateMonetaryTotalsAndThrow(data);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Legal Monetary Total section is missing*");
    }

    [Fact]
    public void ValidateMonetaryTotalsAndThrow_InvalidType_ThrowsArgumentException()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", "invalid" }
        };
        Action action = () => InvoiceAmountValidator.ValidateMonetaryTotalsAndThrow(data);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Legal Monetary Total must be a valid object*");
    }

    [Fact]
    public void ValidateMonetaryTotalsAndThrow_NonNumericField_ThrowsArgumentException()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", "invalid" },
                    { "taxExclusiveAmount", 100m },
                    { "taxInclusiveAmount", 115m },
                    { "payableAmount", 115m }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateMonetaryTotalsAndThrow(data);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*must be a numeric value*");
    }

    [Fact]
    public void ValidateMonetaryTotalsAndThrow_NegativeValue_ThrowsArgumentException()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", -100m },
                    { "taxExclusiveAmount", 100m },
                    { "taxInclusiveAmount", 115m },
                    { "payableAmount", 115m }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateMonetaryTotalsAndThrow(data);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void ValidateMonetaryTotalsAndThrow_TaxInclusiveMismatch_ThrowsArgumentException()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 100m },
                    { "taxExclusiveAmount", 100m },
                    { "taxInclusiveAmount", 200m },
                    { "payableAmount", 200m }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 15m }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateMonetaryTotalsAndThrow(data);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*taxInclusiveAmount*does not equal*");
    }

    #endregion

    #region ValidateInvoiceLines Tests

    [Fact]
    public void ValidateInvoiceLines_ValidLines_ReturnsSuccess()
    {
        var lines = CreateValidInvoiceLines();
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateInvoiceLines_MissingQuantity_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("quantity"));
    }

    [Fact]
    public void ValidateInvoiceLines_NegativeQuantity_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", -1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("cannot be negative"));
    }

    [Fact]
    public void ValidateInvoiceLines_MissingPrice_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("price"));
    }

    [Fact]
    public void ValidateInvoiceLines_InvalidPriceType_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", "invalid" },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("valid price object"));
    }

    [Fact]
    public void ValidateInvoiceLines_NegativePriceAmount_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", -100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Price amount cannot be negative"));
    }

    [Fact]
    public void ValidateInvoiceLines_LineExtensionMismatch_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 2m },
                { "lineExtensionAmount", 300m }, // Should be 200 (100 * 2)
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 45m },
                        { "roundingAmount", 345m }
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("lineExtensionAmount is incorrect"));
    }

    [Fact]
    public void ValidateInvoiceLines_InvalidTaxPercent_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "item", new Dictionary<string, object>
                    {
                        { "taxPercent", 150m } // Invalid: should be 0-100
                    }
                },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("taxPercent must be between 0 and 100"));
    }

    [Fact]
    public void ValidateInvoiceLines_InvalidTaxPercentType_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "item", new Dictionary<string, object>
                    {
                        { "taxPercent", "invalid" }
                    }
                },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("taxPercent must be a numeric value"));
    }

    [Fact]
    public void ValidateInvoiceLines_MissingTaxTotal_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("valid taxTotal object"));
    }

    [Fact]
    public void ValidateInvoiceLines_NegativeTaxAmount_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", -15m },
                        { "roundingAmount", 85m }
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("taxAmount cannot be negative"));
    }

    [Fact]
    public void ValidateInvoiceLines_RoundingAmountMismatch_ReturnsError()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 200m } // Should be 115 (100 + 15)
                    }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateInvoiceLines(lines);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("roundingAmount is incorrect"));
    }

    #endregion

    #region ValidateInvoiceLinesAndThrow Tests

    [Fact]
    public void ValidateInvoiceLinesAndThrow_ValidLines_DoesNotThrow()
    {
        var lines = CreateValidInvoiceLines();
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_MissingQuantity_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*quantity*must be a numeric value*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_NegativeQuantity_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", -1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_InvalidPrice_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", "invalid" },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*valid price object*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_MissingPriceAmount_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object>() },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Price amount must be a numeric value*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_NegativePriceAmount_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", -100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Price amount cannot be negative*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_LineExtensionMismatch_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 2m },
                { "lineExtensionAmount", 300m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 45m },
                        { "roundingAmount", 345m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*lineExtensionAmount is incorrect*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_InvalidTaxPercent_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "item", new Dictionary<string, object>
                    {
                        { "taxPercent", 150m }
                    }
                },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*taxPercent must be between 0 and 100*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_InvalidTaxTotal_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", "invalid" }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*valid taxTotal object*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_MissingTaxAmount_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*TaxTotal taxAmount must be a numeric value*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_NegativeTaxAmount_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", -15m },
                        { "roundingAmount", 85m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*taxAmount cannot be negative*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_MissingRoundingAmount_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*roundingAmount must be a numeric value*");
    }

    [Fact]
    public void ValidateInvoiceLinesAndThrow_RoundingAmountMismatch_ThrowsArgumentException()
    {
        var lines = new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 200m }
                    }
                }
            }
        };
        Action action = () => InvoiceAmountValidator.ValidateInvoiceLinesAndThrow(lines);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*roundingAmount is incorrect*");
    }

    #endregion

    #region Numeric Type Handling Tests

    [Theory]
    [InlineData(100)]  // int
    [InlineData(100L)] // long
    public void ValidateMonetaryTotals_IntegerTypes_ParsesCorrectly(object amount)
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", amount },
                    { "taxExclusiveAmount", amount },
                    { "taxInclusiveAmount", 115 },
                    { "payableAmount", 115 }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 15 }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateMonetaryTotals_FloatType_ParsesCorrectly()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 100.0f },
                    { "taxExclusiveAmount", 100.0f },
                    { "taxInclusiveAmount", 115.0f },
                    { "payableAmount", 115.0f }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 15.0f }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateMonetaryTotals_DoubleType_ParsesCorrectly()
    {
        var data = new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 100.0 },
                    { "taxExclusiveAmount", 100.0 },
                    { "taxInclusiveAmount", 115.0 },
                    { "payableAmount", 115.0 }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 15.0 }
                }
            }
        };
        var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static Dictionary<string, object> CreateValidInvoiceData()
    {
        return new Dictionary<string, object>
        {
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 100m },
                    { "taxExclusiveAmount", 100m },
                    { "taxInclusiveAmount", 115m },
                    { "payableAmount", 115m }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 15m }
                }
            }
        };
    }

    private static List<object> CreateValidInvoiceLines()
    {
        return new List<object>
        {
            new Dictionary<string, object>
            {
                { "quantity", 1m },
                { "lineExtensionAmount", 100m },
                { "price", new Dictionary<string, object> { { "amount", 100m } } },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 15m },
                        { "roundingAmount", 115m }
                    }
                }
            }
        };
    }

    #endregion
}
