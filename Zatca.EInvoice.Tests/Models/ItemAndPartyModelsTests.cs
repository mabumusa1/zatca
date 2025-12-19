using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.Party;

namespace Zatca.EInvoice.Tests.Models;

public class ItemAndPartyModelsTests
{
    #region ClassifiedTaxCategory Tests

    [Fact]
    public void ClassifiedTaxCategory_UNCL5305_Constant()
    {
        ClassifiedTaxCategory.UNCL5305.Should().Be("UNCL5305");
    }

    [Fact]
    public void ClassifiedTaxCategory_Id_AutoDerived_Standard()
    {
        var category = new ClassifiedTaxCategory { Percent = 15m };
        category.Id.Should().Be("S");
    }

    [Fact]
    public void ClassifiedTaxCategory_Id_AutoDerived_Reduced()
    {
        var category = new ClassifiedTaxCategory { Percent = 10m };
        category.Id.Should().Be("AA");
    }

    [Fact]
    public void ClassifiedTaxCategory_Id_AutoDerived_Zero()
    {
        var category = new ClassifiedTaxCategory { Percent = 0m };
        category.Id.Should().Be("Z");
    }

    [Fact]
    public void ClassifiedTaxCategory_Id_ExplicitOverridesAutoDerived()
    {
        var category = new ClassifiedTaxCategory { Percent = 15m, Id = "E" };
        category.Id.Should().Be("E");
    }

    [Fact]
    public void ClassifiedTaxCategory_Id_ThrowsOnEmpty()
    {
        var category = new ClassifiedTaxCategory();
        Action act = () => category.Id = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void ClassifiedTaxCategory_Name_CanBeSet()
    {
        var category = new ClassifiedTaxCategory { Name = "Standard Rate" };
        category.Name.Should().Be("Standard Rate");
    }

    [Fact]
    public void ClassifiedTaxCategory_Name_ThrowsOnEmpty()
    {
        var category = new ClassifiedTaxCategory();
        Action act = () => category.Name = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void ClassifiedTaxCategory_Percent_ThrowsOnNegative()
    {
        var category = new ClassifiedTaxCategory();
        Action act = () => category.Percent = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void ClassifiedTaxCategory_SchemeID_CanBeSet()
    {
        var category = new ClassifiedTaxCategory { SchemeID = "UNCL5305" };
        category.SchemeID.Should().Be("UNCL5305");
    }

    [Fact]
    public void ClassifiedTaxCategory_SchemeID_ThrowsOnEmpty()
    {
        var category = new ClassifiedTaxCategory();
        Action act = () => category.SchemeID = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void ClassifiedTaxCategory_SchemeName_CanBeSet()
    {
        var category = new ClassifiedTaxCategory { SchemeName = "UN/ECE 5305" };
        category.SchemeName.Should().Be("UN/ECE 5305");
    }

    [Fact]
    public void ClassifiedTaxCategory_SchemeName_ThrowsOnEmpty()
    {
        var category = new ClassifiedTaxCategory();
        Action act = () => category.SchemeName = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void ClassifiedTaxCategory_TaxExemptionReason_CanBeSet()
    {
        var category = new ClassifiedTaxCategory { TaxExemptionReason = "Export" };
        category.TaxExemptionReason.Should().Be("Export");
    }

    [Fact]
    public void ClassifiedTaxCategory_TaxExemptionReason_ThrowsOnEmpty()
    {
        var category = new ClassifiedTaxCategory();
        Action act = () => category.TaxExemptionReason = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void ClassifiedTaxCategory_TaxExemptionReasonCode_CanBeSet()
    {
        var category = new ClassifiedTaxCategory { TaxExemptionReasonCode = "VATEX-SA-29" };
        category.TaxExemptionReasonCode.Should().Be("VATEX-SA-29");
    }

    [Fact]
    public void ClassifiedTaxCategory_TaxExemptionReasonCode_ThrowsOnEmpty()
    {
        var category = new ClassifiedTaxCategory();
        Action act = () => category.TaxExemptionReasonCode = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void ClassifiedTaxCategory_TaxScheme_CanBeSet()
    {
        var category = new ClassifiedTaxCategory { TaxScheme = new TaxScheme { Id = "VAT" } };
        category.TaxScheme.Should().NotBeNull();
    }

    [Fact]
    public void ClassifiedTaxCategory_Validate_ThrowsWhenIdMissing()
    {
        var category = new ClassifiedTaxCategory();
        Action act = () => category.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*ID*");
    }

    [Fact]
    public void ClassifiedTaxCategory_Validate_ThrowsWhenPercentMissing()
    {
        var category = new ClassifiedTaxCategory { Id = "S" };
        Action act = () => category.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*percent*");
    }

    [Fact]
    public void ClassifiedTaxCategory_Validate_PassesWithValidData()
    {
        var category = new ClassifiedTaxCategory { Id = "S", Percent = 15m };
        Action act = () => category.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region Item Tests

    [Fact]
    public void Item_Name_CanBeSet()
    {
        var item = new Item { Name = "Product A" };
        item.Name.Should().Be("Product A");
    }

    [Fact]
    public void Item_Name_ThrowsOnEmpty()
    {
        var item = new Item();
        Action act = () => item.Name = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Item_Description_CanBeSet()
    {
        var item = new Item { Description = "A great product" };
        item.Description.Should().Be("A great product");
    }

    [Fact]
    public void Item_Description_ThrowsOnEmpty()
    {
        var item = new Item();
        Action act = () => item.Description = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Item_StandardItemIdentification_CanBeSet()
    {
        var item = new Item { StandardItemIdentification = "GTIN123456" };
        item.StandardItemIdentification.Should().Be("GTIN123456");
    }

    [Fact]
    public void Item_BuyersItemIdentification_CanBeSet()
    {
        var item = new Item { BuyersItemIdentification = "BUY-001" };
        item.BuyersItemIdentification.Should().Be("BUY-001");
    }

    [Fact]
    public void Item_BuyersItemIdentification_ThrowsOnEmpty()
    {
        var item = new Item();
        Action act = () => item.BuyersItemIdentification = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Item_SellersItemIdentification_CanBeSet()
    {
        var item = new Item { SellersItemIdentification = "SELL-001" };
        item.SellersItemIdentification.Should().Be("SELL-001");
    }

    [Fact]
    public void Item_SellersItemIdentification_ThrowsOnEmpty()
    {
        var item = new Item();
        Action act = () => item.SellersItemIdentification = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Item_ClassifiedTaxCategories_CanBeSet()
    {
        var item = new Item
        {
            ClassifiedTaxCategories = new List<ClassifiedTaxCategory>
            {
                new ClassifiedTaxCategory { Id = "S", Percent = 15m }
            }
        };
        item.ClassifiedTaxCategories.Should().HaveCount(1);
    }

    #endregion

    #region Price Tests

    [Fact]
    public void Price_UnitCode_DefaultsToUNIT()
    {
        var price = new Price();
        price.UnitCode.Should().Be("C62");
    }

    [Fact]
    public void Price_PriceAmount_CanBeSet()
    {
        var price = new Price { PriceAmount = 100m };
        price.PriceAmount.Should().Be(100m);
    }

    [Fact]
    public void Price_PriceAmount_ThrowsOnNegative()
    {
        var price = new Price();
        Action act = () => price.PriceAmount = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void Price_BaseQuantity_CanBeSet()
    {
        var price = new Price { BaseQuantity = 1m };
        price.BaseQuantity.Should().Be(1m);
    }

    [Fact]
    public void Price_BaseQuantity_ThrowsOnNegative()
    {
        var price = new Price();
        Action act = () => price.BaseQuantity = -1m;
        act.Should().Throw<ArgumentException>().WithMessage("*non-negative*");
    }

    [Fact]
    public void Price_UnitCode_CanBeSet()
    {
        var price = new Price { UnitCode = "KG" };
        price.UnitCode.Should().Be("KG");
    }

    [Fact]
    public void Price_UnitCode_ThrowsOnEmpty()
    {
        var price = new Price();
        Action act = () => price.UnitCode = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Price_UnitCode_KeepsDefaultOnNull()
    {
        var price = new Price();
        var originalUnitCode = price.UnitCode;
        price.UnitCode = null!;
        price.UnitCode.Should().Be(originalUnitCode);
    }

    [Fact]
    public void Price_AllowanceCharges_CanBeSet()
    {
        var price = new Price
        {
            AllowanceCharges = new List<AllowanceCharge>
            {
                new AllowanceCharge { Amount = 10m }
            }
        };
        price.AllowanceCharges.Should().HaveCount(1);
    }

    [Fact]
    public void Price_Validate_ThrowsWhenPriceAmountMissing()
    {
        var price = new Price();
        Action act = () => price.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*Price amount*");
    }

    [Fact]
    public void Price_Validate_PassesWithPriceAmount()
    {
        var price = new Price { PriceAmount = 100m };
        Action act = () => price.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region Address Tests

    [Fact]
    public void Address_StreetName_CanBeSet()
    {
        var address = new Address { StreetName = "Prince Sultan" };
        address.StreetName.Should().Be("Prince Sultan");
    }

    [Fact]
    public void Address_StreetName_ThrowsOnEmpty()
    {
        var address = new Address();
        Action act = () => address.StreetName = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Address_AdditionalStreetName_CanBeSet()
    {
        var address = new Address { AdditionalStreetName = "Building A" };
        address.AdditionalStreetName.Should().Be("Building A");
    }

    [Fact]
    public void Address_AdditionalStreetName_ThrowsOnEmpty()
    {
        var address = new Address();
        Action act = () => address.AdditionalStreetName = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Address_BuildingNumber_CanBeSet()
    {
        var address = new Address { BuildingNumber = "2322" };
        address.BuildingNumber.Should().Be("2322");
    }

    [Fact]
    public void Address_BuildingNumber_ThrowsOnEmpty()
    {
        var address = new Address();
        Action act = () => address.BuildingNumber = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Address_PlotIdentification_CanBeSet()
    {
        var address = new Address { PlotIdentification = "1234" };
        address.PlotIdentification.Should().Be("1234");
    }

    [Fact]
    public void Address_PlotIdentification_ThrowsOnEmpty()
    {
        var address = new Address();
        Action act = () => address.PlotIdentification = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Address_CityName_CanBeSet()
    {
        var address = new Address { CityName = "Riyadh" };
        address.CityName.Should().Be("Riyadh");
    }

    [Fact]
    public void Address_CityName_ThrowsOnEmpty()
    {
        var address = new Address();
        Action act = () => address.CityName = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Address_PostalZone_CanBeSet()
    {
        var address = new Address { PostalZone = "12345" };
        address.PostalZone.Should().Be("12345");
    }

    [Fact]
    public void Address_PostalZone_ThrowsOnEmpty()
    {
        var address = new Address();
        Action act = () => address.PostalZone = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Address_Country_CanBeSet()
    {
        var address = new Address { Country = "SA" };
        address.Country.Should().Be("SA");
    }

    [Fact]
    public void Address_Country_ThrowsOnEmpty()
    {
        var address = new Address();
        Action act = () => address.Country = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Address_CountrySubentity_CanBeSet()
    {
        var address = new Address { CountrySubentity = "Riyadh Province" };
        address.CountrySubentity.Should().Be("Riyadh Province");
    }

    [Fact]
    public void Address_CountrySubentity_ThrowsOnEmpty()
    {
        var address = new Address();
        Action act = () => address.CountrySubentity = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Address_CitySubdivisionName_CanBeSet()
    {
        var address = new Address { CitySubdivisionName = "Al-Olaya" };
        address.CitySubdivisionName.Should().Be("Al-Olaya");
    }

    [Fact]
    public void Address_CitySubdivisionName_ThrowsOnEmpty()
    {
        var address = new Address();
        Action act = () => address.CitySubdivisionName = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    #endregion

    #region PartyTaxScheme Tests

    [Fact]
    public void PartyTaxScheme_CompanyId_CanBeSet()
    {
        var scheme = new PartyTaxScheme { CompanyId = "399999999900003" };
        scheme.CompanyId.Should().Be("399999999900003");
    }

    [Fact]
    public void PartyTaxScheme_CompanyId_ThrowsOnEmpty()
    {
        var scheme = new PartyTaxScheme();
        Action act = () => scheme.CompanyId = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void PartyTaxScheme_TaxScheme_CanBeSet()
    {
        var scheme = new PartyTaxScheme { TaxScheme = new TaxScheme { Id = "VAT" } };
        scheme.TaxScheme.Should().NotBeNull();
    }

    [Fact]
    public void PartyTaxScheme_Validate_ThrowsWhenTaxSchemeMissing()
    {
        var scheme = new PartyTaxScheme();
        Action act = () => scheme.Validate();
        act.Should().Throw<ArgumentException>().WithMessage("*TaxScheme*");
    }

    [Fact]
    public void PartyTaxScheme_Validate_PassesWithTaxScheme()
    {
        var scheme = new PartyTaxScheme { TaxScheme = new TaxScheme() };
        Action act = () => scheme.Validate();
        act.Should().NotThrow();
    }

    #endregion

    #region Party Tests

    [Fact]
    public void Party_PartyIdentification_CanBeSet()
    {
        var party = new Party { PartyIdentification = "123456" };
        party.PartyIdentification.Should().Be("123456");
    }

    [Fact]
    public void Party_PartyIdentification_ThrowsOnEmpty()
    {
        var party = new Party();
        Action act = () => party.PartyIdentification = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Party_PartyIdentificationId_CanBeSet()
    {
        var party = new Party { PartyIdentificationId = "CRN" };
        party.PartyIdentificationId.Should().Be("CRN");
    }

    [Fact]
    public void Party_PartyIdentificationId_ThrowsOnEmpty()
    {
        var party = new Party();
        Action act = () => party.PartyIdentificationId = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void Party_PostalAddress_CanBeSet()
    {
        var party = new Party { PostalAddress = new Address { CityName = "Riyadh" } };
        party.PostalAddress.Should().NotBeNull();
    }

    [Fact]
    public void Party_PartyTaxScheme_CanBeSet()
    {
        var party = new Party { PartyTaxScheme = new PartyTaxScheme { CompanyId = "123" } };
        party.PartyTaxScheme.Should().NotBeNull();
    }

    [Fact]
    public void Party_LegalEntity_CanBeSet()
    {
        var party = new Party { LegalEntity = new LegalEntity { RegistrationName = "Test Company" } };
        party.LegalEntity.Should().NotBeNull();
    }

    #endregion

    #region LegalEntity Tests

    [Fact]
    public void LegalEntity_RegistrationName_CanBeSet()
    {
        var entity = new LegalEntity { RegistrationName = "Test Company LLC" };
        entity.RegistrationName.Should().Be("Test Company LLC");
    }

    [Fact]
    public void LegalEntity_RegistrationName_ThrowsOnEmpty()
    {
        var entity = new LegalEntity();
        Action act = () => entity.RegistrationName = "";
        act.Should().Throw<ArgumentException>().WithMessage("*empty*");
    }

    [Fact]
    public void LegalEntity_RegistrationName_AcceptsNull()
    {
        var entity = new LegalEntity { RegistrationName = "Test" };
        entity.RegistrationName = null;
        entity.RegistrationName.Should().BeNull();
    }

    #endregion
}
