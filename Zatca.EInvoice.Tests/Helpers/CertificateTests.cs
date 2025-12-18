using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;
using Zatca.EInvoice.Certificates;

namespace Zatca.EInvoice.Tests.Helpers;

/// <summary>
/// Tests for CertificateInfo class.
/// </summary>
public class CertificateTests
{
    // Sample certificate for testing (base64 format, matching PHP tests)
    private const string TestCertificatePem = "MIICAzCCAaqgAwIBAgIGAZT7anBcMAoGCCqGSM49BAMCMBUxEzARBgNVBAMMCmVJbnZvaWNpbmcwHhcNMjUwMjEyMTgyNzE5WhcNMzAwMjExMjEwMDAwWjBUMRgwFgYDVQQDDA9NeSBPcmdhbml6YXRpb24xEzARBgNVBAoMCk15IENvbXBhbnkxFjAUBgNVBAsMDUlUIERlcGFydG1lbnQxCzAJBgNVBAYTAlNBMFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEdg+fe1K42qCMlH8MQmxi02RzKU3SfNHA5QUTh9ub6vqiTvY5ON0Q3CjBJ2qzrCeBguijyQQCFARDulpKaWAaW6OBqTCBpjAMBgNVHRMBAf8EAjAAMIGVBgNVHREEgY0wgYqkgYcwgYQxIDAeBgNVBAQMFzEtU2FsZWh8Mi0xbnwzLVNNRTAwMDIzMR8wHQYKCZImiZPyLGQBAQwPMzEyMzQ1Njc4OTAxMjMzMQ0wCwYDVQQMDAQxMTAwMRswGQYDVQQaDBJSaXlhZGggMTIzNCBTdHJlZXQxEzARBgNVBA8MClRlY2hub2xvZ3kwCgYIKoZIzj0EAwIDRwAwRAIgINT+MFQefLLdd7Jlayr8nZq1lQrXQgKYxuA14LRoDvUCIGVS+MserlYamKvlCtk/g9J4gPWoJMXygSGp7FTPV8e4";

    // Sample EC private key for testing (base64 format, matching PHP tests)
    private const string TestPrivateKeyPem = "MIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQgPsPX88rLECL/346pDroiltt9ZFz8arMlt3FHeqdxaD6hRANCAAR2D597UrjaoIyUfwxCbGLTZHMpTdJ80cDlBROH25vq+qJO9jk43RDcKMEnarOsJ4GC6KPJBAIUBEO6WkppYBpb";

    private const string TestSecret = "test-secret-key-123";

    [Fact]
    public void TestGetRawCertificate_ReturnsRawCertificateString()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var rawCert = certInfo.RawCertificate;

        // Assert - Raw certificate should match the input certificate data
        Assert.NotNull(rawCert);
        Assert.NotEmpty(rawCert);
        Assert.Equal(TestCertificatePem, rawCert);
    }

    [Fact]
    public void TestGetCertificateHash_ReturnsSha256Hash()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var certHash = certInfo.GetCertificateHash();

        // Assert
        Assert.NotNull(certHash);
        Assert.NotEmpty(certHash);

        // Verify it's valid base64
        var hashBytes = Convert.FromBase64String(certHash);
        Assert.Equal(32, hashBytes.Length); // SHA-256 produces 32 bytes

        // Verify hash is consistent
        var certHash2 = certInfo.GetCertificateHash();
        Assert.Equal(certHash, certHash2);
    }

    [Fact]
    public void TestGetFormattedIssuer_ReturnsFormattedIssuerString()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var formattedIssuer = certInfo.GetFormattedIssuer();

        // Assert
        Assert.NotNull(formattedIssuer);
        Assert.NotEmpty(formattedIssuer);
        // The formatted issuer should contain reversed parts
        Assert.Contains("CN=", formattedIssuer);
    }

    [Fact]
    public void TestGetRawPublicKeyBase64_ExtractsPublicKey()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var publicKeyBase64 = certInfo.GetRawPublicKeyBase64();

        // Assert
        Assert.NotNull(publicKeyBase64);
        Assert.NotEmpty(publicKeyBase64);

        // Verify it's valid base64 without headers
        Assert.DoesNotContain("BEGIN", publicKeyBase64);
        Assert.DoesNotContain("END", publicKeyBase64);
        Assert.DoesNotContain("\n", publicKeyBase64);
        Assert.DoesNotContain("\r", publicKeyBase64);

        // Verify it can be decoded
        var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
        Assert.NotEmpty(publicKeyBytes);
    }

    [Fact]
    public void TestGetAuthorizationHeader_ReturnsBasicAuthHeader()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var authHeader = certInfo.GetAuthorizationHeader();

        // Assert
        Assert.NotNull(authHeader);
        Assert.StartsWith("Basic ", authHeader);

        // Verify the format: Basic base64(base64(cert):secret)
        var base64Part = authHeader.Substring(6); // Remove "Basic "
        var decodedBytes = Convert.FromBase64String(base64Part);
        var decoded = Encoding.UTF8.GetString(decodedBytes);

        Assert.Contains(":", decoded);
        var parts = decoded.Split(':');
        Assert.Equal(2, parts.Length);

        // First part should be base64-encoded certificate
        var certBytes = Convert.FromBase64String(parts[0]);
        Assert.NotEmpty(certBytes);

        // Second part should be the secret
        Assert.Equal(TestSecret, parts[1]);
    }

    [Fact]
    public void TestCertificateProperties_AreAccessible()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act & Assert
        Assert.NotNull(certInfo.Certificate);
        Assert.NotNull(certInfo.Issuer);
        Assert.NotNull(certInfo.Subject);
        Assert.NotNull(certInfo.SerialNumber);
        Assert.Equal(TestSecret, certInfo.Secret);
    }

    [Fact]
    public void TestGetPrivateKey_ReturnsPrivateKey()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var privateKey = certInfo.GetPrivateKey();

        // Assert
        Assert.NotNull(privateKey);
        Assert.True(privateKey.IsPrivate);
    }

    [Fact]
    public void TestGetPublicKeyBytes_ReturnsPublicKeyBytes()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var publicKeyBytes = certInfo.GetPublicKeyBytes();

        // Assert
        Assert.NotNull(publicKeyBytes);
        Assert.NotEmpty(publicKeyBytes);
    }

    [Fact]
    public void TestGetCertificateSignature_ReturnsSignature()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var signature = certInfo.GetCertificateSignature();

        // Assert
        Assert.NotNull(signature);
        Assert.NotEmpty(signature);
    }

    [Fact]
    public void TestConstructor_ThrowsOnNullCertificate()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CertificateInfo(null, TestPrivateKeyPem, TestSecret));
    }

    [Fact]
    public void TestConstructor_ThrowsOnEmptyCertificate()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CertificateInfo(string.Empty, TestPrivateKeyPem, TestSecret));
    }

    [Fact]
    public void TestConstructor_ThrowsOnNullPrivateKey()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CertificateInfo(TestCertificatePem, null, TestSecret));
    }

    [Fact]
    public void TestConstructor_ThrowsOnEmptyPrivateKey()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CertificateInfo(TestCertificatePem, string.Empty, TestSecret));
    }

    [Fact]
    public void TestConstructor_AcceptsNullSecret()
    {
        // Act
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, null);

        // Assert
        Assert.NotNull(certInfo);
        Assert.Null(certInfo.Secret);
    }

    [Fact]
    public void TestGetCertificateHash_IsDeterministic()
    {
        // Arrange
        var certInfo1 = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);
        var certInfo2 = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var hash1 = certInfo1.GetCertificateHash();
        var hash2 = certInfo2.GetCertificateHash();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void TestGetRawPublicKeyBase64_IsDeterministic()
    {
        // Arrange
        var certInfo1 = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);
        var certInfo2 = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var publicKey1 = certInfo1.GetRawPublicKeyBase64();
        var publicKey2 = certInfo2.GetRawPublicKeyBase64();

        // Assert
        Assert.Equal(publicKey1, publicKey2);
    }

    [Fact]
    public void TestIssuerAndSubject_ArePopulated()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var issuer = certInfo.Issuer;
        var subject = certInfo.Subject;

        // Assert
        Assert.NotNull(issuer);
        Assert.NotEmpty(issuer);
        Assert.NotNull(subject);
        Assert.NotEmpty(subject);
        Assert.Contains("CN=", issuer);
        Assert.Contains("CN=", subject);
    }

    [Fact]
    public void TestSerialNumber_IsAccessible()
    {
        // Arrange
        var certInfo = new CertificateInfo(TestCertificatePem, TestPrivateKeyPem, TestSecret);

        // Act
        var serialNumber = certInfo.SerialNumber;

        // Assert
        Assert.NotNull(serialNumber);
        Assert.NotEmpty(serialNumber);
    }
}
