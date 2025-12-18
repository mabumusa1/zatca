using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Org.BouncyCastle.X509;

namespace Zatca.EInvoice.Signing;

/// <summary>
/// Main orchestrator for signing ZATCA e-invoices with digital signatures and QR codes.
/// This class implements the Category E digital signing requirements.
/// </summary>
public class InvoiceSigner
{
    /// <summary>
    /// Signs an invoice XML with the provided certificate.
    /// </summary>
    /// <param name="xmlInvoice">The unsigned invoice XML string.</param>
    /// <param name="certificate">The X509Certificate2 with private key for signing.</param>
    /// <returns>A SignedInvoiceResult containing the signed XML, hash, and QR code.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentException">Thrown when the certificate doesn't have a private key.</exception>
    public static SignedInvoiceResult Sign(string xmlInvoice, X509Certificate2 certificate)
    {
        if (string.IsNullOrWhiteSpace(xmlInvoice))
            throw new ArgumentNullException(nameof(xmlInvoice));
        if (certificate == null)
            throw new ArgumentNullException(nameof(certificate));
        if (!certificate.HasPrivateKey)
            throw new ArgumentException("Certificate must have a private key.", nameof(certificate));

        // Step 1: Parse the invoice XML
        var invoiceExtension = InvoiceExtension.FromString(xmlInvoice);

        // Extract UUID before any modifications
        var uuid = invoiceExtension.GetUuid();

        // Step 2: Remove elements that should not be included in the hash
        invoiceExtension
            .RemoveUblExtensions()
            .RemoveSignature()
            .RemoveQrCodeReference();

        // Step 3: Compute the invoice hash (SHA-256 of canonicalized XML)
        var hash = invoiceExtension.ComputeHash();
        var hashBytes = Convert.FromBase64String(hash);

        // Step 4: Create the digital signature
        var digitalSignature = SignData(hashBytes, certificate);

        // Step 5: Extract public key and certificate signature
        var publicKeyBytes = ExtractPublicKey(certificate);
        var certificateSignatureBytes = ExtractCertificateSignature(certificate);

        // Step 6: Build the UBL signature XML
        var signatureBuilder = new SignatureBuilder()
            .SetCertificate(certificate)
            .SetInvoiceDigest(hash)
            .SetSignatureValue(digitalSignature);

        var ublExtensionXml = signatureBuilder.BuildSignatureXml();

        // Step 7: Generate the QR code
        var qrTags = invoiceExtension.GenerateQrTags(
            certificate,
            hash,
            digitalSignature,
            publicKeyBytes,
            certificateSignatureBytes
        );

        var qrCodeGenerator = QrCodeGenerator.CreateFromTags(qrTags);
        var qrCode = qrCodeGenerator.EncodeBase64();

        // Step 8: Insert UBL Extension and QR Code into the ORIGINAL XML
        // Important: We use the original xmlInvoice, not invoiceExtension which was modified for hashing
        var signedXml = InsertSignatureAndQrCode(
            xmlInvoice,
            ublExtensionXml,
            qrCode
        );

        // Step 9: Clean up extra blank lines
        signedXml = Regex.Replace(signedXml, @"^[ \t]*[\r\n]+", "", RegexOptions.Multiline);

        return new SignedInvoiceResult
        {
            SignedXml = signedXml,
            Hash = hash,
            QrCode = qrCode,
            DigitalSignature = digitalSignature,
            Uuid = uuid
        };
    }

    /// <summary>
    /// Signs data using the certificate's private key (ECDSA-SHA256).
    /// </summary>
    /// <param name="data">The data to sign.</param>
    /// <param name="certificate">The certificate with private key.</param>
    /// <returns>Base64-encoded signature.</returns>
    private static string SignData(byte[] data, X509Certificate2 certificate)
    {
        using var ecdsa = certificate.GetECDsaPrivateKey();
        if (ecdsa == null)
            throw new InvalidOperationException("Certificate does not contain an ECDSA private key.");

        var signatureBytes = ecdsa.SignData(data, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(signatureBytes);
    }

    /// <summary>
    /// Extracts the public key bytes from the certificate.
    /// </summary>
    /// <param name="certificate">The X509Certificate2.</param>
    /// <returns>Public key bytes.</returns>
    private static byte[] ExtractPublicKey(X509Certificate2 certificate)
    {
        using var ecdsa = certificate.GetECDsaPublicKey();
        if (ecdsa == null)
            throw new InvalidOperationException("Certificate does not contain an ECDSA public key.");

        // Export the public key in SubjectPublicKeyInfo format
        var publicKeyBytes = ecdsa.ExportSubjectPublicKeyInfo();
        return publicKeyBytes;
    }

    /// <summary>
    /// Extracts the certificate signature bytes from the X509 certificate.
    /// This extracts the actual digital signature value from the certificate structure,
    /// not the certificate hash.
    /// </summary>
    /// <param name="certificate">The X509Certificate2.</param>
    /// <returns>Certificate signature bytes.</returns>
    private static byte[] ExtractCertificateSignature(X509Certificate2 certificate)
    {
        // Use BouncyCastle to parse the certificate and extract the signature value
        var parser = new X509CertificateParser();
        var bcCert = parser.ReadCertificate(certificate.RawData);

        // Get the signature bytes from the certificate
        return bcCert.GetSignature();
    }

    /// <summary>
    /// Inserts the UBL Extension and QR code into the invoice XML.
    /// </summary>
    /// <param name="xmlInvoice">The original invoice XML.</param>
    /// <param name="ublExtension">The UBL extension XML string.</param>
    /// <param name="qrCode">The base64-encoded QR code.</param>
    /// <returns>The signed invoice XML.</returns>
    private static string InsertSignatureAndQrCode(string xmlInvoice, string ublExtension, string qrCode)
    {
        // Create the QR node
        var qrNode = GetQrNode(qrCode);

        // Add ext namespace declaration to Invoice root element if not present
        var signedXml = xmlInvoice;
        const string extNamespaceDecl = "xmlns:ext=\"urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2\"";
        if (!signedXml.Contains("xmlns:ext="))
        {
            // Insert ext namespace after the Invoice opening tag's xmlns declarations
            signedXml = Regex.Replace(
                signedXml,
                @"(<Invoice\s+[^>]*)(>)",
                $"$1 {extNamespaceDecl}$2"
            );
        }

        // Insert UBL Extension before cbc:ProfileID
        // IMPORTANT: Do NOT add extra whitespace between UBLExtensions and ProfileID
        // When ZATCA removes UBLExtensions, no extra whitespace should remain
        signedXml = Regex.Replace(
            signedXml,
            @"(<cbc:ProfileID>)",
            $"<ext:UBLExtensions>{ublExtension}</ext:UBLExtensions>$1"
        );

        // Insert QR code AdditionalDocumentReference before the existing cac:Signature element
        // IMPORTANT: Do NOT add extra whitespace between QR and Signature
        // When ZATCA removes QR, no extra whitespace should remain
        if (signedXml.Contains("<cac:Signature>"))
        {
            signedXml = Regex.Replace(
                signedXml,
                @"(<cac:Signature>)",
                $"{qrNode}$1"
            );
        }
        else
        {
            // If no Signature element exists, insert both QR and Signature before AccountingSupplierParty
            signedXml = signedXml.Replace(
                "<cac:AccountingSupplierParty>",
                $@"{qrNode}
    <cac:Signature>
        <cbc:ID>urn:oasis:names:specification:ubl:signature:Invoice</cbc:ID>
        <cbc:SignatureMethod>urn:oasis:names:specification:ubl:dsig:enveloped:xades</cbc:SignatureMethod>
    </cac:Signature>
    <cac:AccountingSupplierParty>"
            );
        }

        return signedXml;
    }

    /// <summary>
    /// Generates the QR node XML string.
    /// </summary>
    /// <param name="qrCode">The base64-encoded QR code.</param>
    /// <returns>The QR node XML string.</returns>
    private static string GetQrNode(string qrCode)
    {
        return $@"<cac:AdditionalDocumentReference>
        <cbc:ID>QR</cbc:ID>
        <cac:Attachment>
            <cbc:EmbeddedDocumentBinaryObject mimeCode=""text/plain"">{qrCode}</cbc:EmbeddedDocumentBinaryObject>
        </cac:Attachment>
    </cac:AdditionalDocumentReference>";
    }

    /// <summary>
    /// Computes the SHA-256 hash of an invoice XML.
    /// </summary>
    /// <param name="xmlInvoice">The invoice XML string.</param>
    /// <returns>Base64-encoded hash.</returns>
    public static string GetHash(string xmlInvoice)
    {
        if (string.IsNullOrWhiteSpace(xmlInvoice))
            throw new ArgumentNullException(nameof(xmlInvoice));

        var invoiceExtension = InvoiceExtension.FromString(xmlInvoice);
        invoiceExtension
            .RemoveUblExtensions()
            .RemoveSignature()
            .RemoveQrCodeReference();

        return invoiceExtension.ComputeHash();
    }

    /// <summary>
    /// Generates a QR code for an already-signed invoice.
    /// </summary>
    /// <param name="signedXmlInvoice">The signed invoice XML.</param>
    /// <param name="certificate">The certificate used for signing.</param>
    /// <param name="hash">The invoice hash.</param>
    /// <param name="digitalSignature">The digital signature.</param>
    /// <returns>Base64-encoded QR code.</returns>
    public static string GetQrCode(
        string signedXmlInvoice,
        X509Certificate2 certificate,
        string hash,
        string digitalSignature)
    {
        if (string.IsNullOrWhiteSpace(signedXmlInvoice))
            throw new ArgumentNullException(nameof(signedXmlInvoice));
        if (certificate == null)
            throw new ArgumentNullException(nameof(certificate));

        var invoiceExtension = InvoiceExtension.FromString(signedXmlInvoice);
        var publicKeyBytes = ExtractPublicKey(certificate);
        var certificateSignatureBytes = ExtractCertificateSignature(certificate);

        var qrTags = invoiceExtension.GenerateQrTags(
            certificate,
            hash,
            digitalSignature,
            publicKeyBytes,
            certificateSignatureBytes
        );

        var qrCodeGenerator = QrCodeGenerator.CreateFromTags(qrTags);
        return qrCodeGenerator.EncodeBase64();
    }
}
