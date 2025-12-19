using Zatca.EInvoice.Models;
using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.References;

namespace Zatca.EInvoice.Tests.Models;

public class InvoiceLineAndTypeTests
{
    #region InvoiceLine Tests

    [Fact]
    public void InvoiceLine_Id_CanBeSet()
    {
        var line = new InvoiceLine { Id = "1" };
        line.Id.Should().Be("1");
    }

    [Fact]
    public void InvoiceLine_Id_ThrowsOnEmpty()
    {
        var line = new InvoiceLine();
        Action act = () => line.Id = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void InvoiceLine_Id_AcceptsNull()
    {
        var line = new InvoiceLine { Id = "1" };
        line.Id = null;
        line.Id.Should().BeNull();
    }

    [Fact]
    public void InvoiceLine_InvoicedQuantity_CanBeSet()
    {
        var line = new InvoiceLine { InvoicedQuantity = 10m };
        line.InvoicedQuantity.Should().Be(10m);
    }

    [Fact]
    public void InvoiceLine_InvoicedQuantity_ThrowsOnNegative()
    {
        var line = new InvoiceLine();
        Action act = () => line.InvoicedQuantity = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void InvoiceLine_LineExtensionAmount_CanBeSet()
    {
        var line = new InvoiceLine { LineExtensionAmount = 100m };
        line.LineExtensionAmount.Should().Be(100m);
    }

    [Fact]
    public void InvoiceLine_LineExtensionAmount_ThrowsOnNegative()
    {
        var line = new InvoiceLine();
        Action act = () => line.LineExtensionAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void InvoiceLine_UnitCode_DefaultsToMON()
    {
        var line = new InvoiceLine();
        line.UnitCode.Should().Be("MON");
    }

    [Fact]
    public void InvoiceLine_UnitCode_CanBeSet()
    {
        var line = new InvoiceLine { UnitCode = "PCE" };
        line.UnitCode.Should().Be("PCE");
    }

    [Fact]
    public void InvoiceLine_UnitCode_ThrowsOnEmpty()
    {
        var line = new InvoiceLine();
        Action act = () => line.UnitCode = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void InvoiceLine_UnitCode_KeepsDefaultOnNull()
    {
        var line = new InvoiceLine();
        line.UnitCode = null!;
        line.UnitCode.Should().Be("MON");
    }

    [Fact]
    public void InvoiceLine_Note_CanBeSet()
    {
        var line = new InvoiceLine { Note = "Special item" };
        line.Note.Should().Be("Special item");
    }

    [Fact]
    public void InvoiceLine_Note_ThrowsOnEmpty()
    {
        var line = new InvoiceLine();
        Action act = () => line.Note = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void InvoiceLine_AccountingCostCode_CanBeSet()
    {
        var line = new InvoiceLine { AccountingCostCode = "ACC-001" };
        line.AccountingCostCode.Should().Be("ACC-001");
    }

    [Fact]
    public void InvoiceLine_AccountingCostCode_ThrowsOnEmpty()
    {
        var line = new InvoiceLine();
        Action act = () => line.AccountingCostCode = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void InvoiceLine_AccountingCost_CanBeSet()
    {
        var line = new InvoiceLine { AccountingCost = "Cost Center A" };
        line.AccountingCost.Should().Be("Cost Center A");
    }

    [Fact]
    public void InvoiceLine_AccountingCost_ThrowsOnEmpty()
    {
        var line = new InvoiceLine();
        Action act = () => line.AccountingCost = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void InvoiceLine_AllowanceCharges_CanBeSet()
    {
        var line = new InvoiceLine
        {
            AllowanceCharges = new List<AllowanceCharge> { new AllowanceCharge { Amount = 10m } }
        };
        line.AllowanceCharges.Should().HaveCount(1);
    }

    [Fact]
    public void InvoiceLine_DocumentReference_CanBeSet()
    {
        var line = new InvoiceLine { DocumentReference = new DocumentReference { Id = "DOC-001" } };
        line.DocumentReference.Should().NotBeNull();
    }

    [Fact]
    public void InvoiceLine_TaxTotal_CanBeSet()
    {
        var line = new InvoiceLine { TaxTotal = new TaxTotal { TaxAmount = 15m } };
        line.TaxTotal.Should().NotBeNull();
    }

    [Fact]
    public void InvoiceLine_InvoicePeriod_CanBeSet()
    {
        var line = new InvoiceLine { InvoicePeriod = new InvoicePeriod { StartDate = new DateOnly(2024, 1, 1) } };
        line.InvoicePeriod.Should().NotBeNull();
    }

    [Fact]
    public void InvoiceLine_Item_CanBeSet()
    {
        var line = new InvoiceLine { Item = new Item { Name = "Product" } };
        line.Item.Should().NotBeNull();
    }

    [Fact]
    public void InvoiceLine_Price_CanBeSet()
    {
        var line = new InvoiceLine { Price = new Price { PriceAmount = 50m } };
        line.Price.Should().NotBeNull();
    }

    #endregion

    #region InvoiceType Tests

    [Fact]
    public void InvoiceType_Invoice_CanBeSet()
    {
        var type = new InvoiceType { Invoice = "standard" };
        type.Invoice.Should().Be("standard");
    }

    [Fact]
    public void InvoiceType_Invoice_ConvertsToLowercase()
    {
        var type = new InvoiceType { Invoice = "STANDARD" };
        type.Invoice.Should().Be("standard");
    }

    [Fact]
    public void InvoiceType_Invoice_ThrowsOnEmpty()
    {
        var type = new InvoiceType();
        Action act = () => type.Invoice = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void InvoiceType_InvoiceSubType_CanBeSet()
    {
        var type = new InvoiceType { InvoiceSubType = "invoice" };
        type.InvoiceSubType.Should().Be("invoice");
    }

    [Fact]
    public void InvoiceType_InvoiceSubType_ConvertsToLowercase()
    {
        var type = new InvoiceType { InvoiceSubType = "INVOICE" };
        type.InvoiceSubType.Should().Be("invoice");
    }

    [Fact]
    public void InvoiceType_InvoiceSubType_ThrowsOnEmpty()
    {
        var type = new InvoiceType();
        Action act = () => type.InvoiceSubType = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void InvoiceType_BooleanFlags_DefaultToFalse()
    {
        var type = new InvoiceType();
        type.IsExportInvoice.Should().BeFalse();
        type.IsThirdParty.Should().BeFalse();
        type.IsNominal.Should().BeFalse();
        type.IsSummary.Should().BeFalse();
        type.IsSelfBilled.Should().BeFalse();
    }

    [Fact]
    public void InvoiceType_BooleanFlags_CanBeSet()
    {
        var type = new InvoiceType
        {
            IsExportInvoice = true,
            IsThirdParty = true,
            IsNominal = true,
            IsSummary = true,
            IsSelfBilled = true
        };

        type.IsExportInvoice.Should().BeTrue();
        type.IsThirdParty.Should().BeTrue();
        type.IsNominal.Should().BeTrue();
        type.IsSummary.Should().BeTrue();
        type.IsSelfBilled.Should().BeTrue();
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeCode_Invoice_Returns388()
    {
        var type = new InvoiceType { InvoiceSubType = "invoice" };
        type.GetInvoiceTypeCode().Should().Be(388);
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeCode_Debit_Returns383()
    {
        var type = new InvoiceType { InvoiceSubType = "debit" };
        type.GetInvoiceTypeCode().Should().Be(383);
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeCode_Credit_Returns381()
    {
        var type = new InvoiceType { InvoiceSubType = "credit" };
        type.GetInvoiceTypeCode().Should().Be(381);
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeCode_Prepayment_Returns386()
    {
        var type = new InvoiceType { InvoiceSubType = "prepayment" };
        type.GetInvoiceTypeCode().Should().Be(386);
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeCode_Invalid_ThrowsException()
    {
        var type = new InvoiceType { InvoiceSubType = "invalid" };
        Action act = () => type.GetInvoiceTypeCode();
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid*");
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeValue_Standard_Invoice()
    {
        var type = new InvoiceType { Invoice = "standard", InvoiceSubType = "invoice" };
        var result = type.GetInvoiceTypeValue();
        result.Should().StartWith("01");
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeValue_Simplified_Invoice()
    {
        var type = new InvoiceType { Invoice = "simplified", InvoiceSubType = "invoice" };
        var result = type.GetInvoiceTypeValue();
        result.Should().StartWith("02");
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeValue_Simplified_Prepayment_ReturnsStandard()
    {
        var type = new InvoiceType { Invoice = "simplified", InvoiceSubType = "prepayment" };
        var result = type.GetInvoiceTypeValue();
        result.Should().StartWith("01");
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeValue_WithFlags()
    {
        var type = new InvoiceType
        {
            Invoice = "standard",
            InvoiceSubType = "invoice",
            IsThirdParty = true,
            IsNominal = false,
            IsExportInvoice = true,
            IsSummary = false,
            IsSelfBilled = true
        };
        var result = type.GetInvoiceTypeValue();
        result.Should().Be("0110101");
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeValue_AllFlagsTrue()
    {
        var type = new InvoiceType
        {
            Invoice = "standard",
            InvoiceSubType = "invoice",
            IsThirdParty = true,
            IsNominal = true,
            IsExportInvoice = true,
            IsSummary = true,
            IsSelfBilled = true
        };
        var result = type.GetInvoiceTypeValue();
        result.Should().Be("0111111");
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeValue_ThrowsWhenMissingInvoice()
    {
        var type = new InvoiceType { InvoiceSubType = "invoice" };
        Action act = () => type.GetInvoiceTypeValue();
        act.Should().Throw<ArgumentException>().WithMessage("*must be set*");
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeValue_ThrowsWhenMissingInvoiceSubType()
    {
        var type = new InvoiceType { Invoice = "standard" };
        Action act = () => type.GetInvoiceTypeValue();
        act.Should().Throw<ArgumentException>().WithMessage("*must be set*");
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeValue_ThrowsOnInvalidInvoiceCategory()
    {
        var type = new InvoiceType { Invoice = "invalid", InvoiceSubType = "invoice" };
        Action act = () => type.GetInvoiceTypeValue();
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid*category*");
    }

    [Fact]
    public void InvoiceType_GetInvoiceTypeValue_ThrowsOnInvalidSubType()
    {
        var type = new InvoiceType { Invoice = "standard", InvoiceSubType = "invalid" };
        Action act = () => type.GetInvoiceTypeValue();
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid*type*");
    }

    [Fact]
    public void InvoiceType_Validate_ThrowsWhenMissingInvoiceSubType()
    {
        var type = new InvoiceType { Invoice = "standard" };
        Action act = () => type.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*must be set*");
    }

    [Fact]
    public void InvoiceType_Validate_ThrowsWhenMissingInvoice()
    {
        var type = new InvoiceType { InvoiceSubType = "invoice" };
        Action act = () => type.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*must be set*");
    }

    [Fact]
    public void InvoiceType_Validate_PassesWithBothFields()
    {
        var type = new InvoiceType { Invoice = "standard", InvoiceSubType = "invoice" };
        Action act = () => type.Validate();
        act.Should().NotThrow();
    }

    #endregion
}
