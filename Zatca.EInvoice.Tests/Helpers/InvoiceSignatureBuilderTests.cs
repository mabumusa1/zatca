using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Xunit;
using Zatca.EInvoice.Signing;

namespace Zatca.EInvoice.Tests.Helpers;

/// <summary>
/// Tests for SignatureBuilder class.
/// </summary>
public class InvoiceSignatureBuilderTests
{
    // Sample certificate for testing (base64 format, matching PHP tests)
    private const string TestCertificatePem = "MIICAzCCAaqgAwIBAgIGAZT7anBcMAoGCCqGSM49BAMCMBUxEzARBgNVBAMMCmVJbnZvaWNpbmcwHhcNMjUwMjEyMTgyNzE5WhcNMzAwMjExMjEwMDAwWjBUMRgwFgYDVQQDDA9NeSBPcmdhbml6YXRpb24xEzARBgNVBAoMCk15IENvbXBhbnkxFjAUBgNVBAsMDUlUIERlcGFydG1lbnQxCzAJBgNVBAYTAlNBMFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEdg+fe1K42qCMlH8MQmxi02RzKU3SfNHA5QUTh9ub6vqiTvY5ON0Q3CjBJ2qzrCeBguijyQQCFARDulpKaWAaW6OBqTCBpjAMBgNVHRMBAf8EAjAAMIGVBgNVHREEgY0wgYqkgYcwgYQxIDAeBgNVBAQMFzEtU2FsZWh8Mi0xbnwzLVNNRTAwMDIzMR8wHQYKCZImiZPyLGQBAQwPMzEyMzQ1Njc4OTAxMjMzMQ0wCwYDVQQMDAQxMTAwMRswGQYDVQQaDBJSaXlhZGggMTIzNCBTdHJlZXQxEzARBgNVBA8MClRlY2hub2xvZ3kwCgYIKoZIzj0EAwIDRwAwRAIgINT+MFQefLLdd7Jlayr8nZq1lQrXQgKYxuA14LRoDvUCIGVS+MserlYamKvlCtk/g9J4gPWoJMXygSGp7FTPV8e4";

    private static X509Certificate2 GetTestCertificate()
    {
        var certBytes = Convert.FromBase64String(TestCertificatePem);
        return new X509Certificate2(certBytes);
    }

    [Fact]
    public void TestBuildSignatureXml_ReturnsValidXml()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var invoiceDigest = "SGVsbG9Xb3JsZFRlc3REaWdlc3Q="; // Sample base64 digest
        var signatureValue = "U2FtcGxlU2lnbmF0dXJlVmFsdWU="; // Sample base64 signature

        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest(invoiceDigest)
            .SetSignatureValue(signatureValue);

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert
        Assert.NotNull(signatureXml);
        Assert.NotEmpty(signatureXml);
        Assert.Contains("UBLExtension", signatureXml);
        Assert.Contains("UBLDocumentSignatures", signatureXml);
        Assert.Contains("ds:Signature", signatureXml);
    }

    [Fact]
    public void TestBuildSignatureXml_ContainsExpectedElements()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var invoiceDigest = "dGVzdERpZ2VzdA==";
        var signatureValue = "dGVzdFNpZ25hdHVyZQ==";

        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest(invoiceDigest)
            .SetSignatureValue(signatureValue);

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert
        // Check for key signature elements
        Assert.Contains("SignedInfo", signatureXml);
        Assert.Contains("SignatureValue", signatureXml);
        Assert.Contains("KeyInfo", signatureXml);
        Assert.Contains("QualifyingProperties", signatureXml);
        Assert.Contains("SignedProperties", signatureXml);
        Assert.Contains("X509Certificate", signatureXml);

        // Check for digest value
        Assert.Contains(invoiceDigest, signatureXml);

        // Check for signature value
        Assert.Contains(signatureValue, signatureXml);
    }

    [Fact]
    public void TestBuildSignatureXml_ContainsXAdESElements()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert
        // XAdES specific elements
        Assert.Contains("xades:SignedProperties", signatureXml);
        Assert.Contains("xades:SignedSignatureProperties", signatureXml);
        Assert.Contains("xades:SigningTime", signatureXml);
        Assert.Contains("xades:SigningCertificate", signatureXml);
        Assert.Contains("xades:CertDigest", signatureXml);
        Assert.Contains("xades:IssuerSerial", signatureXml);
    }

    [Fact]
    public void TestBuildSignatureXml_ContainsTransforms()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert
        // Check for XPath transforms
        Assert.Contains("ds:Transforms", signatureXml);
        Assert.Contains("ds:Transform", signatureXml);
        Assert.Contains("http://www.w3.org/TR/1999/REC-xpath-19991116", signatureXml);
        Assert.Contains("not(//ancestor-or-self::ext:UBLExtensions)", signatureXml);
        Assert.Contains("not(//ancestor-or-self::cac:Signature)", signatureXml);
        Assert.Contains("not(//ancestor-or-self::cac:AdditionalDocumentReference[cbc:ID='QR'])", signatureXml);
    }

    [Fact]
    public void TestBuildSignatureXml_ContainsAlgorithms()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert
        // Check for required algorithms
        Assert.Contains("http://www.w3.org/2006/12/xml-c14n11", signatureXml); // Canonicalization
        Assert.Contains("http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256", signatureXml); // Signature method
        Assert.Contains("http://www.w3.org/2001/04/xmlenc#sha256", signatureXml); // Digest method
    }

    [Fact]
    public void TestSettersReturnSelf_ForFluentInterface()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder();

        // Act
        var result1 = builder.SetCertificate(certificate);
        var result2 = result1.SetInvoiceDigest("dGVzdA==");
        var result3 = result2.SetSignatureValue("dGVzdA==");

        // Assert
        Assert.Same(builder, result1);
        Assert.Same(builder, result2);
        Assert.Same(builder, result3);
    }

    [Fact]
    public void TestBuildSignatureXml_ThrowsWhenCertificateNotSet()
    {
        // Arrange
        var builder = new SignatureBuilder()
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");
        // Certificate not set

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.BuildSignatureXml());

        Assert.Contains("Certificate must be set", exception.Message);
    }

    [Fact]
    public void TestBuildSignatureXml_ThrowsWhenInvoiceDigestNotSet()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetSignatureValue("dGVzdA==");
        // Invoice digest not set

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.BuildSignatureXml());

        Assert.Contains("Invoice digest must be set", exception.Message);
    }

    [Fact]
    public void TestBuildSignatureXml_ThrowsWhenSignatureValueNotSet()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==");
        // Signature value not set

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            builder.BuildSignatureXml());

        Assert.Contains("Signature value must be set", exception.Message);
    }

    [Fact]
    public void TestSetCertificate_ThrowsOnNull()
    {
        // Arrange
        var builder = new SignatureBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.SetCertificate(null));
    }

    [Fact]
    public void TestSetInvoiceDigest_ThrowsOnNull()
    {
        // Arrange
        var builder = new SignatureBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.SetInvoiceDigest(null));
    }

    [Fact]
    public void TestSetSignatureValue_ThrowsOnNull()
    {
        // Arrange
        var builder = new SignatureBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.SetSignatureValue(null));
    }

    [Fact]
    public void TestBuildSignatureXml_IsValidXml()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdERpZ2VzdA==")
            .SetSignatureValue("dGVzdFNpZ25hdHVyZQ==");

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert - Should be able to parse as valid XML
        var exception = Record.Exception(() => XDocument.Parse(signatureXml));
        Assert.Null(exception);

        var doc = XDocument.Parse(signatureXml);
        Assert.NotNull(doc.Root);
    }

    [Fact]
    public void TestBuildSignatureXml_ContainsCertificateData()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert
        // Should contain certificate in base64
        var certBase64 = Convert.ToBase64String(certificate.Export(X509ContentType.Cert));
        Assert.Contains(certBase64, signatureXml);

        // Should contain issuer information
        Assert.Contains("X509IssuerName", signatureXml);
        Assert.Contains("X509SerialNumber", signatureXml);
    }

    [Fact]
    public void TestBuildSignatureXml_ContainsSignatureReferences()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert
        // Should contain reference to invoice signed data
        Assert.Contains("invoiceSignedData", signatureXml);

        // Should contain reference to signed properties
        Assert.Contains("xadesSignedProperties", signatureXml);

        // Should contain digest method and digest value
        Assert.Contains("DigestMethod", signatureXml);
        Assert.Contains("DigestValue", signatureXml);
    }

    [Fact]
    public void TestBuildSignatureXml_ContainsNamespaceDeclarations()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert
        // Check for required namespaces
        Assert.Contains("xmlns:sig=\"urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2\"", signatureXml);
        Assert.Contains("xmlns:sac=\"urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2\"", signatureXml);
        Assert.Contains("xmlns:sbc=\"urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2\"", signatureXml);
        Assert.Contains("xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"", signatureXml);
        Assert.Contains("xmlns:xades=\"http://uri.etsi.org/01903/v1.3.2#\"", signatureXml);
    }

    [Fact]
    public async Task TestBuildSignatureXml_GeneratesDifferentSigningTimes()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder1 = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");

        // Act
        var xml1 = builder1.BuildSignatureXml();

        // Small delay to ensure different timestamp
        await Task.Delay(1100);

        var builder2 = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");

        var xml2 = builder2.BuildSignatureXml();

        // Assert - XMLs should be different due to different SigningTime
        Assert.NotEqual(xml1, xml2);
        Assert.Contains("SigningTime", xml1);
        Assert.Contains("SigningTime", xml2);
    }

    [Fact]
    public void TestBuildSignatureXml_DoesNotIncludeXmlDeclaration()
    {
        // Arrange
        var certificate = GetTestCertificate();
        var builder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest("dGVzdA==")
            .SetSignatureValue("dGVzdA==");

        // Act
        var signatureXml = builder.BuildSignatureXml();

        // Assert
        // Should not start with XML declaration
        Assert.DoesNotContain("<?xml", signatureXml.Substring(0, Math.Min(100, signatureXml.Length)));
    }
}
