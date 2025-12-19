using System.Security.Cryptography;
using Zatca.EInvoice.Exceptions;

namespace Zatca.EInvoice.Tests.Exceptions;

/// <summary>
/// Tests for all exception classes in the Zatca.EInvoice.Exceptions namespace.
/// </summary>
public class ExceptionTests
{
    #region ZatcaException Tests

    [Fact]
    public void ZatcaException_DefaultConstructor_SetsDefaultMessage()
    {
        // Arrange & Act
        var exception = new ZatcaException();

        // Assert
        exception.Message.Should().Be("An error occurred");
        exception.Context.Should().NotBeNull();
        exception.Context.Should().BeEmpty();
    }

    [Fact]
    public void ZatcaException_MessageConstructor_SetsMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new ZatcaException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Context.Should().NotBeNull();
        exception.Context.Should().BeEmpty();
    }

    [Fact]
    public void ZatcaException_MessageAndContextConstructor_SetsMessageAndContext()
    {
        // Arrange
        var message = "Test error message";
        var context = new Dictionary<string, object> { { "key1", "value1" }, { "key2", 42 } };

        // Act
        var exception = new ZatcaException(message, context);

        // Assert
        exception.Message.Should().Be(message);
        exception.Context.Should().ContainKey("key1");
        exception.Context["key1"].Should().Be("value1");
        exception.Context["key2"].Should().Be(42);
    }

    [Fact]
    public void ZatcaException_NullContext_InitializesEmptyContext()
    {
        // Arrange & Act
        var exception = new ZatcaException("Test", (Dictionary<string, object>)null!);

        // Assert
        exception.Context.Should().NotBeNull();
        exception.Context.Should().BeEmpty();
    }

    [Fact]
    public void ZatcaException_MessageAndInnerExceptionConstructor_SetsInnerException()
    {
        // Arrange
        var message = "Outer exception";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new ZatcaException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
        exception.Context.Should().NotBeNull();
    }

    [Fact]
    public void ZatcaException_FullConstructor_SetsAllProperties()
    {
        // Arrange
        var message = "Full exception";
        var context = new Dictionary<string, object> { { "operation", "test" } };
        var innerException = new Exception("Inner");

        // Act
        var exception = new ZatcaException(message, context, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.Context.Should().ContainKey("operation");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ZatcaException_WithContext_MergesContext()
    {
        // Arrange
        var initialContext = new Dictionary<string, object> { { "key1", "value1" } };
        var exception = new ZatcaException("Test", initialContext);
        var additionalContext = new Dictionary<string, object> { { "key2", "value2" }, { "key3", 100 } };

        // Act
        var result = exception.WithContext(additionalContext);

        // Assert
        result.Should().BeSameAs(exception);
        exception.Context.Should().HaveCount(3);
        exception.Context["key1"].Should().Be("value1");
        exception.Context["key2"].Should().Be("value2");
        exception.Context["key3"].Should().Be(100);
    }

    [Fact]
    public void ZatcaException_WithContext_OverwritesExistingKeys()
    {
        // Arrange
        var initialContext = new Dictionary<string, object> { { "key1", "original" } };
        var exception = new ZatcaException("Test", initialContext);
        var additionalContext = new Dictionary<string, object> { { "key1", "updated" } };

        // Act
        exception.WithContext(additionalContext);

        // Assert
        exception.Context["key1"].Should().Be("updated");
    }

    [Fact]
    public void ZatcaException_WithContext_NullContext_DoesNotThrow()
    {
        // Arrange
        var exception = new ZatcaException("Test");

        // Act
        var result = exception.WithContext(null!);

        // Assert
        result.Should().BeSameAs(exception);
        exception.Context.Should().BeEmpty();
    }

    #endregion

    #region ZatcaValidationException Tests

    [Fact]
    public void ZatcaValidationException_DefaultConstructor_SetsDefaultMessage()
    {
        // Arrange & Act
        var exception = new ZatcaValidationException();

        // Assert
        exception.Message.Should().Be("Validation failed.");
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ZatcaValidationException_MessageConstructor_SetsMessage()
    {
        // Arrange
        var message = "Custom validation error";

        // Act
        var exception = new ZatcaValidationException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ZatcaValidationException_MessageAndErrorsConstructor_SetsMessageAndErrors()
    {
        // Arrange
        var message = "Validation failed";
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var exception = new ZatcaValidationException(message, errors);

        // Assert
        exception.Message.Should().Be(message);
        exception.Errors.Should().HaveCount(2);
        exception.Errors.Should().Contain("Error 1");
        exception.Errors.Should().Contain("Error 2");
    }

    [Fact]
    public void ZatcaValidationException_NullErrors_InitializesEmptyList()
    {
        // Arrange & Act
        var exception = new ZatcaValidationException("Test", (List<string>)null!);

        // Assert
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ZatcaValidationException_ErrorsListConstructor_BuildsMessageFromErrors()
    {
        // Arrange
        var errors = new List<string> { "Field is required" };

        // Act
        var exception = new ZatcaValidationException(errors);

        // Assert
        exception.Message.Should().Contain("Validation failed");
        exception.Message.Should().Contain("Field is required");
        exception.Errors.Should().Contain("Field is required");
    }

    [Fact]
    public void ZatcaValidationException_ErrorsListConstructor_MultipleErrors_BuildsMessageWithCount()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var exception = new ZatcaValidationException(errors);

        // Assert
        exception.Message.Should().Contain("3 errors");
    }

    [Fact]
    public void ZatcaValidationException_ErrorsListConstructor_MoreThanThreeErrors_TruncatesMessage()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3", "Error 4" };

        // Act
        var exception = new ZatcaValidationException(errors);

        // Assert
        exception.Message.Should().Contain("4 errors");
        exception.Message.Should().EndWith("...");
    }

    [Fact]
    public void ZatcaValidationException_ErrorsListConstructor_EmptyList_SetsDefaultMessage()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var exception = new ZatcaValidationException(errors);

        // Assert
        exception.Message.Should().Be("Validation failed.");
    }

    [Fact]
    public void ZatcaValidationException_ErrorsListConstructor_NullList_SetsDefaultMessage()
    {
        // Arrange & Act
        var exception = new ZatcaValidationException((List<string>)null!);

        // Assert
        exception.Message.Should().Be("Validation failed.");
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ZatcaValidationException_MessageAndSingleErrorConstructor_SetsSingleError()
    {
        // Arrange
        var message = "Field validation failed";
        var error = "Name is required";

        // Act
        var exception = new ZatcaValidationException(message, error);

        // Assert
        exception.Message.Should().Be(message);
        exception.Errors.Should().HaveCount(1);
        exception.Errors[0].Should().Be(error);
    }

    [Fact]
    public void ZatcaValidationException_AddError_AddsToErrorsList()
    {
        // Arrange
        var exception = new ZatcaValidationException("Validation failed");

        // Act
        var result = exception.AddError("New error");

        // Assert
        result.Should().BeSameAs(exception);
        exception.Errors.Should().Contain("New error");
    }

    [Fact]
    public void ZatcaValidationException_AddError_NullOrEmpty_DoesNotAdd()
    {
        // Arrange
        var exception = new ZatcaValidationException("Validation failed");

        // Act
        exception.AddError(null!);
        exception.AddError("");
        exception.AddError("   ");

        // Assert
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ZatcaValidationException_AddError_ChainedCalls()
    {
        // Arrange
        var exception = new ZatcaValidationException();

        // Act
        exception
            .AddError("Error 1")
            .AddError("Error 2")
            .AddError("Error 3");

        // Assert
        exception.Errors.Should().HaveCount(3);
    }

    #endregion

    #region CertificateBuilderException Tests

    [Fact]
    public void CertificateBuilderException_DefaultConstructor_SetsDefaultMessage()
    {
        // Arrange & Act
        var exception = new CertificateBuilderException();

        // Assert
        exception.Message.Should().Be("Certificate builder operation failed.");
    }

    [Fact]
    public void CertificateBuilderException_MessageConstructor_SetsMessage()
    {
        // Arrange
        var message = "Failed to generate CSR";

        // Act
        var exception = new CertificateBuilderException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void CertificateBuilderException_MessageAndContextConstructor_SetsMessageAndContext()
    {
        // Arrange
        var message = "Certificate generation failed";
        var context = new Dictionary<string, object> { { "operation", "CSR" } };

        // Act
        var exception = new CertificateBuilderException(message, context);

        // Assert
        exception.Message.Should().Be(message);
        exception.Context.Should().ContainKey("operation");
    }

    [Fact]
    public void CertificateBuilderException_MessageAndInnerExceptionConstructor_SetsInnerException()
    {
        // Arrange
        var message = "Certificate failed";
        var innerException = new CryptographicException("Invalid key");

        // Act
        var exception = new CertificateBuilderException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void CertificateBuilderException_FullConstructor_SetsAllProperties()
    {
        // Arrange
        var message = "Full certificate error";
        var context = new Dictionary<string, object> { { "step", "signing" } };
        var innerException = new Exception("Crypto error");

        // Act
        var exception = new CertificateBuilderException(message, context, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.Context["step"].Should().Be("signing");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void CertificateBuilderException_InheritsFromZatcaException()
    {
        // Arrange & Act
        var exception = new CertificateBuilderException();

        // Assert
        exception.Should().BeAssignableTo<ZatcaException>();
    }

    #endregion

    #region ZatcaStorageException Tests

    [Fact]
    public void ZatcaStorageException_DefaultConstructor_SetsDefaultMessage()
    {
        // Arrange & Act
        var exception = new ZatcaStorageException();

        // Assert
        exception.Message.Should().Be("Storage operation failed.");
    }

    [Fact]
    public void ZatcaStorageException_MessageConstructor_SetsMessage()
    {
        // Arrange
        var message = "Failed to write file";

        // Act
        var exception = new ZatcaStorageException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void ZatcaStorageException_MessageAndContextConstructor_SetsMessageAndContext()
    {
        // Arrange
        var message = "Storage error";
        var context = new Dictionary<string, object> { { "path", "/tmp/test.xml" } };

        // Act
        var exception = new ZatcaStorageException(message, context);

        // Assert
        exception.Message.Should().Be(message);
        exception.Context["path"].Should().Be("/tmp/test.xml");
    }

    [Fact]
    public void ZatcaStorageException_MessageAndInnerExceptionConstructor_SetsInnerException()
    {
        // Arrange
        var message = "IO Error";
        var innerException = new IOException("Disk full");

        // Act
        var exception = new ZatcaStorageException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ZatcaStorageException_FullConstructor_SetsAllProperties()
    {
        // Arrange
        var message = "Full storage error";
        var context = new Dictionary<string, object> { { "operation", "write" } };
        var innerException = new UnauthorizedAccessException("Access denied");

        // Act
        var exception = new ZatcaStorageException(message, context, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.Context["operation"].Should().Be("write");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ZatcaStorageException_InheritsFromZatcaException()
    {
        // Arrange & Act
        var exception = new ZatcaStorageException();

        // Assert
        exception.Should().BeAssignableTo<ZatcaException>();
    }

    #endregion

    #region ZatcaApiException Tests

    [Fact]
    public void ZatcaApiException_DefaultConstructor_SetsDefaultMessage()
    {
        // Arrange & Act
        var exception = new ZatcaApiException();

        // Assert
        exception.Message.Should().Be("ZATCA API request failed.");
        exception.StatusCode.Should().BeNull();
        exception.Response.Should().BeEmpty();
    }

    [Fact]
    public void ZatcaApiException_MessageConstructor_SetsMessage()
    {
        // Arrange
        var message = "Failed to submit invoice";

        // Act
        var exception = new ZatcaApiException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().BeNull();
    }

    [Fact]
    public void ZatcaApiException_MessageAndContextConstructor_SetsMessageAndContext()
    {
        // Arrange
        var message = "API Error";
        var context = new Dictionary<string, object> { { "endpoint", "/invoices" } };

        // Act
        var exception = new ZatcaApiException(message, context);

        // Assert
        exception.Message.Should().Be(message);
        exception.Context["endpoint"].Should().Be("/invoices");
    }

    [Fact]
    public void ZatcaApiException_MessageContextStatusCodeConstructor_SetsStatusCode()
    {
        // Arrange
        var message = "Unauthorized";
        var context = new Dictionary<string, object>();
        var statusCode = 401;

        // Act
        var exception = new ZatcaApiException(message, context, statusCode);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(401);
    }

    [Fact]
    public void ZatcaApiException_MessageContextStatusCodeInnerExceptionConstructor_SetsAll()
    {
        // Arrange
        var message = "Server Error";
        var context = new Dictionary<string, object> { { "retry", 3 } };
        var statusCode = 500;
        var innerException = new HttpRequestException("Network error");

        // Act
        var exception = new ZatcaApiException(message, context, statusCode, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(500);
        exception.Context["retry"].Should().Be(3);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ZatcaApiException_MessageStatusCodeResponseConstructor_SetsResponse()
    {
        // Arrange
        var message = "Bad Request";
        var statusCode = 400;
        var response = "{\"error\": \"Invalid invoice\"}";

        // Act
        var exception = new ZatcaApiException(message, statusCode, response);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(400);
        exception.Response.Should().Be(response);
    }

    [Fact]
    public void ZatcaApiException_MessageStatusCodeResponseInnerExceptionConstructor_SetsAll()
    {
        // Arrange
        var message = "Gateway Timeout";
        var statusCode = 504;
        var response = "Timeout";
        var innerException = new TaskCanceledException("Request cancelled");

        // Act
        var exception = new ZatcaApiException(message, statusCode, response, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(504);
        exception.Response.Should().Be(response);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ZatcaApiException_NullMessage_UsesDefaultMessage()
    {
        // Arrange & Act
        var exception = new ZatcaApiException(null!, new Dictionary<string, object>());

        // Assert
        exception.Message.Should().Be("ZATCA API request failed.");
    }

    [Fact]
    public void ZatcaApiException_InheritsFromZatcaException()
    {
        // Arrange & Act
        var exception = new ZatcaApiException();

        // Assert
        exception.Should().BeAssignableTo<ZatcaException>();
    }

    #endregion
}
