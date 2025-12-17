using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

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

        // Step 8: Insert UBL Extension and QR Code into the original XML
        var signedXml = InsertSignatureAndQrCode(
            invoiceExtension.GetDocument().ToString(),
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
            DigitalSignature = digitalSignature
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
    /// Extracts the certificate signature bytes.
    /// </summary>
    /// <param name="certificate">The X509Certificate2.</param>
    /// <returns>Certificate signature bytes.</returns>
    private static byte[] ExtractCertificateSignature(X509Certificate2 certificate)
    {
        // The certificate signature is available via GetCertHash() or parsing the certificate
        // For ZATCA requirements, we need the signature bytes from the certificate
        return certificate.GetCertHash();
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

        // Insert UBL Extension before cbc:ProfileID
        var signedXml = xmlInvoice.Replace(
            "<cbc:ProfileID>",
            $"<ext:UBLExtensions>{ublExtension}</ext:UBLExtensions>\n    <cbc:ProfileID>"
        );

        // Insert QR code and signature before cac:AccountingSupplierParty
        signedXml = signedXml.Replace(
            "<cac:AccountingSupplierParty>",
            $"{qrNode}\n    <cac:AccountingSupplierParty>"
        );

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
    </cac:AdditionalDocumentReference>
    <cac:Signature>
        <cbc:ID>urn:oasis:names:specification:ubl:signature:Invoice</cbc:ID>
        <cbc:SignatureMethod>urn:oasis:names:specification:ubl:dsig:enveloped:xades</cbc:SignatureMethod>
    </cac:Signature>";
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
