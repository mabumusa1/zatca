using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Zatca.EInvoice.Signing;

/// <summary>
/// Builds UBL signature XML structure with XAdES signature for ZATCA e-invoices.
/// </summary>
public class SignatureBuilder
{
    private const string SacNs = "urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2";
    private const string SbcNs = "urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2";
    private const string SigNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2";
    private const string DsNs = "http://www.w3.org/2000/09/xmldsig#";
    private const string XadesNs = "http://uri.etsi.org/01903/v1.3.2#";
    private const string Algorithm = "Algorithm";
    private const string Transform = "Transform";

    private X509Certificate2? _certificate;
    private string _invoiceDigest = string.Empty;
    private string _signatureValue = string.Empty;

    /// <summary>
    /// Sets the certificate to use for building the signature.
    /// </summary>
    /// <param name="certificate">The X509Certificate2 instance.</param>
    /// <returns>The current instance for method chaining.</returns>
    public SignatureBuilder SetCertificate(X509Certificate2 certificate)
    {
        _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        return this;
    }

    /// <summary>
    /// Sets the invoice digest (hash).
    /// </summary>
    /// <param name="invoiceDigest">The base64-encoded SHA-256 hash of the invoice.</param>
    /// <returns>The current instance for method chaining.</returns>
    public SignatureBuilder SetInvoiceDigest(string invoiceDigest)
    {
        _invoiceDigest = invoiceDigest ?? throw new ArgumentNullException(nameof(invoiceDigest));
        return this;
    }

    /// <summary>
    /// Sets the signature value.
    /// </summary>
    /// <param name="signatureValue">The base64-encoded signature value.</param>
    /// <returns>The current instance for method chaining.</returns>
    public SignatureBuilder SetSignatureValue(string signatureValue)
    {
        _signatureValue = signatureValue ?? throw new ArgumentNullException(nameof(signatureValue));
        return this;
    }

    /// <summary>
    /// Builds and returns the UBL signature XML as a formatted string.
    /// </summary>
    /// <returns>The formatted UBL signature XML.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required properties are not set.</exception>
    public string BuildSignatureXml()
    {
        if (_certificate == null)
            throw new InvalidOperationException("Certificate must be set before building signature XML.");
        if (string.IsNullOrEmpty(_invoiceDigest))
            throw new InvalidOperationException("Invoice digest must be set before building signature XML.");
        if (string.IsNullOrEmpty(_signatureValue))
            throw new InvalidOperationException("Signature value must be set before building signature XML.");

        var signingTime = DateTime.UtcNow.ToString("yyyy-MM-dd") + "T" + DateTime.UtcNow.ToString("HH:mm:ss");

        // Create the signed properties XML
        var signedPropertiesXml = CreateSignedPropertiesXml(signingTime);

        // Build the UBLExtension structure
        var doc = new XDocument();
        var extNs = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
        var cbcNs = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

        var extension = new XElement(extNs + "UBLExtension",
            new XElement(extNs + "ExtensionURI", "urn:oasis:names:specification:ubl:dsig:enveloped:xades"),
            new XElement(extNs + "ExtensionContent",
                CreateUblDocumentSignatures(signingTime, signedPropertiesXml, cbcNs)
            )
        );

        doc.Add(extension);

        // Convert to string and format
        var xml = doc.ToString(SaveOptions.None);

        // Remove XML declaration
        if (xml.StartsWith("<?xml"))
        {
            var endOfDeclaration = xml.IndexOf("?>") + 2;
            xml = xml.Substring(endOfDeclaration).TrimStart();
        }

        // Ensure proper indentation (double the spacing)
        xml = System.Text.RegularExpressions.Regex.Replace(xml, @"^([ ]+)(?=<)", "$0$0", System.Text.RegularExpressions.RegexOptions.Multiline);

        return xml;
    }

    /// <summary>
    /// Creates the UBLDocumentSignatures element.
    /// </summary>
    private XElement CreateUblDocumentSignatures(string signingTime, string signedPropertiesXml, XNamespace cbcNs)
    {
        var sigNs2 = XNamespace.Get(SigNs);
        var sacNs2 = XNamespace.Get(SacNs);
        var sbcNs2 = XNamespace.Get(SbcNs);

        var ublDocSigs = new XElement(sigNs2 + "UBLDocumentSignatures",
            new XAttribute(XNamespace.Xmlns + "sig", SigNs),
            new XAttribute(XNamespace.Xmlns + "sac", SacNs),
            new XAttribute(XNamespace.Xmlns + "sbc", SbcNs)
        );

        var signatureInfo = new XElement(sacNs2 + "SignatureInformation",
            new XElement(cbcNs + "ID", "urn:oasis:names:specification:ubl:signature:1"),
            new XElement(sbcNs2 + "ReferencedSignatureID", "urn:oasis:names:specification:ubl:signature:Invoice"),
            CreateDsSignature(signingTime, signedPropertiesXml)
        );

        ublDocSigs.Add(signatureInfo);
        return ublDocSigs;
    }

    /// <summary>
    /// Creates the ds:Signature element.
    /// </summary>
    private XElement CreateDsSignature(string signingTime, string signedPropertiesXml)
    {
        var dsNs2 = XNamespace.Get(DsNs);

        var signature = new XElement(dsNs2 + "Signature",
            new XAttribute(XNamespace.Xmlns + "ds", DsNs),
            new XAttribute("Id", "signature"),
            CreateSignedInfo(signedPropertiesXml),
            new XElement(dsNs2 + "SignatureValue", _signatureValue),
            CreateKeyInfo(),
            CreateDsObject(signingTime)
        );

        return signature;
    }

    /// <summary>
    /// Creates the SignedInfo element.
    /// </summary>
    private XElement CreateSignedInfo(string signedPropertiesXml)
    {
        var dsNs2 = XNamespace.Get(DsNs);

        var signedInfo = new XElement(dsNs2 + "SignedInfo",
            new XElement(dsNs2 + "CanonicalizationMethod",
                new XAttribute(Algorithm, "http://www.w3.org/2006/12/xml-c14n11")),
            new XElement(dsNs2 + "SignatureMethod",
                new XAttribute(Algorithm, "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256")),
            CreateInvoiceReference(),
            CreateSignedPropertiesReference(signedPropertiesXml)
        );

        return signedInfo;
    }

    /// <summary>
    /// Creates the reference element for the invoice signed data.
    /// </summary>
    private XElement CreateInvoiceReference()
    {
        var dsNs2 = XNamespace.Get(DsNs);

        var reference = new XElement(dsNs2 + "Reference",
            new XAttribute("Id", "invoiceSignedData"),
            new XAttribute("URI", ""),
            CreateTransforms(),
            new XElement(dsNs2 + "DigestMethod",
                new XAttribute(Algorithm, "http://www.w3.org/2001/04/xmlenc#sha256")),
            new XElement(dsNs2 + "DigestValue", _invoiceDigest)
        );

        return reference;
    }

    /// <summary>
    /// Creates the Transforms element with XPath filters.
    /// </summary>
    private static XElement CreateTransforms()
    {
        var dsNs2 = XNamespace.Get(DsNs);

        var transforms = new XElement(dsNs2 + "Transforms",
            // Exclude UBLExtensions
            new XElement(dsNs2 + Transform,
                new XAttribute(Algorithm, "http://www.w3.org/TR/1999/REC-xpath-19991116"),
                new XElement(dsNs2 + "XPath", "not(//ancestor-or-self::ext:UBLExtensions)")
            ),
            // Exclude cac:Signature
            new XElement(dsNs2 + Transform,
                new XAttribute(Algorithm, "http://www.w3.org/TR/1999/REC-xpath-19991116"),
                new XElement(dsNs2 + "XPath", "not(//ancestor-or-self::cac:Signature)")
            ),
            // Exclude QR AdditionalDocumentReference
            new XElement(dsNs2 + Transform,
                new XAttribute(Algorithm, "http://www.w3.org/TR/1999/REC-xpath-19991116"),
                new XElement(dsNs2 + "XPath", "not(//ancestor-or-self::cac:AdditionalDocumentReference[cbc:ID='QR'])")
            ),
            // Canonicalization
            new XElement(dsNs2 + Transform,
                new XAttribute(Algorithm, "http://www.w3.org/2006/12/xml-c14n11")
            )
        );

        return transforms;
    }

    /// <summary>
    /// Creates the reference element for signed properties.
    /// </summary>
    private static XElement CreateSignedPropertiesReference(string signedPropertiesXml)
    {
        var dsNs2 = XNamespace.Get(DsNs);

        // Compute hash of signed properties in ZATCA format: base64(hex(sha256(...)))
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(signedPropertiesXml));
        var hexHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        var digestValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(hexHash));

        var reference = new XElement(dsNs2 + "Reference",
            new XAttribute("Type", "http://www.w3.org/2000/09/xmldsig#SignatureProperties"),
            new XAttribute("URI", "#xadesSignedProperties"),
            new XElement(dsNs2 + "DigestMethod",
                new XAttribute(Algorithm, "http://www.w3.org/2001/04/xmlenc#sha256")),
            new XElement(dsNs2 + "DigestValue", digestValue)
        );

        return reference;
    }

    /// <summary>
    /// Creates the KeyInfo element.
    /// </summary>
    private XElement CreateKeyInfo()
    {
        var dsNs2 = XNamespace.Get(DsNs);

        // Get the base64-encoded certificate without header/footer
        var certBytes = _certificate!.Export(X509ContentType.Cert);
        var certBase64 = Convert.ToBase64String(certBytes);

        var keyInfo = new XElement(dsNs2 + "KeyInfo",
            new XElement(dsNs2 + "X509Data",
                new XElement(dsNs2 + "X509Certificate", certBase64)
            )
        );

        return keyInfo;
    }

    /// <summary>
    /// Creates the ds:Object element with XAdES qualifying properties.
    /// </summary>
    private XElement CreateDsObject(string signingTime)
    {
        var dsNs2 = XNamespace.Get(DsNs);
        var xadesNs2 = XNamespace.Get(XadesNs);

        var dsObject = new XElement(dsNs2 + "Object",
            new XElement(xadesNs2 + "QualifyingProperties",
                new XAttribute(XNamespace.Xmlns + "xades", XadesNs),
                new XAttribute("Target", "signature"),
                CreateSignedProperties(signingTime, xadesNs2)
            )
        );

        return dsObject;
    }

    /// <summary>
    /// Creates the xades:SignedProperties element.
    /// </summary>
    private XElement CreateSignedProperties(string signingTime, XNamespace xadesNs2)
    {
        var dsNs2 = XNamespace.Get(DsNs);

        // Compute certificate hash in ZATCA format: base64(hex(sha256(DER)))
        var certHash = ComputeCertificateHash(_certificate!);

        // Get issuer and serial number (convert hex to decimal for XML)
        var issuer = _certificate!.IssuerName.Name;
        var serialNumber = GetSerialNumberAsDecimal(_certificate);

        var signedProps = new XElement(xadesNs2 + "SignedProperties",
            new XAttribute(XNamespace.Xmlns + "xades", XadesNs),
            new XAttribute("Id", "xadesSignedProperties"),
            new XElement(xadesNs2 + "SignedSignatureProperties",
                new XElement(xadesNs2 + "SigningTime", signingTime),
                new XElement(xadesNs2 + "SigningCertificate",
                    new XElement(xadesNs2 + "Cert",
                        new XElement(xadesNs2 + "CertDigest",
                            new XElement(dsNs2 + "DigestMethod",
                                new XAttribute(Algorithm, "http://www.w3.org/2001/04/xmlenc#sha256")),
                            new XElement(dsNs2 + "DigestValue", certHash)
                        ),
                        new XElement(xadesNs2 + "IssuerSerial",
                            new XElement(dsNs2 + "X509IssuerName", issuer),
                            new XElement(dsNs2 + "X509SerialNumber", serialNumber)
                        )
                    )
                )
            )
        );

        return signedProps;
    }

    /// <summary>
    /// Creates the signed properties XML string for hash computation.
    /// The spacing must be exact to match the expected hash.
    /// </summary>
    private string CreateSignedPropertiesXml(string signingTime)
    {

        // Compute certificate hash in ZATCA format: base64(hex(sha256(DER)))
        var certHash = ComputeCertificateHash(_certificate!);

        // Get issuer and serial number (convert hex to decimal for XML)
        var issuer = _certificate!.IssuerName.Name;
        var serialNumber = GetSerialNumberAsDecimal(_certificate);

        // Build the XML with exact spacing as per ZATCA requirements
        var template = @"<xades:SignedProperties xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"" Id=""xadesSignedProperties"">
                                <xades:SignedSignatureProperties>
                                    <xades:SigningTime>SIGNING_TIME_PLACEHOLDER</xades:SigningTime>
                                    <xades:SigningCertificate>
                                        <xades:Cert>
                                            <xades:CertDigest>
                                                <ds:DigestMethod xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"" Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256""/>
                                                <ds:DigestValue xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">DIGEST_PLACEHOLDER</ds:DigestValue>
                                            </xades:CertDigest>
                                            <xades:IssuerSerial>
                                                <ds:X509IssuerName xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">ISSUER_PLACEHOLDER</ds:X509IssuerName>
                                                <ds:X509SerialNumber xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">SERIAL_PLACEHOLDER</ds:X509SerialNumber>
                                            </xades:IssuerSerial>
                                        </xades:Cert>
                                    </xades:SigningCertificate>
                                </xades:SignedSignatureProperties>
                            </xades:SignedProperties>";

        return template
            .Replace("SIGNING_TIME_PLACEHOLDER", signingTime)
            .Replace("DIGEST_PLACEHOLDER", certHash)
            .Replace("ISSUER_PLACEHOLDER", issuer)
            .Replace("SERIAL_PLACEHOLDER", serialNumber);
    }

    /// <summary>
    /// Gets the certificate serial number as a decimal string.
    /// X509SerialNumber in XML must be a decimal integer, not hex.
    /// </summary>
    private static string GetSerialNumberAsDecimal(X509Certificate2 certificate)
    {
        var hexSerial = certificate.GetSerialNumberString();
        var serialBigInt = BigInteger.Parse(hexSerial, NumberStyles.HexNumber);
        return serialBigInt.ToString();
    }

    /// <summary>
    /// Computes the certificate hash in ZATCA format.
    /// ZATCA expects: base64(hex(sha256(rawCertificate)))
    /// where rawCertificate is the base64 content of the certificate (DER bytes).
    /// </summary>
    private static string ComputeCertificateHash(X509Certificate2 certificate)
    {
        // ZATCA format: base64(hex(sha256(DER)))
        var hashBytes = SHA256.HashData(certificate.RawData);
        // Convert hash to lowercase hex string, then base64 encode
        var hexHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(hexHash));
    }
}
