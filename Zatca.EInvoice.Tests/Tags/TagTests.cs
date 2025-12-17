using System.Text;
using Zatca.EInvoice.Tags;

namespace Zatca.EInvoice.Tests.Tags;

/// <summary>
/// Theory-based tests for all 9 tag classes used in QR code generation.
/// Tests TLV (Tag-Length-Value) encoding format for each tag type.
/// </summary>
public class TagTests
{
    /// <summary>
    /// Test data for all 9 tag types.
    /// Parameters: Tag Number, Tag Value, Expected Tag Type Name
    /// </summary>
    public static TheoryData<byte, string, string> TagTestData => new()
    {
        { 1, "Maximum Speed Tech Supply", "SellerTag" },
        { 2, "399999999900003", "TaxNumberTag" },
        { 3, "2024-09-07T17:41:08Z", "InvoiceDateTag" },
        { 4, "4.60", "InvoiceTotalTag" },
        { 5, "0.60", "TaxAmountTag" },
        { 6, "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==", "InvoiceHashTag" },
        { 7, "MEUCIQDu8CIDdlUsls8+5V4SugNPP+wzAxNIVZ/0M3h/E7LUgwIgMy4w4dA0nH4kzZ9LdJ0C5k1N3QqrMuNz8G2UbE4/qXE=", "DigitalSignatureTag" },
        { 8, "MFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEjHSKTdSDJ7fZGP7WrRQ9K5cTi+8QYoIhKcCrJhHHBXQpJKbwHLRQCcEaG7pAFr4xkPcVH2QpZxXlKXFxHKkUXg==", "PublicKeyTag" },
        { 9, "MEQCIBVx/MYt2PdEHLpOtGhJKjkDmvJdmF3V4Hkv8TnGc5y6AiBH7S0V7bMKVQXZGJpVp8W5VvQpWNP3hUcT3EQMJ2kKFA==", "CertificateSignatureTag" }
    };

    /// <summary>
    /// Test that each tag type can be created and has the correct tag number.
    /// </summary>
    [Theory]
    [MemberData(nameof(TagTestData))]
    public void TestGetTag(byte tagNumber, string value, string tagType)
    {
        // Arrange & Act
        var tag = CreateTag(tagNumber, value);

        // Assert
        tag.Should().NotBeNull();
        tag.TagNumber.Should().Be(tagNumber);
    }

    /// <summary>
    /// Test that each tag stores the correct value.
    /// </summary>
    [Theory]
    [MemberData(nameof(TagTestData))]
    public void TestGetValue(byte tagNumber, string value, string tagType)
    {
        // Arrange & Act
        var tag = CreateTag(tagNumber, value);

        // Assert
        // Tags 8 and 9 use byte[] values, others use string
        if (tagNumber == 8 || tagNumber == 9)
        {
            tag.Value.Should().BeEquivalentTo(Convert.FromBase64String(value));
        }
        else
        {
            tag.Value.Should().Be(value);
        }
    }

    /// <summary>
    /// Test that the tag length is calculated correctly.
    /// Length should be the byte length of the UTF-8 encoded value.
    /// </summary>
    [Theory]
    [MemberData(nameof(TagTestData))]
    public void TestGetLength(byte tagNumber, string value, string tagType)
    {
        // Arrange
        var tag = CreateTag(tagNumber, value);
        // Tags 8 and 9 use byte[] values (decoded from base64), others use string
        var expectedLength = (tagNumber == 8 || tagNumber == 9)
            ? Convert.FromBase64String(value).Length
            : Encoding.UTF8.GetByteCount(value);

        // Act
        var tlvBytes = tag.ToBytes();
        var actualLength = tlvBytes[1]; // Second byte is the length

        // Assert
        actualLength.Should().Be((byte)expectedLength);
    }

    /// <summary>
    /// Test that TLV encoding format is correct: [Tag:1byte][Length:1byte][Value:N bytes]
    /// </summary>
    [Theory]
    [MemberData(nameof(TagTestData))]
    public void TestEncode(byte tagNumber, string value, string tagType)
    {
        // Arrange
        var tag = CreateTag(tagNumber, value);
        // Tags 8 and 9 use byte[] values (decoded from base64), others use string
        var valueBytes = (tagNumber == 8 || tagNumber == 9)
            ? Convert.FromBase64String(value)
            : Encoding.UTF8.GetBytes(value);
        var expectedLength = valueBytes.Length;

        // Act
        var tlvBytes = tag.ToBytes();

        // Assert
        tlvBytes.Should().NotBeNull();
        tlvBytes.Length.Should().Be(2 + expectedLength); // Tag + Length + Value
        tlvBytes[0].Should().Be(tagNumber); // First byte is tag number
        tlvBytes[1].Should().Be((byte)expectedLength); // Second byte is length

        // Verify the value bytes
        var actualValueBytes = new byte[expectedLength];
        Array.Copy(tlvBytes, 2, actualValueBytes, 0, expectedLength);
        actualValueBytes.Should().Equal(valueBytes);
    }

    /// <summary>
    /// Test SellerTag (Tag 1) specifically.
    /// </summary>
    [Fact]
    public void TestSellerTag()
    {
        // Arrange
        var sellerName = "Maximum Speed Tech Supply";
        var tag = new SellerTag(sellerName);

        // Act & Assert
        tag.TagNumber.Should().Be(1);
        tag.Value.Should().Be(sellerName);

        var bytes = tag.ToBytes();
        bytes[0].Should().Be(1);
        bytes[1].Should().Be((byte)Encoding.UTF8.GetByteCount(sellerName));
    }

    /// <summary>
    /// Test TaxNumberTag (Tag 2) specifically.
    /// </summary>
    [Fact]
    public void TestTaxNumberTag()
    {
        // Arrange
        var taxNumber = "399999999900003";
        var tag = new TaxNumberTag(taxNumber);

        // Act & Assert
        tag.TagNumber.Should().Be(2);
        tag.Value.Should().Be(taxNumber);

        var bytes = tag.ToBytes();
        bytes[0].Should().Be(2);
    }

    /// <summary>
    /// Test InvoiceDateTag (Tag 3) specifically.
    /// </summary>
    [Fact]
    public void TestInvoiceDateTag()
    {
        // Arrange
        var dateTime = "2024-09-07T17:41:08Z";
        var tag = new InvoiceDateTag(dateTime);

        // Act & Assert
        tag.TagNumber.Should().Be(3);
        tag.Value.Should().Be(dateTime);

        var bytes = tag.ToBytes();
        bytes[0].Should().Be(3);
    }

    /// <summary>
    /// Test InvoiceTotalTag (Tag 4) specifically.
    /// </summary>
    [Fact]
    public void TestInvoiceTotalTag()
    {
        // Arrange
        var totalAmount = "4.60";
        var tag = new InvoiceTotalTag(totalAmount);

        // Act & Assert
        tag.TagNumber.Should().Be(4);
        tag.Value.Should().Be(totalAmount);

        var bytes = tag.ToBytes();
        bytes[0].Should().Be(4);
    }

    /// <summary>
    /// Test TaxAmountTag (Tag 5) specifically.
    /// </summary>
    [Fact]
    public void TestTaxAmountTag()
    {
        // Arrange
        var taxAmount = "0.60";
        var tag = new TaxAmountTag(taxAmount);

        // Act & Assert
        tag.TagNumber.Should().Be(5);
        tag.Value.Should().Be(taxAmount);

        var bytes = tag.ToBytes();
        bytes[0].Should().Be(5);
    }

    /// <summary>
    /// Test InvoiceHashTag (Tag 6) specifically.
    /// </summary>
    [Fact]
    public void TestInvoiceHashTag()
    {
        // Arrange
        var hash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
        var tag = new InvoiceHashTag(hash);

        // Act & Assert
        tag.TagNumber.Should().Be(6);
        tag.Value.Should().Be(hash);

        var bytes = tag.ToBytes();
        bytes[0].Should().Be(6);
    }

    /// <summary>
    /// Test DigitalSignatureTag (Tag 7) specifically.
    /// </summary>
    [Fact]
    public void TestDigitalSignatureTag()
    {
        // Arrange
        var signature = "MEUCIQDu8CIDdlUsls8+5V4SugNPP+wzAxNIVZ/0M3h/E7LUgwIgMy4w4dA0nH4kzZ9LdJ0C5k1N3QqrMuNz8G2UbE4/qXE=";
        var tag = new DigitalSignatureTag(signature);

        // Act & Assert
        tag.TagNumber.Should().Be(7);
        tag.Value.Should().Be(signature);

        var bytes = tag.ToBytes();
        bytes[0].Should().Be(7);
    }

    /// <summary>
    /// Test PublicKeyTag (Tag 8) specifically.
    /// </summary>
    [Fact]
    public void TestPublicKeyTag()
    {
        // Arrange
        var publicKeyBase64 = "MFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEjHSKTdSDJ7fZGP7WrRQ9K5cTi+8QYoIhKcCrJhHHBXQpJKbwHLRQCcEaG7pAFr4xkPcVH2QpZxXlKXFxHKkUXg==";
        var publicKey = Convert.FromBase64String(publicKeyBase64);
        var tag = new PublicKeyTag(publicKey);

        // Act & Assert
        tag.TagNumber.Should().Be(8);
        tag.Value.Should().BeEquivalentTo(publicKey);

        var bytes = tag.ToBytes();
        bytes[0].Should().Be(8);
    }

    /// <summary>
    /// Test CertificateSignatureTag (Tag 9) specifically.
    /// </summary>
    [Fact]
    public void TestCertificateSignatureTag()
    {
        // Arrange
        var certSignatureBase64 = "MEQCIBVx/MYt2PdEHLpOtGhJKjkDmvJdmF3V4Hkv8TnGc5y6AiBH7S0V7bMKVQXZGJpVp8W5VvQpWNP3hUcT3EQMJ2kKFA==";
        var certSignature = Convert.FromBase64String(certSignatureBase64);
        var tag = new CertificateSignatureTag(certSignature);

        // Act & Assert
        tag.TagNumber.Should().Be(9);
        tag.Value.Should().BeEquivalentTo(certSignature);

        var bytes = tag.ToBytes();
        bytes[0].Should().Be(9);
    }

    /// <summary>
    /// Test that empty string values are handled correctly.
    /// </summary>
    [Fact]
    public void TestEmptyStringValue()
    {
        // Arrange
        var tag = new SellerTag("");

        // Act
        var bytes = tag.ToBytes();

        // Assert
        tag.Value.Should().Be("");
        bytes.Length.Should().Be(2); // Tag + Length (0) + no value bytes
        bytes[0].Should().Be(1); // Tag number
        bytes[1].Should().Be(0); // Length is 0
    }

    /// <summary>
    /// Test that tags with special characters are encoded correctly.
    /// </summary>
    [Fact]
    public void TestSpecialCharactersEncoding()
    {
        // Arrange
        var specialText = "Test äöü 中文";
        var tag = new SellerTag(specialText);

        // Act
        var bytes = tag.ToBytes();
        var expectedLength = Encoding.UTF8.GetByteCount(specialText);

        // Assert
        bytes[1].Should().Be((byte)expectedLength);
        bytes.Length.Should().Be(2 + expectedLength);

        // Verify we can decode it back
        var valueBytes = new byte[expectedLength];
        Array.Copy(bytes, 2, valueBytes, 0, expectedLength);
        var decodedValue = Encoding.UTF8.GetString(valueBytes);
        decodedValue.Should().Be(specialText);
    }

    /// <summary>
    /// Helper method to create tags based on tag number.
    /// </summary>
    private Tag CreateTag(byte tagNumber, string value)
    {
        return tagNumber switch
        {
            1 => new SellerTag(value),
            2 => new TaxNumberTag(value),
            3 => new InvoiceDateTag(value),
            4 => new InvoiceTotalTag(value),
            5 => new TaxAmountTag(value),
            6 => new InvoiceHashTag(value),
            7 => new DigitalSignatureTag(value),
            8 => new PublicKeyTag(Convert.FromBase64String(value)),
            9 => new CertificateSignatureTag(Convert.FromBase64String(value)),
            _ => throw new ArgumentException($"Unknown tag number: {tagNumber}")
        };
    }
}
