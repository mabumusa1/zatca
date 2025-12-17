using System.Text;
using Xunit;
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Tags;

namespace Zatca.EInvoice.Tests.Helpers;

/// <summary>
/// Tests for QrCodeGenerator class.
/// </summary>
public class QrCodeGeneratorTests
{
    [Fact]
    public void TestEncodeTlv_ProducesCorrectBytes()
    {
        // Arrange
        var sellerTag = new SellerTag("Test Company");
        var taxNumberTag = new TaxNumberTag("300000000000003");
        var invoiceDateTag = new InvoiceDateTag("2023-12-17T14:30:00Z");

        var generator = QrCodeGenerator.CreateFromTags(sellerTag, taxNumberTag, invoiceDateTag);

        // Act
        var tlvBytes = generator.EncodeTlv();

        // Assert
        Assert.NotNull(tlvBytes);
        Assert.NotEmpty(tlvBytes);

        // Verify TLV structure: Tag(1) + Length(1) + Value
        // First tag: SellerTag (tag number 1)
        Assert.Equal(1, tlvBytes[0]); // Tag number
        Assert.Equal(12, tlvBytes[1]); // Length of "Test Company"
        var sellerValue = Encoding.UTF8.GetString(tlvBytes, 2, 12);
        Assert.Equal("Test Company", sellerValue);

        // Second tag: TaxNumberTag (tag number 2) starts at position 14 (2 + 12)
        Assert.Equal(2, tlvBytes[14]); // Tag number
        Assert.Equal(15, tlvBytes[15]); // Length of "300000000000003"
        var taxValue = Encoding.UTF8.GetString(tlvBytes, 16, 15);
        Assert.Equal("300000000000003", taxValue);

        // Third tag: InvoiceDateTag (tag number 3) starts at position 31 (14 + 2 + 15)
        Assert.Equal(3, tlvBytes[31]); // Tag number
        Assert.Equal(20, tlvBytes[32]); // Length of "2023-12-17T14:30:00Z"
        var dateValue = Encoding.UTF8.GetString(tlvBytes, 33, 20);
        Assert.Equal("2023-12-17T14:30:00Z", dateValue);
    }

    [Fact]
    public void TestEncodeBase64_ReturnsValidBase64String()
    {
        // Arrange
        var sellerTag = new SellerTag("Saudi Company");
        var taxNumberTag = new TaxNumberTag("310122393500003");

        var generator = QrCodeGenerator.CreateFromTags(sellerTag, taxNumberTag);

        // Act
        var base64String = generator.EncodeBase64();

        // Assert
        Assert.NotNull(base64String);
        Assert.NotEmpty(base64String);

        // Verify it's valid base64
        var decodedBytes = Convert.FromBase64String(base64String);
        Assert.NotEmpty(decodedBytes);

        // Verify decoded bytes match TLV encoding
        var tlvBytes = generator.EncodeTlv();
        Assert.Equal(tlvBytes, decodedBytes);
    }

    [Fact]
    public void TestEmptyTagsThrowsException_WhenNoTags()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            QrCodeGenerator.CreateFromTags());

        Assert.Contains("No valid Tag instances found", exception.Message);
    }

    [Fact]
    public void TestEmptyTagsThrowsException_WhenAllNullTags()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            QrCodeGenerator.CreateFromTags(null, null, null));

        Assert.Contains("No valid Tag instances found", exception.Message);
    }

    [Fact]
    public void TestCreateFromTagsArray_WithMultipleTags()
    {
        // Arrange
        var tags = new Tag[]
        {
            new SellerTag("Company Name"),
            new TaxNumberTag("123456789"),
            new InvoiceDateTag("2023-12-17T10:00:00Z"),
            new InvoiceTotalTag("1150.00"),
            new TaxAmountTag("150.00")
        };

        // Act
        var generator = QrCodeGenerator.CreateFromTags(tags);
        var tlvBytes = generator.EncodeTlv();

        // Assert
        Assert.NotNull(tlvBytes);
        // Each tag has: 1 byte tag + 1 byte length + N bytes value
        // Tag 1: 1 + 1 + 12 = 14
        // Tag 2: 1 + 1 + 9 = 11
        // Tag 3: 1 + 1 + 20 = 22
        // Tag 4: 1 + 1 + 7 = 9
        // Tag 5: 1 + 1 + 6 = 8
        // Total: 64 bytes
        Assert.Equal(64, tlvBytes.Length);
    }

    [Fact]
    public void TestCreateFromTagsList_WithEnumerable()
    {
        // Arrange
        var tagsList = new List<Tag>
        {
            new SellerTag("My Company"),
            new TaxNumberTag("999888777")
        };

        // Act
        var generator = QrCodeGenerator.CreateFromTags(tagsList);
        var base64 = generator.EncodeBase64();

        // Assert
        Assert.NotNull(base64);
        Assert.NotEmpty(base64);
    }

    [Fact]
    public void TestEncodeTlv_WithHashTag()
    {
        // Arrange
        var hashValue = "SGVsbG9Xb3JsZA=="; // Sample base64 hash
        var hashTag = new InvoiceHashTag(hashValue);

        var generator = QrCodeGenerator.CreateFromTags(hashTag);

        // Act
        var tlvBytes = generator.EncodeTlv();

        // Assert
        Assert.Equal(6, tlvBytes[0]); // InvoiceHashTag has tag number 6
        Assert.Equal(hashValue.Length, tlvBytes[1]); // Length
        var value = Encoding.UTF8.GetString(tlvBytes, 2, tlvBytes[1]);
        Assert.Equal(hashValue, value);
    }

    [Fact]
    public void TestEncodeTlv_FiltersNullTags()
    {
        // Arrange
        var validTag = new SellerTag("Valid Company");

        // Act
        var generator = QrCodeGenerator.CreateFromTags(validTag, null, null);
        var tlvBytes = generator.EncodeTlv();

        // Assert
        // Should only encode the valid tag
        Assert.Equal(1, tlvBytes[0]); // Tag number 1 (SellerTag)
        Assert.Equal(13, tlvBytes[1]); // Length of "Valid Company"
        Assert.Equal(15, tlvBytes.Length); // Total: 1 + 1 + 13
    }

    [Fact]
    public void TestEncodeBase64_CanBeDecoded()
    {
        // Arrange
        var sellerName = "ZATCA Compliance Test";
        var taxNumber = "300075588700003";
        var invoiceDate = "2024-01-15T09:30:00Z";
        var total = "2300.00";
        var tax = "300.00";

        var generator = QrCodeGenerator.CreateFromTags(
            new SellerTag(sellerName),
            new TaxNumberTag(taxNumber),
            new InvoiceDateTag(invoiceDate),
            new InvoiceTotalTag(total),
            new TaxAmountTag(tax)
        );

        // Act
        var base64 = generator.EncodeBase64();
        var decodedBytes = Convert.FromBase64String(base64);

        // Assert
        // Verify we can extract the seller name from TLV
        Assert.Equal(1, decodedBytes[0]); // SellerTag number
        var sellerLength = decodedBytes[1];
        var extractedSeller = Encoding.UTF8.GetString(decodedBytes, 2, sellerLength);
        Assert.Equal(sellerName, extractedSeller);
    }
}
