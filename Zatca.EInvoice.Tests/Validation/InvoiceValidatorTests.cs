using Zatca.EInvoice.Validation;

namespace Zatca.EInvoice.Tests.Validation;

public class InvoiceValidatorTests
{
    private readonly InvoiceValidator _validator;

    public InvoiceValidatorTests()
    {
        _validator = new InvoiceValidator();
    }

    #region Validate Method Tests

    [Fact]
    public void Validate_ValidInvoice_ReturnsSuccess()
    {
        var data = CreateValidInvoiceData();
        var result = _validator.Validate(data);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MissingUuid_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data.Remove("uuid");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("UUID"));
    }

    [Fact]
    public void Validate_MissingId_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data.Remove("id");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invoice ID"));
    }

    [Fact]
    public void Validate_MissingIssueDate_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data.Remove("issueDate");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Issue Date"));
    }

    [Fact]
    public void Validate_MissingCurrencyCode_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data.Remove("currencyCode");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invoice Currency Code"));
    }

    [Fact]
    public void Validate_MissingSupplier_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data.Remove("supplier");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Supplier"));
    }

    [Fact]
    public void Validate_InvalidSupplierType_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["supplier"] = "invalid";

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Supplier data must be a valid object"));
    }

    [Fact]
    public void Validate_MissingSupplierAddress_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        var supplier = (Dictionary<string, object>)data["supplier"];
        supplier.Remove("address");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Supplier address"));
    }

    [Fact]
    public void Validate_SimplifiedInvoice_SkipsCustomerValidation()
    {
        var data = CreateValidInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "invoice", "simplified" },
            { "type", "388" }
        };
        data.Remove("customer");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_StandardInvoice_MissingCustomer_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "invoice", "standard" },
            { "type", "388" }
        };
        data.Remove("customer");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Customer"));
    }

    [Fact]
    public void Validate_InvalidCustomerType_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "invoice", "standard" },
            { "type", "388" }
        };
        data["customer"] = "invalid";

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Customer data must be a valid object"));
    }

    [Fact]
    public void Validate_MissingLegalMonetaryTotal_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data.Remove("legalMonetaryTotal");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Legal Monetary Total"));
    }

    [Fact]
    public void Validate_InvalidLegalMonetaryTotalType_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["legalMonetaryTotal"] = "invalid";

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Legal Monetary Total must be a valid object"));
    }

    [Fact]
    public void Validate_MissingInvoiceLines_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data.Remove("invoiceLines");

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("invoice line"));
    }

    [Fact]
    public void Validate_EmptyInvoiceLines_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["invoiceLines"] = new List<object>();

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("invoice line"));
    }

    [Fact]
    public void Validate_PaymentMeans_MissingCode_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["paymentMeans"] = new Dictionary<string, object>();

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Payment Means code"));
    }

    [Fact]
    public void Validate_TaxTotal_MissingTaxAmount_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["taxTotal"] = new Dictionary<string, object>();

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Tax Total taxAmount"));
    }

    [Fact]
    public void Validate_TaxSubTotals_MissingFields_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["taxTotal"] = new Dictionary<string, object>
        {
            { "taxAmount", 15.0m },
            { "subTotals", new List<object> { new Dictionary<string, object>() } }
        };

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("subTotals"));
    }

    [Fact]
    public void Validate_AdditionalDocument_MissingId_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["additionalDocuments"] = new List<object>
        {
            new Dictionary<string, object>()
        };

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("AdditionalDocuments"));
    }

    [Fact]
    public void Validate_AdditionalDocument_PIH_MissingAttachment_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["additionalDocuments"] = new List<object>
        {
            new Dictionary<string, object> { { "id", "PIH" } }
        };

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("PIH"));
    }

    [Fact]
    public void Validate_InvoiceLine_MissingRequiredFields_ReturnsErrors()
    {
        var data = CreateValidInvoiceData();
        data["invoiceLines"] = new List<object>
        {
            new Dictionary<string, object>
            {
                { "id", "1" }
            }
        };

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region ValidateAndThrow Method Tests

    [Fact]
    public void ValidateAndThrow_ValidInvoice_DoesNotThrow()
    {
        var data = CreateValidInvoiceData();
        Action action = () => _validator.ValidateAndThrow(data);
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateAndThrow_MissingUuid_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data.Remove("uuid");

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*UUID*");
    }

    [Fact]
    public void ValidateAndThrow_MissingSupplier_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data.Remove("supplier");

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Supplier*");
    }

    [Fact]
    public void ValidateAndThrow_InvalidSupplier_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["supplier"] = "invalid";

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Supplier data must be a valid object*");
    }

    [Fact]
    public void ValidateAndThrow_MissingSupplierAddress_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        var supplier = (Dictionary<string, object>)data["supplier"];
        supplier["address"] = "invalid";

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Supplier address*");
    }

    [Fact]
    public void ValidateAndThrow_StandardInvoice_MissingCustomer_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "invoice", "standard" },
            { "type", "388" }
        };
        data.Remove("customer");

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Customer*");
    }

    [Fact]
    public void ValidateAndThrow_InvalidCustomer_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "invoice", "standard" },
            { "type", "388" }
        };
        data["customer"] = "invalid";

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Customer data must be a valid object*");
    }

    [Fact]
    public void ValidateAndThrow_InvalidCustomerAddress_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "invoice", "standard" },
            { "type", "388" }
        };
        var customer = (Dictionary<string, object>)data["customer"];
        customer["address"] = "invalid";

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Customer address*");
    }

    [Fact]
    public void ValidateAndThrow_PaymentMeans_MissingCode_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["paymentMeans"] = new Dictionary<string, object>();

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Payment Means code*");
    }

    [Fact]
    public void ValidateAndThrow_TaxTotal_MissingTaxAmount_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["taxTotal"] = new Dictionary<string, object>
        {
            { "subTotals", new List<object>() }
        };

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Tax Total taxAmount*");
    }

    [Fact]
    public void ValidateAndThrow_TaxSubTotal_MissingTaxableAmount_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["taxTotal"] = new Dictionary<string, object>
        {
            { "taxAmount", 15.0m },
            { "subTotals", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "taxCategory", new Dictionary<string, object>
                            {
                                { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                            }
                        }
                    }
                }
            }
        };

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*subTotals*taxableAmount*");
    }

    [Fact]
    public void ValidateAndThrow_TaxSubTotal_MissingTaxSchemeId_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["taxTotal"] = new Dictionary<string, object>
        {
            { "taxAmount", 15.0m },
            { "subTotals", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "taxableAmount", 100m },
                        { "taxCategory", new Dictionary<string, object>
                            {
                                { "taxScheme", new Dictionary<string, object>() }
                            }
                        }
                    }
                }
            }
        };

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*TaxScheme id*");
    }

    [Fact]
    public void ValidateAndThrow_MissingLegalMonetaryTotal_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data.Remove("legalMonetaryTotal");

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Legal Monetary Total*");
    }

    [Fact]
    public void ValidateAndThrow_InvalidLegalMonetaryTotal_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["legalMonetaryTotal"] = "invalid";

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Legal Monetary Total must be a valid object*");
    }

    [Fact]
    public void ValidateAndThrow_MissingInvoiceLines_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data.Remove("invoiceLines");

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*invoice line*");
    }

    [Fact]
    public void ValidateAndThrow_EmptyInvoiceLines_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["invoiceLines"] = new List<object>();

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*invoice line*");
    }

    [Fact]
    public void ValidateAndThrow_InvoiceLine_InvalidItem_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        var lines = (List<object>)data["invoiceLines"];
        var line = (Dictionary<string, object>)lines[0];
        line["item"] = "invalid";

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*item*must be a valid object*");
    }

    [Fact]
    public void ValidateAndThrow_InvoiceLine_MissingItemName_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        var lines = (List<object>)data["invoiceLines"];
        var line = (Dictionary<string, object>)lines[0];
        var item = (Dictionary<string, object>)line["item"];
        item.Remove("name");

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Item name*");
    }

    [Fact]
    public void ValidateAndThrow_InvoiceLine_InvalidPrice_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        var lines = (List<object>)data["invoiceLines"];
        var line = (Dictionary<string, object>)lines[0];
        line["price"] = "invalid";

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*price*must be a valid object*");
    }

    [Fact]
    public void ValidateAndThrow_InvoiceLine_MissingPriceAmount_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        var lines = (List<object>)data["invoiceLines"];
        var line = (Dictionary<string, object>)lines[0];
        line["price"] = new Dictionary<string, object>();

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Price amount*");
    }

    [Fact]
    public void ValidateAndThrow_InvoiceLine_InvalidTaxTotal_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        var lines = (List<object>)data["invoiceLines"];
        var line = (Dictionary<string, object>)lines[0];
        line["taxTotal"] = "invalid";

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*taxTotal*must be a valid object*");
    }

    [Fact]
    public void ValidateAndThrow_InvoiceLine_MissingTaxAmount_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        var lines = (List<object>)data["invoiceLines"];
        var line = (Dictionary<string, object>)lines[0];
        line["taxTotal"] = new Dictionary<string, object>();

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*TaxTotal taxAmount*");
    }

    [Fact]
    public void ValidateAndThrow_AdditionalDocument_MissingId_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["additionalDocuments"] = new List<object>
        {
            new Dictionary<string, object>()
        };

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*AdditionalDocuments*id*");
    }

    [Fact]
    public void ValidateAndThrow_AdditionalDocument_PIH_MissingAttachment_ThrowsArgumentException()
    {
        var data = CreateValidInvoiceData();
        data["additionalDocuments"] = new List<object>
        {
            new Dictionary<string, object> { { "id", "PIH" } }
        };

        Action action = () => _validator.ValidateAndThrow(data);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*attachment*PIH*");
    }

    #endregion

    #region InvoiceType Tests

    [Fact]
    public void Validate_InvoiceType_MissingInvoiceField_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "type", "388" }
        };

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invoice Type (invoice)"));
    }

    [Fact]
    public void Validate_InvoiceType_MissingTypeField_ReturnsError()
    {
        var data = CreateValidInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "invoice", "standard" }
        };

        var result = _validator.Validate(data);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invoice Type (type)"));
    }

    #endregion

    #region Helper Methods

    private static Dictionary<string, object> CreateValidInvoiceData()
    {
        return new Dictionary<string, object>
        {
            { "uuid", "12345678-1234-1234-1234-123456789012" },
            { "id", "INV-001" },
            { "issueDate", "2024-01-01" },
            { "currencyCode", "SAR" },
            { "taxCurrencyCode", "SAR" },
            { "invoiceType", new Dictionary<string, object>
                {
                    { "invoice", "simplified" },
                    { "type", "388" }
                }
            },
            { "supplier", CreateValidParty() },
            { "customer", CreateValidParty() },
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 100.0m },
                    { "taxExclusiveAmount", 100.0m },
                    { "taxInclusiveAmount", 115.0m },
                    { "payableAmount", 115.0m }
                }
            },
            { "invoiceLines", new List<object> { CreateValidInvoiceLine() } }
        };
    }

    private static Dictionary<string, object> CreateValidParty()
    {
        return new Dictionary<string, object>
        {
            { "registrationName", "Test Company" },
            { "taxId", "300000000000003" },
            { "address", new Dictionary<string, object>
                {
                    { "street", "Test Street" },
                    { "buildingNumber", "123" },
                    { "city", "Riyadh" },
                    { "postalZone", "12345" },
                    { "country", "SA" }
                }
            }
        };
    }

    private static Dictionary<string, object> CreateValidInvoiceLine()
    {
        return new Dictionary<string, object>
        {
            { "id", "1" },
            { "unitCode", "PCE" },
            { "quantity", 1.0m },
            { "lineExtensionAmount", 100.0m },
            { "item", new Dictionary<string, object>
                {
                    { "name", "Test Item" },
                    { "classifiedTaxCategory", new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                { "percent", 15.0m },
                                { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                            }
                        }
                    }
                }
            },
            { "price", new Dictionary<string, object>
                {
                    { "amount", 100.0m }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 15.0m }
                }
            }
        };
    }

    #endregion
}
