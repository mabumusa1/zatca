using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using Xunit;
using Zatca.EInvoice.Signing;

namespace Zatca.EInvoice.Tests.Signing
{
    /// <summary>
    /// Tests for InvoiceSigner class.
    /// </summary>
    public class InvoiceSignerTests : IDisposable
    {
        private X509Certificate2? _testCertificate;
        private const string TestInvoiceXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
         xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
    <cbc:ProfileID>reporting:1.0</cbc:ProfileID>
    <cbc:ID>INV-001</cbc:ID>
    <cbc:UUID>12345678-1234-1234-1234-123456789012</cbc:UUID>
    <cbc:IssueDate>2024-01-01</cbc:IssueDate>
    <cbc:IssueTime>12:00:00</cbc:IssueTime>
    <cbc:InvoiceTypeCode name=""simplified"">388</cbc:InvoiceTypeCode>
    <cbc:DocumentCurrencyCode>SAR</cbc:DocumentCurrencyCode>
    <cbc:TaxCurrencyCode>SAR</cbc:TaxCurrencyCode>
    <cac:AccountingSupplierParty>
        <cac:Party>
            <cac:PartyTaxScheme>
                <cbc:CompanyID>300000000000003</cbc:CompanyID>
                <cac:TaxScheme>
                    <cbc:ID>VAT</cbc:ID>
                </cac:TaxScheme>
            </cac:PartyTaxScheme>
            <cac:PartyLegalEntity>
                <cbc:RegistrationName>Test Company</cbc:RegistrationName>
            </cac:PartyLegalEntity>
        </cac:Party>
    </cac:AccountingSupplierParty>
    <cac:TaxTotal>
        <cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
    </cac:TaxTotal>
    <cac:LegalMonetaryTotal>
        <cbc:LineExtensionAmount currencyID=""SAR"">100.00</cbc:LineExtensionAmount>
        <cbc:TaxExclusiveAmount currencyID=""SAR"">100.00</cbc:TaxExclusiveAmount>
        <cbc:TaxInclusiveAmount currencyID=""SAR"">115.00</cbc:TaxInclusiveAmount>
        <cbc:PayableAmount currencyID=""SAR"">115.00</cbc:PayableAmount>
    </cac:LegalMonetaryTotal>
</Invoice>";

        /// <summary>
        /// Test that Sign() produces valid signed XML with signature elements.
        /// </summary>
        [Fact]
        public void TestSignInvoiceProducesValidOutput()
        {
            // Arrange
            var certificate = CreateMockCertificate();

            // Act
            var result = InvoiceSigner.Sign(TestInvoiceXml, certificate);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SignedXml);
            Assert.NotEmpty(result.SignedXml);
            Assert.NotNull(result.Hash);
            Assert.NotEmpty(result.Hash);
            Assert.NotNull(result.QrCode);
            Assert.NotEmpty(result.QrCode);
            Assert.NotNull(result.DigitalSignature);
            Assert.NotEmpty(result.DigitalSignature);

            // Verify signed XML contains required elements
            Assert.Contains("UBLExtensions", result.SignedXml);
            Assert.Contains("AdditionalDocumentReference", result.SignedXml);
            Assert.Contains("QR", result.SignedXml);
            Assert.Contains("Signature", result.SignedXml);

            // Verify it's valid XML
            var doc = XDocument.Parse(result.SignedXml);
            Assert.NotNull(doc.Root);
        }

        /// <summary>
        /// Test that GetHash() returns a valid SHA-256 hash.
        /// </summary>
        [Fact]
        public void TestGetHash()
        {
            // Act
            var hash = InvoiceSigner.GetHash(TestInvoiceXml);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);

            // base64(sha256()) = 44 characters
            Assert.Equal(44, hash.Length);

            // Hash should be base64 encoded
            var hashBytes = Convert.FromBase64String(hash);

            // SHA-256 produces 32 bytes
            Assert.Equal(32, hashBytes.Length);
        }

        /// <summary>
        /// Test that GetQrCode() returns a valid QR code string.
        /// </summary>
        [Fact]
        public void TestGetQrCode()
        {
            // Arrange
            var certificate = CreateMockCertificate();
            var signedResult = InvoiceSigner.Sign(TestInvoiceXml, certificate);

            // Act
            var qrCode = InvoiceSigner.GetQrCode(
                signedResult.SignedXml,
                certificate,
                signedResult.Hash,
                signedResult.DigitalSignature);

            // Assert
            Assert.NotNull(qrCode);
            Assert.NotEmpty(qrCode);

            // QR code should be base64 encoded
            var qrBytes = Convert.FromBase64String(qrCode);
            Assert.NotEmpty(qrBytes);
        }

        /// <summary>
        /// Test that Sign() throws ArgumentNullException for null invoice XML.
        /// </summary>
        [Fact]
        public void TestSignThrowsExceptionForNullInvoiceXml()
        {
            // Arrange
            var certificate = CreateMockCertificate();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                InvoiceSigner.Sign(null!, certificate));
        }

        /// <summary>
        /// Test that Sign() throws ArgumentNullException for empty invoice XML.
        /// </summary>
        [Fact]
        public void TestSignThrowsExceptionForEmptyInvoiceXml()
        {
            // Arrange
            var certificate = CreateMockCertificate();

            // Act & Assert - Empty string throws ArgumentException from InvoiceExtension.FromString
            Assert.Throws<ArgumentException>(() =>
                InvoiceSigner.Sign("", certificate));
        }

        /// <summary>
        /// Test that Sign() throws ArgumentNullException for null certificate.
        /// </summary>
        [Fact]
        public void TestSignThrowsExceptionForNullCertificate()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                InvoiceSigner.Sign(TestInvoiceXml, null!));
        }

        /// <summary>
        /// Test that Sign() throws ArgumentException for certificate without private key.
        /// </summary>
        [Fact]
        public void TestSignThrowsExceptionForCertificateWithoutPrivateKey()
        {
            // Arrange - Create a certificate without private key
            var certWithoutKey = new X509Certificate2(CreateMockCertificate().Export(X509ContentType.Cert));

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                InvoiceSigner.Sign(TestInvoiceXml, certWithoutKey));
        }

        /// <summary>
        /// Test that GetHash() produces consistent results for the same input.
        /// </summary>
        [Fact]
        public void TestGetHashConsistency()
        {
            // Act
            var hash1 = InvoiceSigner.GetHash(TestInvoiceXml);
            var hash2 = InvoiceSigner.GetHash(TestInvoiceXml);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        /// <summary>
        /// Test that GetHash() produces different results for different inputs.
        /// </summary>
        [Fact]
        public void TestGetHashDifferentForDifferentInputs()
        {
            // Arrange
            var modifiedXml = TestInvoiceXml.Replace("INV-001", "INV-002");

            // Act
            var hash1 = InvoiceSigner.GetHash(TestInvoiceXml);
            var hash2 = InvoiceSigner.GetHash(modifiedXml);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        /// <summary>
        /// Test that GetHash() throws ArgumentNullException for null input.
        /// </summary>
        [Fact]
        public void TestGetHashThrowsExceptionForNullInput()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                InvoiceSigner.GetHash(null!));
        }

        /// <summary>
        /// Test that signed invoice contains all required QR tags.
        /// </summary>
        [Fact]
        public void TestSignedInvoiceContainsQrTags()
        {
            // Arrange
            var certificate = CreateMockCertificate();

            // Act
            var result = InvoiceSigner.Sign(TestInvoiceXml, certificate);

            // Assert
            // Verify QR code is embedded in the signed XML
            var doc = XDocument.Parse(result.SignedXml);
            var ns = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var cbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

            var qrElement = doc.Descendants(XName.Get("AdditionalDocumentReference", ns))
                .FirstOrDefault(e => e.Element(XName.Get("ID", cbcNs))?.Value == "QR");

            Assert.NotNull(qrElement);
        }

        /// <summary>
        /// Creates a mock X509Certificate2 with ECDSA private key for testing.
        /// </summary>
        private X509Certificate2 CreateMockCertificate()
        {
            if (_testCertificate != null)
                return _testCertificate;

            // Generate EC key pair
            using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

            var certRequest = new CertificateRequest(
                "CN=Test Certificate, O=Test Org, C=SA",
                ecdsa,
                HashAlgorithmName.SHA256);

            certRequest.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature,
                    critical: true));

            certRequest.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(
                    certificateAuthority: false,
                    hasPathLengthConstraint: false,
                    pathLengthConstraint: 0,
                    critical: true));

            // Create self-signed certificate
            var certificate = certRequest.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddYears(1));

            // Export and re-import to ensure private key is properly attached
            var pfxBytes = certificate.Export(X509ContentType.Pfx, "test");
            _testCertificate = new X509Certificate2(pfxBytes, "test",
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);

            return _testCertificate;
        }

        /// <summary>
        /// Test signing with large invoice XML (performance test).
        /// </summary>
        [Fact]
        public void TestSignLargeInvoiceXml()
        {
            // Arrange
            var certificate = CreateMockCertificate();
            var largeInvoice = GenerateLargeInvoiceXml();

            // Act
            var result = InvoiceSigner.Sign(largeInvoice, certificate);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SignedXml);
            Assert.NotEmpty(result.Hash);
            Assert.NotEmpty(result.QrCode);
            Assert.NotEmpty(result.DigitalSignature);
        }

        /// <summary>
        /// Test signing multiple invoices in sequence.
        /// </summary>
        [Fact]
        public void TestSignMultipleInvoicesInSequence()
        {
            // Arrange
            var certificate = CreateMockCertificate();
            var results = new List<SignedInvoiceResult>();

            // Act
            for (int i = 0; i < 5; i++)
            {
                var modifiedXml = TestInvoiceXml.Replace("INV-001", $"INV-{i:000}");
                var result = InvoiceSigner.Sign(modifiedXml, certificate);
                results.Add(result);
            }

            // Assert
            Assert.Equal(5, results.Count);

            // Each result should have unique hash
            var hashes = results.Select(r => r.Hash).Distinct().ToList();
            Assert.Equal(5, hashes.Count);

            // Each result should have unique signature
            var signatures = results.Select(r => r.DigitalSignature).Distinct().ToList();
            Assert.Equal(5, signatures.Count);
        }

        /// <summary>
        /// Test that signed XML is valid and parseable.
        /// </summary>
        [Fact]
        public void TestSignedXmlIsValidXml()
        {
            // Arrange
            var certificate = CreateMockCertificate();

            // Act
            var result = InvoiceSigner.Sign(TestInvoiceXml, certificate);

            // Assert
            var doc = XDocument.Parse(result.SignedXml);
            Assert.NotNull(doc.Root);
            Assert.Equal("Invoice", doc.Root.Name.LocalName);

            // Verify namespace declarations are preserved
            var invoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            Assert.Equal(invoiceNs, doc.Root.Name.NamespaceName);
        }

        /// <summary>
        /// Test that hash changes when invoice content changes.
        /// </summary>
        [Fact]
        public void TestHashSensitivityToInvoiceChanges()
        {
            // Arrange
            var modifiedXml1 = TestInvoiceXml.Replace("100.00", "100.01");
            var modifiedXml2 = TestInvoiceXml.Replace("Test Company", "Test Company Inc");

            // Act
            var hash1 = InvoiceSigner.GetHash(TestInvoiceXml);
            var hash2 = InvoiceSigner.GetHash(modifiedXml1);
            var hash3 = InvoiceSigner.GetHash(modifiedXml2);

            // Assert - All hashes should be different
            Assert.NotEqual(hash1, hash2);
            Assert.NotEqual(hash1, hash3);
            Assert.NotEqual(hash2, hash3);
        }

        /// <summary>
        /// Test signing with XML containing special characters.
        /// </summary>
        [Fact]
        public void TestSignInvoiceWithSpecialCharacters()
        {
            // Arrange
            var certificate = CreateMockCertificate();
            // Use properly XML-escaped special characters
            var specialCharXml = TestInvoiceXml.Replace("Test Company", "Test &amp; Company &lt; &gt; \"quoted\"");

            // Act
            var result = InvoiceSigner.Sign(specialCharXml, certificate);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.SignedXml);

            // Verify XML is still valid
            var doc = XDocument.Parse(result.SignedXml);
            Assert.NotNull(doc.Root);
        }

        /// <summary>
        /// Test that QR code is properly base64 encoded.
        /// </summary>
        [Fact]
        public void TestQrCodeIsValidBase64()
        {
            // Arrange
            var certificate = CreateMockCertificate();

            // Act
            var result = InvoiceSigner.Sign(TestInvoiceXml, certificate);

            // Assert
            byte[] qrBytes = null;
            var exception = Record.Exception(() => qrBytes = Convert.FromBase64String(result.QrCode));

            Assert.Null(exception);
            Assert.NotNull(qrBytes);
            Assert.NotEmpty(qrBytes);
        }

        /// <summary>
        /// Test that signature value is properly base64 encoded.
        /// </summary>
        [Fact]
        public void TestDigitalSignatureIsValidBase64()
        {
            // Arrange
            var certificate = CreateMockCertificate();

            // Act
            var result = InvoiceSigner.Sign(TestInvoiceXml, certificate);

            // Assert
            byte[] sigBytes = null;
            var exception = Record.Exception(() => sigBytes = Convert.FromBase64String(result.DigitalSignature));

            Assert.Null(exception);
            Assert.NotNull(sigBytes);
            Assert.NotEmpty(sigBytes);
        }

        /// <summary>
        /// Test GetHash with whitespace variations.
        /// </summary>
        [Fact]
        public void TestGetHashWithWhitespaceVariations()
        {
            // Arrange
            var xml1 = TestInvoiceXml;
            var xml2 = TestInvoiceXml.Replace("  ", "    "); // Different whitespace

            // Act
            var hash1 = InvoiceSigner.GetHash(xml1);
            var hash2 = InvoiceSigner.GetHash(xml2);

            // Assert - Hashes might differ due to whitespace, testing robustness
            Assert.NotNull(hash1);
            Assert.NotNull(hash2);
            Assert.NotEmpty(hash1);
            Assert.NotEmpty(hash2);
        }

        /// <summary>
        /// Test concurrent invoice signing (thread safety).
        /// </summary>
        [Fact]
        public async Task TestConcurrentInvoiceSigning()
        {
            // Arrange
            var certificate = CreateMockCertificate();
            var tasks = new List<Task<SignedInvoiceResult>>();
            var numberOfConcurrentSigns = 10;

            // Act
            for (int i = 0; i < numberOfConcurrentSigns; i++)
            {
                var invoiceNumber = i;
                var task = Task.Run(() =>
                {
                    var modifiedXml = TestInvoiceXml.Replace("INV-001", $"INV-{invoiceNumber:000}");
                    return InvoiceSigner.Sign(modifiedXml, certificate);
                });
                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.NotNull(result);
                Assert.NotEmpty(result.Hash);
                Assert.NotEmpty(result.SignedXml);
                Assert.NotEmpty(result.QrCode);
            }

            // All signatures should be unique
            var signatures = results.Select(r => r.DigitalSignature).Distinct().ToList();
            Assert.Equal(numberOfConcurrentSigns, signatures.Count);
        }

        /// <summary>
        /// Test GetQrCode throws exception for null signed XML.
        /// </summary>
        [Fact]
        public void TestGetQrCodeThrowsForNullSignedXml()
        {
            // Arrange
            var certificate = CreateMockCertificate();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                InvoiceSigner.GetQrCode(null!, certificate, "hash", "signature"));
        }

        /// <summary>
        /// Test GetQrCode throws exception for null certificate.
        /// </summary>
        [Fact]
        public void TestGetQrCodeThrowsForNullCertificate()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                InvoiceSigner.GetQrCode("<Invoice></Invoice>", null!, "hash", "signature"));
        }

        /// <summary>
        /// Test signing invoice with minimal XML structure.
        /// </summary>
        [Fact]
        public void TestSignMinimalInvoiceXml()
        {
            // Arrange
            var certificate = CreateMockCertificate();
            var minimalXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2""
         xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2""
         xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"">
    <cbc:ProfileID>reporting:1.0</cbc:ProfileID>
    <cbc:ID>MIN-001</cbc:ID>
</Invoice>";

            // Act
            var result = InvoiceSigner.Sign(minimalXml, certificate);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Hash);
            Assert.NotEmpty(result.SignedXml);
        }

        /// <summary>
        /// Test that hash length is consistent (base64(sha256()) = 44 chars, 32 bytes).
        /// </summary>
        [Fact]
        public void TestHashLengthConsistency()
        {
            // Arrange
            var invoices = new[] { TestInvoiceXml, TestInvoiceXml.Replace("INV-001", "INV-999") };

            // Act & Assert
            foreach (var invoice in invoices)
            {
                var hash = InvoiceSigner.GetHash(invoice);

                // base64(sha256()) = 44 characters
                Assert.Equal(44, hash.Length);

                var hashBytes = Convert.FromBase64String(hash);
                Assert.Equal(32, hashBytes.Length); // SHA-256 produces 32 bytes
            }
        }

        /// <summary>
        /// Generates a large invoice XML for performance testing.
        /// </summary>
        private static string GenerateLargeInvoiceXml()
        {
            var baseXml = TestInvoiceXml.Replace("</Invoice>", "");
            var invoiceLines = new StringBuilder();

            for (int i = 1; i <= 100; i++)
            {
                invoiceLines.AppendLine($@"
    <cac:InvoiceLine>
        <cbc:ID>{i}</cbc:ID>
        <cbc:InvoicedQuantity unitCode=""PCE"">1.00</cbc:InvoicedQuantity>
        <cbc:LineExtensionAmount currencyID=""SAR"">100.00</cbc:LineExtensionAmount>
        <cac:Item>
            <cbc:Name>Product {i}</cbc:Name>
        </cac:Item>
        <cac:Price>
            <cbc:PriceAmount currencyID=""SAR"">100.00</cbc:PriceAmount>
        </cac:Price>
    </cac:InvoiceLine>");
            }

            return baseXml + invoiceLines.ToString() + "</Invoice>";
        }

        /// <summary>
        /// Test that the hash in SignedInvoiceResult matches what ZATCA would compute
        /// by stripping the signed XML and recomputing the hash — the same process ZATCA performs.
        /// </summary>
        [Fact]
        public void TestHashMatchesAfterStrippingSignedXml()
        {
            // Arrange: Sign an invoice
            var certificate = CreateMockCertificate();
            var result = InvoiceSigner.Sign(TestInvoiceXml, certificate);

            // Act: Simulate what ZATCA does — parse the signed XML, strip elements, hash
            var zatcaView = InvoiceExtension.FromString(result.SignedXml);
            zatcaView
                .RemoveUblExtensions()
                .RemoveSignature()
                .RemoveQrCodeReference();
            var recomputedHash = zatcaView.ComputeHash();

            // Debug: Check if EnsureExtNamespace and EnsureIssueTimeHasUtcSuffix are reflected in C14N
            var verifyView = InvoiceExtension.FromString(TestInvoiceXml);
            verifyView.EnsureIssueTimeHasUtcSuffix(); // Must match what Sign() does
            verifyView.RemoveUblExtensions().RemoveSignature().RemoveQrCodeReference();
            verifyView.EnsureExtNamespace();
            var verifyC14N = verifyView.GetCanonicalXml();
            var zatcaC14N = zatcaView.GetCanonicalXml();

            Assert.True(verifyC14N.Contains("xmlns:ext"),
                $"EnsureExtNamespace C14N should have xmlns:ext. First 400:\n{verifyC14N[..Math.Min(400, verifyC14N.Length)]}");

            // Show diffs
            if (verifyC14N != zatcaC14N)
            {
                // Find first difference
                var minLen = Math.Min(verifyC14N.Length, zatcaC14N.Length);
                var diffPos = -1;
                for (int i = 0; i < minLen; i++)
                {
                    if (verifyC14N[i] != zatcaC14N[i]) { diffPos = i; break; }
                }
                if (diffPos == -1) diffPos = minLen; // lengths differ
                var start = Math.Max(0, diffPos - 50);
                var end1 = Math.Min(verifyC14N.Length, diffPos + 50);
                var end2 = Math.Min(zatcaC14N.Length, diffPos + 50);
                Assert.Fail(
                    $"C14N differs at position {diffPos}.\n" +
                    $"Verify len={verifyC14N.Length}, ZATCA len={zatcaC14N.Length}\n" +
                    $"Verify [{start}..{end1}]: [{verifyC14N[start..end1]}]\n" +
                    $"ZATCA  [{start}..{end2}]: [{zatcaC14N[start..end2]}]");
            }
            Assert.Equal(result.Hash, recomputedHash);
        }

        /// <summary>
        /// Same test but using XML generated by InvoiceGenerator (closer to real usage).
        /// </summary>
        [Fact]
        public void TestHashMatchesForGeneratedInvoice()
        {
            // Arrange: Generate invoice XML from data (like ZatcaSampleInvoiceService does)
            var invoiceData = new Dictionary<string, object>
            {
                ["uuid"] = Guid.NewGuid().ToString(),
                ["id"] = "TEST-001",
                ["issueDate"] = "2024-01-01",
                ["issueTime"] = "12:00:00",
                ["currencyCode"] = "SAR",
                ["taxCurrencyCode"] = "SAR",
                ["invoiceType"] = new Dictionary<string, object>
                {
                    ["typeCode"] = "388",
                    ["name"] = "0200000"
                },
                ["additionalDocuments"] = new System.Collections.Generic.List<object>
                {
                    new Dictionary<string, object> { ["id"] = "ICV", ["uuid"] = "1" },
                    new Dictionary<string, object>
                    {
                        ["id"] = "PIH",
                        ["attachment"] = new Dictionary<string, object>
                        {
                            ["embeddedDocument"] = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==",
                            ["mimeCode"] = "text/plain"
                        }
                    }
                },
                ["signature"] = new Dictionary<string, object>
                {
                    ["id"] = "urn:oasis:names:specification:ubl:signature:Invoice",
                    ["signatureMethod"] = "urn:oasis:names:specification:ubl:dsig:enveloped:xades"
                },
                ["supplier"] = new Dictionary<string, object>
                {
                    ["registrationName"] = "Test Company",
                    ["taxId"] = "300000000000003",
                    ["partyIdentification"] = "1010010000",
                    ["partyIdentificationId"] = "CRN",
                    ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" },
                    ["address"] = new Dictionary<string, object>
                    {
                        ["street"] = "Test Street",
                        ["buildingNumber"] = "1234",
                        ["citySubdivisionName"] = "Test District",
                        ["city"] = "Riyadh",
                        ["postalZone"] = "12345",
                        ["country"] = "SA"
                    }
                },
                ["customer"] = new Dictionary<string, object>(),
                ["paymentMeans"] = new Dictionary<string, object> { ["code"] = "10" },
                ["delivery"] = new Dictionary<string, object> { ["actualDeliveryDate"] = "2024-01-01" },
                ["taxTotal"] = new Dictionary<string, object>
                {
                    ["taxAmount"] = 15.0m,
                    ["subTotals"] = new System.Collections.Generic.List<object>
                    {
                        new Dictionary<string, object>
                        {
                            ["taxableAmount"] = 100.0m,
                            ["taxAmount"] = 15.0m,
                            ["taxCategory"] = new Dictionary<string, object>
                            {
                                ["id"] = "S",
                                ["percent"] = 15,
                                ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                            }
                        }
                    }
                },
                ["legalMonetaryTotal"] = new Dictionary<string, object>
                {
                    ["lineExtensionAmount"] = 100.0m,
                    ["taxExclusiveAmount"] = 100.0m,
                    ["taxInclusiveAmount"] = 115.0m,
                    ["prepaidAmount"] = 0.00m,
                    ["payableAmount"] = 115.0m,
                    ["allowanceTotalAmount"] = 0.00m
                },
                ["invoiceLines"] = new System.Collections.Generic.List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "1",
                        ["quantity"] = 1.0m,
                        ["unitCode"] = "PCE",
                        ["lineExtensionAmount"] = 100.0m,
                        ["item"] = new Dictionary<string, object>
                        {
                            ["name"] = "Test Item",
                            ["classifiedTaxCategory"] = new System.Collections.Generic.List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    ["id"] = "S",
                                    ["percent"] = 15,
                                    ["taxScheme"] = new Dictionary<string, object> { ["id"] = "VAT" }
                                }
                            }
                        },
                        ["price"] = new Dictionary<string, object>
                        {
                            ["amount"] = 100.0m,
                            ["baseQuantity"] = 1.0m
                        },
                        ["taxTotal"] = new Dictionary<string, object>
                        {
                            ["taxAmount"] = 15.0m,
                            ["roundingAmount"] = 115.0m
                        }
                    }
                }
            };

            var xmlGenerator = new Zatca.EInvoice.Xml.InvoiceGenerator();
            var xmlContent = xmlGenerator.Generate(invoiceData);

            var certificate = CreateMockCertificate();

            // Act: Sign and then recompute hash from signed XML (simulating ZATCA)
            var result = InvoiceSigner.Sign(xmlContent, certificate);

            var zatcaView = InvoiceExtension.FromString(result.SignedXml);
            zatcaView
                .RemoveUblExtensions()
                .RemoveSignature()
                .RemoveQrCodeReference();
            var recomputedHash = zatcaView.ComputeHash();

            // Assert
            Assert.Equal(result.Hash, recomputedHash);
        }

        /// <summary>
        /// Cleanup test certificate.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _testCertificate?.Dispose();
        }
    }
}
