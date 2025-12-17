using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Xunit;
using Zatca.EInvoice.Signing;

namespace Zatca.EInvoice.Tests.Helpers;

/// <summary>
/// Tests for InvoiceExtension XML manipulation class.
/// </summary>
public class InvoiceExtensionTests
{
    // Sample UBL invoice XML with namespaces
    private const string SampleInvoiceXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
         xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2""
         xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
    <ext:UBLExtensions>
        <ext:UBLExtension>
            <ext:ExtensionURI>urn:oasis:names:specification:ubl:dsig:enveloped:xades</ext:ExtensionURI>
            <ext:ExtensionContent>
                <!-- Signature content here -->
            </ext:ExtensionContent>
        </ext:UBLExtension>
    </ext:UBLExtensions>
    <cbc:ID>INV-12345</cbc:ID>
    <cbc:IssueDate>2024-01-15</cbc:IssueDate>
    <cbc:IssueTime>14:30:00Z</cbc:IssueTime>
    <cbc:InvoiceTypeCode name=""0200000"">388</cbc:InvoiceTypeCode>
    <cac:Signature>
        <cbc:ID>urn:oasis:names:specification:ubl:signature:Invoice</cbc:ID>
        <cbc:SignatureMethod>urn:oasis:names:specification:ubl:dsig:enveloped:xades</cbc:SignatureMethod>
    </cac:Signature>
    <cac:AccountingSupplierParty>
        <cac:Party>
            <cac:PartyLegalEntity>
                <cbc:RegistrationName>Test Company Ltd</cbc:RegistrationName>
            </cac:PartyLegalEntity>
            <cac:PartyTaxScheme>
                <cbc:CompanyID>300000000000003</cbc:CompanyID>
            </cac:PartyTaxScheme>
        </cac:Party>
    </cac:AccountingSupplierParty>
    <cac:LegalMonetaryTotal>
        <cbc:TaxInclusiveAmount currencyID=""SAR"">1150.00</cbc:TaxInclusiveAmount>
    </cac:LegalMonetaryTotal>
    <cac:TaxTotal>
        <cbc:TaxAmount currencyID=""SAR"">150.00</cbc:TaxAmount>
    </cac:TaxTotal>
    <cac:AdditionalDocumentReference>
        <cbc:ID>QR</cbc:ID>
        <cac:Attachment>
            <cbc:EmbeddedDocumentBinaryObject mimeCode=""text/plain"">QRCodeBase64Here</cbc:EmbeddedDocumentBinaryObject>
        </cac:Attachment>
    </cac:AdditionalDocumentReference>
</Invoice>";

    [Fact]
    public void TestFromString_CreatesInstance()
    {
        // Act
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Assert
        Assert.NotNull(invoiceExt);
        var document = invoiceExt.GetDocument();
        Assert.NotNull(document);
        Assert.NotNull(document.Root);
    }

    [Fact]
    public void TestFromString_ThrowsOnNullXml()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InvoiceExtension.FromString(null));

        Assert.Contains("XML string cannot be null or empty", exception.Message);
    }

    [Fact]
    public void TestFromString_ThrowsOnEmptyXml()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InvoiceExtension.FromString(string.Empty));

        Assert.Contains("XML string cannot be null or empty", exception.Message);
    }

    [Fact]
    public void TestFromString_ThrowsOnInvalidXml()
    {
        // Arrange
        var invalidXml = "<Invoice><unclosed>";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            InvoiceExtension.FromString(invalidXml));

        Assert.Contains("Failed to parse XML string", exception.Message);
    }

    [Fact]
    public void TestRemoveUblExtensions_RemovesExtensionsElement()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        invoiceExt.RemoveUblExtensions();
        var xmlString = invoiceExt.ToXmlString();

        // Assert
        Assert.DoesNotContain("UBLExtensions", xmlString);
        Assert.DoesNotContain("ExtensionURI", xmlString);
    }

    [Fact]
    public void TestRemoveSignature_RemovesSignatureElement()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        invoiceExt.RemoveSignature();
        var xmlString = invoiceExt.ToXmlString();

        // Assert
        Assert.DoesNotContain("cac:Signature", xmlString);
        Assert.DoesNotContain("SignatureMethod", xmlString);
    }

    [Fact]
    public void TestRemoveQrCodeReference_RemovesQrElement()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        invoiceExt.RemoveQrCodeReference();
        var xmlString = invoiceExt.ToXmlString();

        // Assert
        // Should not contain the AdditionalDocumentReference with ID="QR"
        var document = invoiceExt.GetDocument();
        var cacNs = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        var cbcNs = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

        var qrElements = document.Descendants(cacNs + "AdditionalDocumentReference")
            .Where(e => e.Elements(cbcNs + "ID").Any(id => id.Value == "QR"))
            .ToList();

        Assert.Empty(qrElements);
    }

    [Fact]
    public void TestRemoveQrCodeReference_DoesNotRemoveOtherReferences()
    {
        // Arrange
        var xmlWithMultipleRefs = SampleInvoiceXml.Replace(
            "</Invoice>",
            @"<cac:AdditionalDocumentReference>
                <cbc:ID>OtherRef</cbc:ID>
            </cac:AdditionalDocumentReference>
            </Invoice>");

        var invoiceExt = InvoiceExtension.FromString(xmlWithMultipleRefs);

        // Act
        invoiceExt.RemoveQrCodeReference();
        var xmlString = invoiceExt.ToXmlString();

        // Assert
        Assert.Contains("OtherRef", xmlString);
        Assert.DoesNotContain("<cbc:ID>QR</cbc:ID>", xmlString);
    }

    [Fact]
    public void TestComputeHash_ReturnsBase64Hash()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);
        invoiceExt.RemoveUblExtensions()
                  .RemoveSignature()
                  .RemoveQrCodeReference();

        // Act
        var hash = invoiceExt.ComputeHash();

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);

        // Verify it's valid base64
        var hashBytes = Convert.FromBase64String(hash);
        Assert.Equal(32, hashBytes.Length); // SHA-256 produces 32 bytes
    }

    [Fact]
    public void TestComputeHash_IsDeterministic()
    {
        // Arrange
        var invoiceExt1 = InvoiceExtension.FromString(SampleInvoiceXml);
        var invoiceExt2 = InvoiceExtension.FromString(SampleInvoiceXml);

        invoiceExt1.RemoveUblExtensions().RemoveSignature().RemoveQrCodeReference();
        invoiceExt2.RemoveUblExtensions().RemoveSignature().RemoveQrCodeReference();

        // Act
        var hash1 = invoiceExt1.ComputeHash();
        var hash2 = invoiceExt2.ComputeHash();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void TestGetCanonicalXml_ReturnsCanonicalizedXml()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var canonicalXml = invoiceExt.GetCanonicalXml();

        // Assert
        Assert.NotNull(canonicalXml);
        Assert.NotEmpty(canonicalXml);
        Assert.Contains("<Invoice", canonicalXml);
    }

    [Fact]
    public void TestToXmlString_WithDeclaration()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var xmlString = invoiceExt.ToXmlString(includeDeclaration: true);

        // Assert
        Assert.NotNull(xmlString);
        Assert.Contains("<?xml", xmlString);
        Assert.Contains("<Invoice", xmlString);
    }

    [Fact]
    public void TestToXmlString_WithoutDeclaration()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var xmlString = invoiceExt.ToXmlString(includeDeclaration: false);

        // Assert
        Assert.NotNull(xmlString);
        Assert.DoesNotContain("<?xml", xmlString);
        Assert.Contains("<Invoice", xmlString);
    }

    [Fact]
    public void TestGetSellerName_ReturnsCorrectValue()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var sellerName = invoiceExt.GetSellerName();

        // Assert
        Assert.Equal("Test Company Ltd", sellerName);
    }

    [Fact]
    public void TestGetTaxNumber_ReturnsCorrectValue()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var taxNumber = invoiceExt.GetTaxNumber();

        // Assert
        Assert.Equal("300000000000003", taxNumber);
    }

    [Fact]
    public void TestGetIssueDate_ReturnsCorrectValue()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var issueDate = invoiceExt.GetIssueDate();

        // Assert
        Assert.Equal("2024-01-15", issueDate);
    }

    [Fact]
    public void TestGetIssueTime_ReturnsCorrectValue()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var issueTime = invoiceExt.GetIssueTime();

        // Assert
        Assert.Equal("14:30:00Z", issueTime);
    }

    [Fact]
    public void TestGetIssueTime_AddsZIfMissing()
    {
        // Arrange
        var xmlWithoutZ = SampleInvoiceXml.Replace("14:30:00Z", "14:30:00");
        var invoiceExt = InvoiceExtension.FromString(xmlWithoutZ);

        // Act
        var issueTime = invoiceExt.GetIssueTime();

        // Assert
        Assert.EndsWith("Z", issueTime);
    }

    [Fact]
    public void TestGetTaxInclusiveAmount_ReturnsCorrectValue()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var totalAmount = invoiceExt.GetTaxInclusiveAmount();

        // Assert
        Assert.Equal("1150.00", totalAmount);
    }

    [Fact]
    public void TestGetTaxAmount_ReturnsCorrectValue()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var taxAmount = invoiceExt.GetTaxAmount();

        // Assert
        Assert.Equal("150.00", taxAmount);
    }

    [Fact]
    public void TestGetInvoiceTypeCodeName_ReturnsCorrectValue()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var typeCodeName = invoiceExt.GetInvoiceTypeCodeName();

        // Assert
        Assert.Equal("0200000", typeCodeName);
    }

    [Fact]
    public void TestIsSimplifiedInvoice_ReturnsTrueForSimplified()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var isSimplified = invoiceExt.IsSimplifiedInvoice();

        // Assert
        Assert.True(isSimplified); // Type code name "0200000" starts with "02"
    }

    [Fact]
    public void TestIsSimplifiedInvoice_ReturnsFalseForStandard()
    {
        // Arrange
        var standardInvoiceXml = SampleInvoiceXml.Replace("0200000", "0100000");
        var invoiceExt = InvoiceExtension.FromString(standardInvoiceXml);

        // Act
        var isSimplified = invoiceExt.IsSimplifiedInvoice();

        // Assert
        Assert.False(isSimplified); // Type code name "0100000" starts with "01"
    }

    [Fact]
    public void TestGetElementValue_ReturnsEmptyForNonExistent()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act
        var value = invoiceExt.GetElementValue("cbc:NonExistentElement");

        // Assert
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void TestMethodChaining_WorksCorrectly()
    {
        // Arrange
        var invoiceExt = InvoiceExtension.FromString(SampleInvoiceXml);

        // Act - Chain multiple remove operations
        var result = invoiceExt
            .RemoveUblExtensions()
            .RemoveSignature()
            .RemoveQrCodeReference();

        // Assert
        Assert.NotNull(result);
        var xmlString = result.ToXmlString();
        Assert.DoesNotContain("UBLExtensions", xmlString);
        Assert.DoesNotContain("cac:Signature", xmlString);
        Assert.DoesNotContain("<cbc:ID>QR</cbc:ID>", xmlString);
    }

    [Fact]
    public void TestRemoveUblExtensions_OnXmlWithoutExtensions()
    {
        // Arrange
        var simpleXml = @"<?xml version=""1.0""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
    <cbc:ID>INV-001</cbc:ID>
</Invoice>";
        var invoiceExt = InvoiceExtension.FromString(simpleXml);

        // Act - Should not throw
        invoiceExt.RemoveUblExtensions();

        // Assert
        var xmlString = invoiceExt.ToXmlString();
        Assert.Contains("INV-001", xmlString);
    }

    [Fact]
    public void TestComputeHash_ChangesWhenContentChanges()
    {
        // Arrange
        var invoiceExt1 = InvoiceExtension.FromString(SampleInvoiceXml);
        var modifiedXml = SampleInvoiceXml.Replace("Test Company Ltd", "Different Company");
        var invoiceExt2 = InvoiceExtension.FromString(modifiedXml);

        // Act
        var hash1 = invoiceExt1.ComputeHash();
        var hash2 = invoiceExt2.ComputeHash();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
}
