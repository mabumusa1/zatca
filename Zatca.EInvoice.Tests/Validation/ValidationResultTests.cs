namespace Zatca.EInvoice.Tests.Validation;

public class ValidationResultTests
{
    [Fact]
    public void ValidationResult_DefaultConstructor_IsValidTrue()
    {
        var result = new ValidationResult();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_AddError_SetsIsValidToFalse()
    {
        var result = new ValidationResult();
        result.AddError("Test error");
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidationResult_AddError_AddsToErrorsList()
    {
        var result = new ValidationResult();
        result.AddError("Error 1");
        result.AddError("Error 2");
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Error 1");
        result.Errors.Should().Contain("Error 2");
    }

    [Fact]
    public void ValidationResult_Success_ReturnsValidResult()
    {
        var result = ValidationResult.Success();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_FailureSingleError_ReturnsInvalidResult()
    {
        var result = ValidationResult.Failure("Single error");
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be("Single error");
    }

    [Fact]
    public void ValidationResult_FailureMultipleErrors_ReturnsInvalidResult()
    {
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };
        var result = ValidationResult.Failure(errors);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void ValidationResult_FailureEmptyErrors_ReturnsValidResult()
    {
        var errors = new List<string>();
        var result = ValidationResult.Failure(errors);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_IsValid_CanBeSetManually()
    {
        var result = new ValidationResult();
        result.IsValid = false;
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidationResult_Errors_CanBeSetManually()
    {
        var result = new ValidationResult();
        result.Errors = new List<string> { "Custom error" };
        result.Errors.Should().HaveCount(1);
    }
}
