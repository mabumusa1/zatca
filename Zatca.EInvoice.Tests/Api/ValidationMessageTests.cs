using Zatca.EInvoice.Api;

namespace Zatca.EInvoice.Tests.Api;

public class ValidationMessageTests
{
    #region Constructor Tests

    [Fact]
    public void ValidationMessage_DefaultConstructor_InitializesEmptyStrings()
    {
        var message = new ValidationMessage();

        message.Type.Should().Be(string.Empty);
        message.Code.Should().Be(string.Empty);
        message.Category.Should().Be(string.Empty);
        message.Message.Should().Be(string.Empty);
        message.Status.Should().Be(string.Empty);
    }

    [Fact]
    public void ValidationMessage_ParameterizedConstructor_SetsAllProperties()
    {
        var message = new ValidationMessage("ERROR", "E001", "VALIDATION", "Field is required", "FAILED");

        message.Type.Should().Be("ERROR");
        message.Code.Should().Be("E001");
        message.Category.Should().Be("VALIDATION");
        message.Message.Should().Be("Field is required");
        message.Status.Should().Be("FAILED");
    }

    [Fact]
    public void ValidationMessage_ParameterizedConstructor_WithEmptyStrings()
    {
        var message = new ValidationMessage("", "", "", "", "");

        message.Type.Should().Be("");
        message.Code.Should().Be("");
        message.Category.Should().Be("");
        message.Message.Should().Be("");
        message.Status.Should().Be("");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void ValidationMessage_Type_CanBeSet()
    {
        var message = new ValidationMessage();
        message.Type = "WARNING";
        message.Type.Should().Be("WARNING");
    }

    [Fact]
    public void ValidationMessage_Code_CanBeSet()
    {
        var message = new ValidationMessage();
        message.Code = "W001";
        message.Code.Should().Be("W001");
    }

    [Fact]
    public void ValidationMessage_Category_CanBeSet()
    {
        var message = new ValidationMessage();
        message.Category = "BUSINESS_RULE";
        message.Category.Should().Be("BUSINESS_RULE");
    }

    [Fact]
    public void ValidationMessage_Message_CanBeSet()
    {
        var message = new ValidationMessage();
        message.Message = "Custom message";
        message.Message.Should().Be("Custom message");
    }

    [Fact]
    public void ValidationMessage_Status_CanBeSet()
    {
        var message = new ValidationMessage();
        message.Status = "PASSED";
        message.Status.Should().Be("PASSED");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ValidationMessage_ToString_FormatsCorrectly()
    {
        var message = new ValidationMessage("ERROR", "E001", "VALIDATION", "Field is required", "FAILED");
        var result = message.ToString();
        result.Should().Be("[ERROR] E001: Field is required");
    }

    [Fact]
    public void ValidationMessage_ToString_WithEmptyValues()
    {
        var message = new ValidationMessage();
        var result = message.ToString();
        result.Should().Be("[] : ");
    }

    [Fact]
    public void ValidationMessage_ToString_WithWarningType()
    {
        var message = new ValidationMessage("WARNING", "W001", "INFO", "Consider updating", "PASSED");
        var result = message.ToString();
        result.Should().Be("[WARNING] W001: Consider updating");
    }

    #endregion
}
