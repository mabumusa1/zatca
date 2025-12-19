using System.Text.Json;
using Zatca.EInvoice.Helpers;

namespace Zatca.EInvoice.Tests.Helpers;

public class DictionaryHelperTests
{
    #region GetString Tests

    [Fact]
    public void GetString_NullDictionary_ReturnsDefault()
    {
        var result = DictionaryHelper.GetString(null, "key", "default");
        result.Should().Be("default");
    }

    [Fact]
    public void GetString_MissingKey_ReturnsDefault()
    {
        var dict = new Dictionary<string, object> { { "other", "value" } };
        var result = DictionaryHelper.GetString(dict, "key", "default");
        result.Should().Be("default");
    }

    [Fact]
    public void GetString_NullValue_ReturnsDefault()
    {
        var dict = new Dictionary<string, object> { { "key", null! } };
        var result = DictionaryHelper.GetString(dict, "key", "default");
        result.Should().Be("default");
    }

    [Fact]
    public void GetString_StringValue_ReturnsValue()
    {
        var dict = new Dictionary<string, object> { { "key", "value" } };
        var result = DictionaryHelper.GetString(dict, "key");
        result.Should().Be("value");
    }

    [Fact]
    public void GetString_IntValue_ReturnsStringRepresentation()
    {
        var dict = new Dictionary<string, object> { { "key", 123 } };
        var result = DictionaryHelper.GetString(dict, "key");
        result.Should().Be("123");
    }

    [Fact]
    public void GetString_JsonElementString_ReturnsValue()
    {
        var json = JsonDocument.Parse("{\"key\": \"value\"}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetString(dict, "key");
        result.Should().Be("value");
    }

    [Fact]
    public void GetString_JsonElementNull_ReturnsDefault()
    {
        var json = JsonDocument.Parse("{\"key\": null}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetString(dict, "key", "default");
        result.Should().Be("default");
    }

    #endregion

    #region GetDecimal Tests

    [Fact]
    public void GetDecimal_NullDictionary_ReturnsDefault()
    {
        var result = DictionaryHelper.GetDecimal(null, "key", 99m);
        result.Should().Be(99m);
    }

    [Fact]
    public void GetDecimal_MissingKey_ReturnsDefault()
    {
        var dict = new Dictionary<string, object>();
        var result = DictionaryHelper.GetDecimal(dict, "key", 99m);
        result.Should().Be(99m);
    }

    [Fact]
    public void GetDecimal_DecimalValue_ReturnsValue()
    {
        var dict = new Dictionary<string, object> { { "key", 123.45m } };
        var result = DictionaryHelper.GetDecimal(dict, "key");
        result.Should().Be(123.45m);
    }

    [Fact]
    public void GetDecimal_DoubleValue_ReturnsConvertedValue()
    {
        var dict = new Dictionary<string, object> { { "key", 123.45 } };
        var result = DictionaryHelper.GetDecimal(dict, "key");
        result.Should().Be(123.45m);
    }

    [Fact]
    public void GetDecimal_FloatValue_ReturnsConvertedValue()
    {
        var dict = new Dictionary<string, object> { { "key", 123.45f } };
        var result = DictionaryHelper.GetDecimal(dict, "key");
        result.Should().BeApproximately(123.45m, 0.01m);
    }

    [Fact]
    public void GetDecimal_IntValue_ReturnsConvertedValue()
    {
        var dict = new Dictionary<string, object> { { "key", 123 } };
        var result = DictionaryHelper.GetDecimal(dict, "key");
        result.Should().Be(123m);
    }

    [Fact]
    public void GetDecimal_LongValue_ReturnsConvertedValue()
    {
        var dict = new Dictionary<string, object> { { "key", 123L } };
        var result = DictionaryHelper.GetDecimal(dict, "key");
        result.Should().Be(123m);
    }

    [Fact]
    public void GetDecimal_StringValue_ParsesSuccessfully()
    {
        var dict = new Dictionary<string, object> { { "key", "123.45" } };
        var result = DictionaryHelper.GetDecimal(dict, "key");
        result.Should().Be(123.45m);
    }

    [Fact]
    public void GetDecimal_InvalidString_ReturnsDefault()
    {
        var dict = new Dictionary<string, object> { { "key", "invalid" } };
        var result = DictionaryHelper.GetDecimal(dict, "key", 99m);
        result.Should().Be(99m);
    }

    [Fact]
    public void GetDecimal_JsonElementNumber_ReturnsValue()
    {
        var json = JsonDocument.Parse("{\"key\": 123.45}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetDecimal(dict, "key");
        result.Should().Be(123.45m);
    }

    [Fact]
    public void GetDecimal_JsonElementString_ParsesSuccessfully()
    {
        var json = JsonDocument.Parse("{\"key\": \"123.45\"}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetDecimal(dict, "key");
        result.Should().Be(123.45m);
    }

    [Fact]
    public void GetDecimal_JsonElementNull_ReturnsDefault()
    {
        var json = JsonDocument.Parse("{\"key\": null}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetDecimal(dict, "key", 99m);
        result.Should().Be(99m);
    }

    #endregion

    #region GetBoolean Tests

    [Fact]
    public void GetBoolean_NullDictionary_ReturnsDefault()
    {
        var result = DictionaryHelper.GetBoolean(null, "key", true);
        result.Should().BeTrue();
    }

    [Fact]
    public void GetBoolean_MissingKey_ReturnsDefault()
    {
        var dict = new Dictionary<string, object>();
        var result = DictionaryHelper.GetBoolean(dict, "key", true);
        result.Should().BeTrue();
    }

    [Fact]
    public void GetBoolean_BoolValue_ReturnsValue()
    {
        var dict = new Dictionary<string, object> { { "key", true } };
        var result = DictionaryHelper.GetBoolean(dict, "key");
        result.Should().BeTrue();
    }

    [Fact]
    public void GetBoolean_StringValue_ParsesSuccessfully()
    {
        var dict = new Dictionary<string, object> { { "key", "true" } };
        var result = DictionaryHelper.GetBoolean(dict, "key");
        result.Should().BeTrue();
    }

    [Fact]
    public void GetBoolean_InvalidString_ReturnsDefault()
    {
        var dict = new Dictionary<string, object> { { "key", "invalid" } };
        var result = DictionaryHelper.GetBoolean(dict, "key", true);
        result.Should().BeTrue();
    }

    [Fact]
    public void GetBoolean_JsonElementTrue_ReturnsTrue()
    {
        var json = JsonDocument.Parse("{\"key\": true}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetBoolean(dict, "key");
        result.Should().BeTrue();
    }

    [Fact]
    public void GetBoolean_JsonElementFalse_ReturnsFalse()
    {
        var json = JsonDocument.Parse("{\"key\": false}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetBoolean(dict, "key", true);
        result.Should().BeFalse();
    }

    [Fact]
    public void GetBoolean_JsonElementStringTrue_ReturnsTrue()
    {
        var json = JsonDocument.Parse("{\"key\": \"true\"}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetBoolean(dict, "key");
        result.Should().BeTrue();
    }

    [Fact]
    public void GetBoolean_JsonElementNull_ReturnsDefault()
    {
        var json = JsonDocument.Parse("{\"key\": null}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetBoolean(dict, "key", true);
        result.Should().BeTrue();
    }

    #endregion

    #region GetInt Tests

    [Fact]
    public void GetInt_NullDictionary_ReturnsDefault()
    {
        var result = DictionaryHelper.GetInt(null, "key", 99);
        result.Should().Be(99);
    }

    [Fact]
    public void GetInt_MissingKey_ReturnsDefault()
    {
        var dict = new Dictionary<string, object>();
        var result = DictionaryHelper.GetInt(dict, "key", 99);
        result.Should().Be(99);
    }

    [Fact]
    public void GetInt_IntValue_ReturnsValue()
    {
        var dict = new Dictionary<string, object> { { "key", 123 } };
        var result = DictionaryHelper.GetInt(dict, "key");
        result.Should().Be(123);
    }

    [Fact]
    public void GetInt_LongValue_ReturnsConvertedValue()
    {
        var dict = new Dictionary<string, object> { { "key", 123L } };
        var result = DictionaryHelper.GetInt(dict, "key");
        result.Should().Be(123);
    }

    [Fact]
    public void GetInt_StringValue_ParsesSuccessfully()
    {
        var dict = new Dictionary<string, object> { { "key", "123" } };
        var result = DictionaryHelper.GetInt(dict, "key");
        result.Should().Be(123);
    }

    [Fact]
    public void GetInt_InvalidString_ReturnsDefault()
    {
        var dict = new Dictionary<string, object> { { "key", "invalid" } };
        var result = DictionaryHelper.GetInt(dict, "key", 99);
        result.Should().Be(99);
    }

    [Fact]
    public void GetInt_JsonElementNumber_ReturnsValue()
    {
        var json = JsonDocument.Parse("{\"key\": 123}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetInt(dict, "key");
        result.Should().Be(123);
    }

    [Fact]
    public void GetInt_JsonElementString_ParsesSuccessfully()
    {
        var json = JsonDocument.Parse("{\"key\": \"123\"}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetInt(dict, "key");
        result.Should().Be(123);
    }

    [Fact]
    public void GetInt_JsonElementNull_ReturnsDefault()
    {
        var json = JsonDocument.Parse("{\"key\": null}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetInt(dict, "key", 99);
        result.Should().Be(99);
    }

    #endregion

    #region GetDictionary Tests

    [Fact]
    public void GetDictionary_NullDictionary_ReturnsEmptyDictionary()
    {
        var result = DictionaryHelper.GetDictionary(null, "key");
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetDictionary_MissingKey_ReturnsEmptyDictionary()
    {
        var dict = new Dictionary<string, object>();
        var result = DictionaryHelper.GetDictionary(dict, "key");
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetDictionary_DictionaryValue_ReturnsValue()
    {
        var innerDict = new Dictionary<string, object> { { "inner", "value" } };
        var dict = new Dictionary<string, object> { { "key", innerDict } };
        var result = DictionaryHelper.GetDictionary(dict, "key");
        result.Should().ContainKey("inner");
        result["inner"].Should().Be("value");
    }

    [Fact]
    public void GetDictionary_JsonElementObject_ConvertsToDictionary()
    {
        var json = JsonDocument.Parse("{\"key\": {\"inner\": \"value\"}}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetDictionary(dict, "key");
        result.Should().ContainKey("inner");
    }

    [Fact]
    public void GetDictionary_NonDictionaryValue_ReturnsEmptyDictionary()
    {
        var dict = new Dictionary<string, object> { { "key", "string value" } };
        var result = DictionaryHelper.GetDictionary(dict, "key");
        result.Should().BeEmpty();
    }

    #endregion

    #region GetList Tests

    [Fact]
    public void GetList_NullDictionary_ReturnsNull()
    {
        var result = DictionaryHelper.GetList(null, "key");
        result.Should().BeNull();
    }

    [Fact]
    public void GetList_MissingKey_ReturnsNull()
    {
        var dict = new Dictionary<string, object>();
        var result = DictionaryHelper.GetList(dict, "key");
        result.Should().BeNull();
    }

    [Fact]
    public void GetList_NullValue_ReturnsNull()
    {
        var dict = new Dictionary<string, object> { { "key", null! } };
        var result = DictionaryHelper.GetList(dict, "key");
        result.Should().BeNull();
    }

    [Fact]
    public void GetList_ListValue_ReturnsList()
    {
        var list = new List<object> { "a", "b", "c" };
        var dict = new Dictionary<string, object> { { "key", list } };
        var result = DictionaryHelper.GetList(dict, "key");
        result.Should().NotBeNull();
        result!.Count().Should().Be(3);
    }

    [Fact]
    public void GetList_JsonElementArray_ConvertsList()
    {
        var json = JsonDocument.Parse("{\"key\": [1, 2, 3]}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetList(dict, "key");
        result.Should().NotBeNull();
        result!.Count().Should().Be(3);
    }

    [Fact]
    public void GetList_JsonElementArrayOfObjects_ConvertsToDictionaries()
    {
        var json = JsonDocument.Parse("{\"key\": [{\"a\": 1}, {\"b\": 2}]}");
        var dict = new Dictionary<string, object> { { "key", json.RootElement.GetProperty("key") } };
        var result = DictionaryHelper.GetList(dict, "key");
        result.Should().NotBeNull();
        var items = result!.ToList();
        items.Should().HaveCount(2);
        items[0].Should().BeOfType<Dictionary<string, object>>();
    }

    [Fact]
    public void GetList_NonListValue_ReturnsNull()
    {
        var dict = new Dictionary<string, object> { { "key", "string value" } };
        var result = DictionaryHelper.GetList(dict, "key");
        result.Should().BeNull();
    }

    #endregion
}
