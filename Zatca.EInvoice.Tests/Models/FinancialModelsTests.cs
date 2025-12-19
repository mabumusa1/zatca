using Zatca.EInvoice.Models.Financial;

namespace Zatca.EInvoice.Tests.Models;

public class FinancialModelsTests
{
    #region AllowanceCharge Tests

    [Fact]
    public void AllowanceCharge_ChargeIndicator_CanBeSet()
    {
        var charge = new AllowanceCharge { ChargeIndicator = true };
        charge.ChargeIndicator.Should().BeTrue();

        charge.ChargeIndicator = false;
        charge.ChargeIndicator.Should().BeFalse();
    }

    [Fact]
    public void AllowanceCharge_AllowanceChargeReasonCode_AcceptsValidValues()
    {
        var charge = new AllowanceCharge();
        charge.AllowanceChargeReasonCode = "95";
        charge.AllowanceChargeReasonCode.Should().Be("95");

        charge.AllowanceChargeReasonCode = null;
        charge.AllowanceChargeReasonCode.Should().BeNull();
    }

    [Fact]
    public void AllowanceCharge_AllowanceChargeReasonCode_ThrowsOnNegativeNumeric()
    {
        var charge = new AllowanceCharge();
        Action act = () => charge.AllowanceChargeReasonCode = "-1";
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void AllowanceCharge_AllowanceChargeReason_AcceptsValidValues()
    {
        var charge = new AllowanceCharge { AllowanceChargeReason = "Discount" };
        charge.AllowanceChargeReason.Should().Be("Discount");
    }

    [Fact]
    public void AllowanceCharge_AllowanceChargeReason_ThrowsOnEmpty()
    {
        var charge = new AllowanceCharge();
        Action act = () => charge.AllowanceChargeReason = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void AllowanceCharge_MultiplierFactorNumeric_AcceptsValidValues()
    {
        var charge = new AllowanceCharge { MultiplierFactorNumeric = 10 };
        charge.MultiplierFactorNumeric.Should().Be(10);
    }

    [Fact]
    public void AllowanceCharge_MultiplierFactorNumeric_ThrowsOnNegative()
    {
        var charge = new AllowanceCharge();
        Action act = () => charge.MultiplierFactorNumeric = -1;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void AllowanceCharge_BaseAmount_AcceptsValidValues()
    {
        var charge = new AllowanceCharge { BaseAmount = 100m };
        charge.BaseAmount.Should().Be(100m);
    }

    [Fact]
    public void AllowanceCharge_BaseAmount_ThrowsOnNegative()
    {
        var charge = new AllowanceCharge();
        Action act = () => charge.BaseAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void AllowanceCharge_Amount_AcceptsValidValues()
    {
        var charge = new AllowanceCharge { Amount = 50m };
        charge.Amount.Should().Be(50m);
    }

    [Fact]
    public void AllowanceCharge_Amount_ThrowsOnNegative()
    {
        var charge = new AllowanceCharge();
        Action act = () => charge.Amount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void AllowanceCharge_TaxTotal_CanBeSet()
    {
        var charge = new AllowanceCharge { TaxTotal = new TaxTotal { TaxAmount = 15m } };
        charge.TaxTotal.Should().NotBeNull();
        charge.TaxTotal!.TaxAmount.Should().Be(15m);
    }

    [Fact]
    public void AllowanceCharge_TaxCategories_CanBeSet()
    {
        var charge = new AllowanceCharge
        {
            TaxCategories = new List<TaxCategory> { new TaxCategory { Id = "S", Percent = 15m } }
        };
        charge.TaxCategories.Should().HaveCount(1);
    }

    #endregion

    #region TaxSubTotal Tests

    [Fact]
    public void TaxSubTotal_Properties_CanBeSet()
    {
        var subTotal = new TaxSubTotal
        {
            TaxableAmount = 100m,
            TaxAmount = 15m,
            Percent = 15m,
            TaxCategory = new TaxCategory { Id = "S" }
        };

        subTotal.TaxableAmount.Should().Be(100m);
        subTotal.TaxAmount.Should().Be(15m);
        subTotal.Percent.Should().Be(15m);
        subTotal.TaxCategory.Should().NotBeNull();
    }

    [Fact]
    public void TaxSubTotal_Validate_ThrowsWhenTaxableAmountMissing()
    {
        var subTotal = new TaxSubTotal { TaxAmount = 15m, TaxCategory = new TaxCategory() };
        Action act = () => subTotal.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*taxableAmount*");
    }

    [Fact]
    public void TaxSubTotal_Validate_ThrowsWhenTaxAmountMissing()
    {
        var subTotal = new TaxSubTotal { TaxableAmount = 100m, TaxCategory = new TaxCategory() };
        Action act = () => subTotal.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*taxAmount*");
    }

    [Fact]
    public void TaxSubTotal_Validate_ThrowsWhenTaxCategoryMissing()
    {
        var subTotal = new TaxSubTotal { TaxableAmount = 100m, TaxAmount = 15m };
        Action act = () => subTotal.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*taxCategory*");
    }

    [Fact]
    public void TaxSubTotal_Validate_PassesWithAllFields()
    {
        var subTotal = new TaxSubTotal
        {
            TaxableAmount = 100m,
            TaxAmount = 15m,
            TaxCategory = new TaxCategory()
        };
        Action act = () => subTotal.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region TaxCategory Tests

    [Fact]
    public void TaxCategory_Constants_AreCorrect()
    {
        TaxCategory.UNCL5305.Should().Be("UN/ECE 5305");
        TaxCategory.UNCL5153.Should().Be("UN/ECE 5153");
    }

    [Fact]
    public void TaxCategory_Id_AutoDerived_Standard()
    {
        var category = new TaxCategory { Percent = 15m };
        category.Id.Should().Be("S");
    }

    [Fact]
    public void TaxCategory_Id_AutoDerived_Reduced()
    {
        var category = new TaxCategory { Percent = 10m };
        category.Id.Should().Be("AA");
    }

    [Fact]
    public void TaxCategory_Id_AutoDerived_Zero()
    {
        var category = new TaxCategory { Percent = 0m };
        category.Id.Should().Be("Z");
    }

    [Fact]
    public void TaxCategory_Id_ExplicitOverridesAutoDerived()
    {
        var category = new TaxCategory { Percent = 15m, Id = "E" };
        category.Id.Should().Be("E");
    }

    [Fact]
    public void TaxCategory_Id_ReturnsNullWhenNoPercentAndNoExplicit()
    {
        var category = new TaxCategory();
        category.Id.Should().BeNull();
    }

    [Fact]
    public void TaxCategory_IdAttributes_HasDefaultValues()
    {
        var category = new TaxCategory();
        category.IdAttributes.Should().ContainKey("schemeID");
        category.IdAttributes["schemeID"].Should().Be("UN/ECE 5305");
    }

    [Fact]
    public void TaxCategory_TaxSchemeAttributes_HasDefaultValues()
    {
        var category = new TaxCategory();
        category.TaxSchemeAttributes.Should().ContainKey("schemeID");
        category.TaxSchemeAttributes["schemeID"].Should().Be("UN/ECE 5153");
    }

    [Fact]
    public void TaxCategory_TaxExemptionReason_CanBeSet()
    {
        var category = new TaxCategory { TaxExemptionReason = "Export" };
        category.TaxExemptionReason.Should().Be("Export");
    }

    [Fact]
    public void TaxCategory_TaxExemptionReasonCode_CanBeSet()
    {
        var category = new TaxCategory { TaxExemptionReasonCode = "VATEX-SA-29" };
        category.TaxExemptionReasonCode.Should().Be("VATEX-SA-29");
    }

    [Fact]
    public void TaxCategory_Validate_ThrowsWhenIdMissing()
    {
        var category = new TaxCategory();
        Action act = () => category.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*tax category id*");
    }

    [Fact]
    public void TaxCategory_Validate_ThrowsWhenPercentMissing()
    {
        var category = new TaxCategory { Id = "S" };
        Action act = () => category.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*percent*");
    }

    [Fact]
    public void TaxCategory_Validate_PassesWithValidData()
    {
        var category = new TaxCategory { Id = "S", Percent = 15m };
        Action act = () => category.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region TaxScheme Tests

    [Fact]
    public void TaxScheme_Id_CanBeSet()
    {
        var scheme = new TaxScheme { Id = "VAT" };
        scheme.Id.Should().Be("VAT");
    }

    [Fact]
    public void TaxScheme_Id_ThrowsOnEmpty()
    {
        var scheme = new TaxScheme();
        Action act = () => scheme.Id = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void TaxScheme_Id_AcceptsNull()
    {
        var scheme = new TaxScheme { Id = "VAT" };
        scheme.Id = null;
        scheme.Id.Should().BeNull();
    }

    [Fact]
    public void TaxScheme_TaxTypeCode_CanBeSet()
    {
        var scheme = new TaxScheme { TaxTypeCode = "VAT" };
        scheme.TaxTypeCode.Should().Be("VAT");
    }

    [Fact]
    public void TaxScheme_Name_CanBeSet()
    {
        var scheme = new TaxScheme { Name = "Value Added Tax" };
        scheme.Name.Should().Be("Value Added Tax");
    }

    #endregion

    #region TaxTotal Tests

    [Fact]
    public void TaxTotal_TaxAmount_CanBeSet()
    {
        var total = new TaxTotal { TaxAmount = 150m };
        total.TaxAmount.Should().Be(150m);
    }

    [Fact]
    public void TaxTotal_TaxAmount_ThrowsOnNegative()
    {
        var total = new TaxTotal();
        Action act = () => total.TaxAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void TaxTotal_RoundingAmount_CanBeSet()
    {
        var total = new TaxTotal { RoundingAmount = 0.01m };
        total.RoundingAmount.Should().Be(0.01m);
    }

    [Fact]
    public void TaxTotal_RoundingAmount_ThrowsOnNegative()
    {
        var total = new TaxTotal();
        Action act = () => total.RoundingAmount = -0.01m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void TaxTotal_TaxSubTotals_DefaultsToEmptyList()
    {
        var total = new TaxTotal();
        total.TaxSubTotals.Should().NotBeNull();
        total.TaxSubTotals.Should().BeEmpty();
    }

    [Fact]
    public void TaxTotal_AddTaxSubTotal_AddsToList()
    {
        var total = new TaxTotal();
        var subTotal = new TaxSubTotal { TaxableAmount = 100m, TaxAmount = 15m };
        total.AddTaxSubTotal(subTotal);
        total.TaxSubTotals.Should().HaveCount(1);
        total.TaxSubTotals[0].Should().Be(subTotal);
    }

    [Fact]
    public void TaxTotal_Validate_ThrowsWhenTaxAmountMissing()
    {
        var total = new TaxTotal();
        Action act = () => total.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*taxAmount*");
    }

    [Fact]
    public void TaxTotal_Validate_PassesWithTaxAmount()
    {
        var total = new TaxTotal { TaxAmount = 15m };
        Action act = () => total.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region LegalMonetaryTotal Tests

    [Fact]
    public void LegalMonetaryTotal_LineExtensionAmount_CanBeSet()
    {
        var total = new LegalMonetaryTotal { LineExtensionAmount = 1000m };
        total.LineExtensionAmount.Should().Be(1000m);
    }

    [Fact]
    public void LegalMonetaryTotal_LineExtensionAmount_ThrowsOnNegative()
    {
        var total = new LegalMonetaryTotal();
        Action act = () => total.LineExtensionAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void LegalMonetaryTotal_TaxExclusiveAmount_CanBeSet()
    {
        var total = new LegalMonetaryTotal { TaxExclusiveAmount = 1000m };
        total.TaxExclusiveAmount.Should().Be(1000m);
    }

    [Fact]
    public void LegalMonetaryTotal_TaxExclusiveAmount_ThrowsOnNegative()
    {
        var total = new LegalMonetaryTotal();
        Action act = () => total.TaxExclusiveAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void LegalMonetaryTotal_TaxInclusiveAmount_CanBeSet()
    {
        var total = new LegalMonetaryTotal { TaxInclusiveAmount = 1150m };
        total.TaxInclusiveAmount.Should().Be(1150m);
    }

    [Fact]
    public void LegalMonetaryTotal_TaxInclusiveAmount_ThrowsOnNegative()
    {
        var total = new LegalMonetaryTotal();
        Action act = () => total.TaxInclusiveAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void LegalMonetaryTotal_AllowanceTotalAmount_DefaultsToZeroOnNull()
    {
        var total = new LegalMonetaryTotal();
        total.AllowanceTotalAmount = null;
        total.AllowanceTotalAmount.Should().Be(0m);
    }

    [Fact]
    public void LegalMonetaryTotal_AllowanceTotalAmount_ThrowsOnNegative()
    {
        var total = new LegalMonetaryTotal();
        Action act = () => total.AllowanceTotalAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void LegalMonetaryTotal_ChargeTotalAmount_DefaultsToZeroOnNull()
    {
        var total = new LegalMonetaryTotal();
        total.ChargeTotalAmount = null;
        total.ChargeTotalAmount.Should().Be(0m);
    }

    [Fact]
    public void LegalMonetaryTotal_ChargeTotalAmount_ThrowsOnNegative()
    {
        var total = new LegalMonetaryTotal();
        Action act = () => total.ChargeTotalAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void LegalMonetaryTotal_PrepaidAmount_CanBeSet()
    {
        var total = new LegalMonetaryTotal { PrepaidAmount = 100m };
        total.PrepaidAmount.Should().Be(100m);
    }

    [Fact]
    public void LegalMonetaryTotal_PrepaidAmount_ThrowsOnNegative()
    {
        var total = new LegalMonetaryTotal();
        Action act = () => total.PrepaidAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void LegalMonetaryTotal_PayableAmount_CanBeSet()
    {
        var total = new LegalMonetaryTotal { PayableAmount = 1150m };
        total.PayableAmount.Should().Be(1150m);
    }

    [Fact]
    public void LegalMonetaryTotal_PayableAmount_ThrowsOnNegative()
    {
        var total = new LegalMonetaryTotal();
        Action act = () => total.PayableAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    #endregion
}
