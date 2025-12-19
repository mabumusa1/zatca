using Zatca.EInvoice.Api;

namespace Zatca.EInvoice.Tests.Api;

public class InvoiceSubmissionResultTests
{
    #region Default Constructor Tests

    [Fact]
    public void InvoiceSubmissionResult_DefaultConstructor_InitializesEmptyCollections()
    {
        var result = new InvoiceSubmissionResult();

        result.Status.Should().Be(string.Empty);
        result.ClearanceStatus.Should().BeNull();
        result.ReportingStatus.Should().BeNull();
        result.ClearedInvoice.Should().BeNull();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().NotBeNull();
        result.Warnings.Should().BeEmpty();
        result.InfoMessages.Should().NotBeNull();
        result.InfoMessages.Should().BeEmpty();
    }

    #endregion

    #region Parameterized Constructor Tests

    [Fact]
    public void InvoiceSubmissionResult_ParameterizedConstructor_SetsAllProperties()
    {
        var errors = new List<ValidationMessage> { new("ERROR", "E001", "CAT", "Error message", "FAILED") };
        var warnings = new List<ValidationMessage> { new("WARNING", "W001", "CAT", "Warning message", "PASSED") };
        var infoMessages = new List<ValidationMessage> { new("INFO", "I001", "CAT", "Info message", "OK") };

        var result = new InvoiceSubmissionResult(
            "SUCCESS",
            "CLEARED",
            null,
            "<Invoice/>",
            errors,
            warnings,
            infoMessages);

        result.Status.Should().Be("SUCCESS");
        result.ClearanceStatus.Should().Be("CLEARED");
        result.ReportingStatus.Should().BeNull();
        result.ClearedInvoice.Should().Be("<Invoice/>");
        result.Errors.Should().HaveCount(1);
        result.Warnings.Should().HaveCount(1);
        result.InfoMessages.Should().HaveCount(1);
    }

    [Fact]
    public void InvoiceSubmissionResult_ParameterizedConstructor_WithNullStatus_DefaultsToEmpty()
    {
        var result = new InvoiceSubmissionResult(null!, null, null, null, null, null, null);

        result.Status.Should().Be(string.Empty);
        result.Errors.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().NotBeNull();
        result.Warnings.Should().BeEmpty();
        result.InfoMessages.Should().NotBeNull();
        result.InfoMessages.Should().BeEmpty();
    }

    [Fact]
    public void InvoiceSubmissionResult_ParameterizedConstructor_WithReportingStatus()
    {
        var result = new InvoiceSubmissionResult(
            "SUCCESS",
            null,
            "REPORTED",
            null,
            null,
            null,
            null);

        result.ReportingStatus.Should().Be("REPORTED");
        result.ClearanceStatus.Should().BeNull();
    }

    #endregion

    #region IsSuccess Tests

    [Fact]
    public void InvoiceSubmissionResult_IsSuccess_TrueWhenNoErrors()
    {
        var result = new InvoiceSubmissionResult();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void InvoiceSubmissionResult_IsSuccess_FalseWhenHasErrors()
    {
        var errors = new List<ValidationMessage> { new("ERROR", "E001", "CAT", "Error", "FAILED") };
        var result = new InvoiceSubmissionResult("FAILED", null, null, null, errors, null, null);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void InvoiceSubmissionResult_IsSuccess_TrueWithOnlyWarnings()
    {
        var warnings = new List<ValidationMessage> { new("WARNING", "W001", "CAT", "Warning", "PASSED") };
        var result = new InvoiceSubmissionResult("SUCCESS", null, null, null, null, warnings, null);
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region HasWarnings Tests

    [Fact]
    public void InvoiceSubmissionResult_HasWarnings_FalseWhenNoWarnings()
    {
        var result = new InvoiceSubmissionResult();
        result.HasWarnings.Should().BeFalse();
    }

    [Fact]
    public void InvoiceSubmissionResult_HasWarnings_TrueWhenHasWarnings()
    {
        var warnings = new List<ValidationMessage> { new("WARNING", "W001", "CAT", "Warning", "PASSED") };
        var result = new InvoiceSubmissionResult("SUCCESS", null, null, null, null, warnings, null);
        result.HasWarnings.Should().BeTrue();
    }

    #endregion

    #region IsClearance Tests

    [Fact]
    public void InvoiceSubmissionResult_IsClearance_FalseWhenNoClearanceStatus()
    {
        var result = new InvoiceSubmissionResult();
        result.IsClearance.Should().BeFalse();
    }

    [Fact]
    public void InvoiceSubmissionResult_IsClearance_TrueWhenHasClearanceStatus()
    {
        var result = new InvoiceSubmissionResult("SUCCESS", "CLEARED", null, null, null, null, null);
        result.IsClearance.Should().BeTrue();
    }

    [Fact]
    public void InvoiceSubmissionResult_IsClearance_FalseWhenClearanceStatusIsEmpty()
    {
        var result = new InvoiceSubmissionResult("SUCCESS", "", null, null, null, null, null);
        result.IsClearance.Should().BeFalse();
    }

    #endregion

    #region IsReporting Tests

    [Fact]
    public void InvoiceSubmissionResult_IsReporting_FalseWhenNoReportingStatus()
    {
        var result = new InvoiceSubmissionResult();
        result.IsReporting.Should().BeFalse();
    }

    [Fact]
    public void InvoiceSubmissionResult_IsReporting_TrueWhenHasReportingStatus()
    {
        var result = new InvoiceSubmissionResult("SUCCESS", null, "REPORTED", null, null, null, null);
        result.IsReporting.Should().BeTrue();
    }

    [Fact]
    public void InvoiceSubmissionResult_IsReporting_FalseWhenReportingStatusIsEmpty()
    {
        var result = new InvoiceSubmissionResult("SUCCESS", null, "", null, null, null, null);
        result.IsReporting.Should().BeFalse();
    }

    #endregion
}
