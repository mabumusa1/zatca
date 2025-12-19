using System.Xml.Linq;
using Zatca.EInvoice.Xml;

namespace Zatca.EInvoice.Tests.Xml;

public class XmlSerializationExtensionsTests
{
    #region FormatAmount Tests

    [Fact]
    public void FormatAmount_Decimal_FormatsWithTwoDecimals()
    {
        var amount = 123.456m;
        var result = amount.FormatAmount();
        result.Should().Be("123.46");
    }

    [Fact]
    public void FormatAmount_Decimal_HandlesZero()
    {
        var amount = 0m;
        var result = amount.FormatAmount();
        result.Should().Be("0.00");
    }

    [Fact]
    public void FormatAmount_Decimal_HandlesLargeNumbers()
    {
        var amount = 1234567890.12m;
        var result = amount.FormatAmount();
        result.Should().Be("1234567890.12");
    }

    [Fact]
    public void FormatAmount_Double_FormatsWithTwoDecimals()
    {
        var amount = 123.456;
        var result = amount.FormatAmount();
        result.Should().Be("123.46");
    }

    [Fact]
    public void FormatAmount_Double_HandlesZero()
    {
        var amount = 0.0;
        var result = amount.FormatAmount();
        result.Should().Be("0.00");
    }

    #endregion

    #region FormatPrice Tests

    [Fact]
    public void FormatPrice_Decimal_FormatsWithFourDecimals()
    {
        var price = 123.456789m;
        var result = price.FormatPrice();
        result.Should().Be("123.4568");
    }

    [Fact]
    public void FormatPrice_Decimal_HandlesZero()
    {
        var price = 0m;
        var result = price.FormatPrice();
        result.Should().Be("0.0000");
    }

    [Fact]
    public void FormatPrice_Double_FormatsWithFourDecimals()
    {
        var price = 123.456789;
        var result = price.FormatPrice();
        result.Should().Be("123.4568");
    }

    #endregion

    #region FormatQuantity Tests

    [Fact]
    public void FormatQuantity_Decimal_FormatsWithSixDecimals()
    {
        var quantity = 123.45678901m;
        var result = quantity.FormatQuantity();
        result.Should().Be("123.456789");
    }

    [Fact]
    public void FormatQuantity_Decimal_HandlesZero()
    {
        var quantity = 0m;
        var result = quantity.FormatQuantity();
        result.Should().Be("0.000000");
    }

    [Fact]
    public void FormatQuantity_Double_FormatsWithSixDecimals()
    {
        var quantity = 123.45678901;
        var result = quantity.FormatQuantity();
        result.Should().Be("123.456789");
    }

    #endregion

    #region CreateAmountElement Tests

    [Fact]
    public void CreateAmountElement_Decimal_CreatesElementWithCurrencyAttribute()
    {
        var elementName = XName.Get("Amount", "urn:test");
        var element = XmlSerializationExtensions.CreateAmountElement(elementName, 100.50m, "SAR");

        element.Name.Should().Be(elementName);
        element.Attribute("currencyID")?.Value.Should().Be("SAR");
        element.Value.Should().Be("100.50");
    }

    [Fact]
    public void CreateAmountElement_Decimal_DefaultsCurrencyToSAR()
    {
        var elementName = XName.Get("Amount");
        var element = XmlSerializationExtensions.CreateAmountElement(elementName, 100m);

        element.Attribute("currencyID")?.Value.Should().Be("SAR");
    }

    [Fact]
    public void CreateAmountElement_Double_CreatesElementWithCurrencyAttribute()
    {
        var elementName = XName.Get("Amount");
        var element = XmlSerializationExtensions.CreateAmountElement(elementName, 100.50, "USD");

        element.Attribute("currencyID")?.Value.Should().Be("USD");
        element.Value.Should().Be("100.50");
    }

    #endregion

    #region CreatePriceElement Tests

    [Fact]
    public void CreatePriceElement_Decimal_CreatesElementWithCurrencyAttribute()
    {
        var elementName = XName.Get("Price");
        var element = XmlSerializationExtensions.CreatePriceElement(elementName, 10.1234m, "SAR");

        element.Attribute("currencyID")?.Value.Should().Be("SAR");
        element.Value.Should().Be("10.1234");
    }

    [Fact]
    public void CreatePriceElement_Double_CreatesElementWithCurrencyAttribute()
    {
        var elementName = XName.Get("Price");
        var element = XmlSerializationExtensions.CreatePriceElement(elementName, 10.1234, "SAR");

        element.Attribute("currencyID")?.Value.Should().Be("SAR");
        element.Value.Should().Be("10.1234");
    }

    #endregion

    #region CreateQuantityElement Tests

    [Fact]
    public void CreateQuantityElement_Decimal_CreatesElementWithUnitCodeAttribute()
    {
        var elementName = XName.Get("Quantity");
        var element = XmlSerializationExtensions.CreateQuantityElement(elementName, 5.5m, "PCE");

        element.Attribute("unitCode")?.Value.Should().Be("PCE");
        element.Value.Should().Be("5.500000");
    }

    [Fact]
    public void CreateQuantityElement_Double_CreatesElementWithUnitCodeAttribute()
    {
        var elementName = XName.Get("Quantity");
        var element = XmlSerializationExtensions.CreateQuantityElement(elementName, 5.5, "KG");

        element.Attribute("unitCode")?.Value.Should().Be("KG");
        element.Value.Should().Be("5.500000");
    }

    #endregion

    #region FormatDate Tests

    [Fact]
    public void FormatDate_FormatsToYYYYMMDD()
    {
        var date = new DateTime(2024, 9, 7);
        var result = date.FormatDate();
        result.Should().Be("2024-09-07");
    }

    [Fact]
    public void FormatDate_HandlesFirstDayOfYear()
    {
        var date = new DateTime(2024, 1, 1);
        var result = date.FormatDate();
        result.Should().Be("2024-01-01");
    }

    [Fact]
    public void FormatDate_HandlesLastDayOfYear()
    {
        var date = new DateTime(2024, 12, 31);
        var result = date.FormatDate();
        result.Should().Be("2024-12-31");
    }

    #endregion

    #region FormatTime Tests

    [Fact]
    public void FormatTime_FormatsToHHMMSS()
    {
        var time = new DateTime(2024, 1, 1, 17, 41, 8);
        var result = time.FormatTime();
        result.Should().Be("17:41:08");
    }

    [Fact]
    public void FormatTime_HandlesMidnight()
    {
        var time = new DateTime(2024, 1, 1, 0, 0, 0);
        var result = time.FormatTime();
        result.Should().Be("00:00:00");
    }

    [Fact]
    public void FormatTime_HandlesEndOfDay()
    {
        var time = new DateTime(2024, 1, 1, 23, 59, 59);
        var result = time.FormatTime();
        result.Should().Be("23:59:59");
    }

    #endregion

    #region FormatPercent Tests

    [Fact]
    public void FormatPercent_Decimal_FormatsWithTwoDecimals()
    {
        var percent = 15.555m;
        var result = percent.FormatPercent();
        result.Should().Be("15.56");
    }

    [Fact]
    public void FormatPercent_Decimal_HandlesZero()
    {
        var percent = 0m;
        var result = percent.FormatPercent();
        result.Should().Be("0.00");
    }

    [Fact]
    public void FormatPercent_Double_FormatsWithTwoDecimals()
    {
        var percent = 15.555;
        var result = percent.FormatPercent();
        result.Should().Be("15.55");
    }

    [Fact]
    public void FormatPercent_Double_HandlesHundredPercent()
    {
        var percent = 100.0;
        var result = percent.FormatPercent();
        result.Should().Be("100.00");
    }

    #endregion
}
