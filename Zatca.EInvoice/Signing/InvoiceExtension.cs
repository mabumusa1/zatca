using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Zatca.EInvoice.Tags;

namespace Zatca.EInvoice.Signing;

/// <summary>
/// Provides XML manipulation utilities for ZATCA e-invoice processing.
/// Handles operations like removing/adding signature elements, QR codes, and extracting invoice data.
/// </summary>
public class InvoiceExtension
{
    private readonly XDocument _document;

    /// <summary>
    /// Initializes a new instance of the InvoiceExtension class.
    /// </summary>
    /// <param name="document">The XDocument to manipulate.</param>
    private InvoiceExtension(XDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    /// <summary>
    /// Creates an InvoiceExtension instance from an XML string.
    /// </summary>
    /// <param name="xmlString">The XML string to parse.</param>
    /// <returns>A new InvoiceExtension instance.</returns>
    /// <exception cref="ArgumentException">Thrown when XML cannot be parsed.</exception>
    public static InvoiceExtension FromString(string xmlString)
    {
        if (string.IsNullOrWhiteSpace(xmlString))
        {
            throw new ArgumentException("XML string cannot be null or empty.", nameof(xmlString));
        }

        try
        {
            var document = XDocument.Parse(xmlString, LoadOptions.PreserveWhitespace);
            return new InvoiceExtension(document);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Failed to parse XML string.", nameof(xmlString), ex);
        }
    }

    /// <summary>
    /// Removes UBLExtensions elements from the XML.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public InvoiceExtension RemoveUblExtensions()
    {
        RemoveElementsByName("UBLExtensions", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
        return this;
    }

    /// <summary>
    /// Removes cac:Signature elements from the XML.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public InvoiceExtension RemoveSignature()
    {
        RemoveElementsByName("Signature", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        return this;
    }

    /// <summary>
    /// Removes the AdditionalDocumentReference element that contains the QR code.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public InvoiceExtension RemoveQrCodeReference()
    {
        var ns = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
        var cbcNs = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

        var qrElements = _document.Descendants(ns + "AdditionalDocumentReference")
            .Where(e => e.Elements(cbcNs + "ID").Any(id => id.Value == "QR"))
            .ToList();

        foreach (var element in qrElements)
        {
            element.Remove();
        }

        return this;
    }

    /// <summary>
    /// Ensures the ext namespace declaration exists on the root element.
    /// This must be called before hashing so that the C14N form includes it,
    /// matching what ZATCA computes after stripping the signed XML.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public InvoiceExtension EnsureExtNamespace()
    {
        var root = _document.Root;
        if (root == null) return this;

        XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        // Add xmlns:ext if not already declared on the root
        if (root.Attribute(XNamespace.Xmlns + "ext") == null)
        {
            root.Add(new XAttribute(XNamespace.Xmlns + "ext", ext));
        }

        return this;
    }

    /// <summary>
    /// Ensures the IssueTime element has a UTC 'Z' suffix.
    /// ZATCA KSA-25 requires the QR timestamp to match the invoice IssueTime exactly.
    /// If the invoice IssueTime lacks the 'Z' suffix, this method adds it.
    /// This must be called before hashing so the hash includes the correct time format.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public InvoiceExtension EnsureIssueTimeHasUtcSuffix()
    {
        var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        var issueTimeElement = _document.Descendants(cbc + "IssueTime").FirstOrDefault();

        if (issueTimeElement != null && !string.IsNullOrEmpty(issueTimeElement.Value))
        {
            var time = issueTimeElement.Value;
            if (!time.EndsWith('Z'))
            {
                issueTimeElement.Value = time + "Z";
            }
        }

        return this;
    }

    /// <summary>
    /// Inserts a UBLExtensions element (parsed from XML string) as the first child of the root.
    /// The element is inserted before cbc:ProfileID so ZATCA sees it in the expected position.
    /// </summary>
    /// <param name="ublExtensionInnerXml">The XML string for the ext:UBLExtension content.</param>
    /// <returns>The current instance for method chaining.</returns>
    public InvoiceExtension InsertUblExtension(string ublExtensionInnerXml)
    {
        var root = _document.Root;
        if (root == null) return this;

        XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
        XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

        // Parse the extension XML fragment
        var extensionContent = XElement.Parse(ublExtensionInnerXml);

        // Build ext:UBLExtensions > ext:UBLExtension
        // If the parsed content is already an ext:UBLExtension, wrap in UBLExtensions
        var ublExtensions = new XElement(ext + "UBLExtensions", extensionContent);

        // Insert before cbc:ProfileID (first child in the expected position)
        var profileId = root.Element(cbc + "ProfileID");
        if (profileId != null)
        {
            profileId.AddBeforeSelf(ublExtensions);
        }
        else
        {
            root.AddFirst(ublExtensions);
        }

        return this;
    }

    /// <summary>
    /// Inserts a QR code AdditionalDocumentReference element before cac:Signature.
    /// If cac:Signature doesn't exist, inserts before cac:AccountingSupplierParty.
    /// </summary>
    /// <param name="qrCode">The Base64-encoded QR code.</param>
    /// <returns>The current instance for method chaining.</returns>
    public InvoiceExtension InsertQrCode(string qrCode)
    {
        var root = _document.Root;
        if (root == null) return this;

        XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

        var qrElement = new XElement(cac + "AdditionalDocumentReference",
            new XElement(cbc + "ID", "QR"),
            new XElement(cac + "Attachment",
                new XElement(cbc + "EmbeddedDocumentBinaryObject",
                    new XAttribute("mimeCode", "text/plain"),
                    qrCode)));

        // Insert before cac:Signature or cac:AccountingSupplierParty
        var signature = root.Element(cac + "Signature");
        if (signature != null)
        {
            signature.AddBeforeSelf(qrElement);
        }
        else
        {
            var supplier = root.Element(cac + "AccountingSupplierParty");
            if (supplier != null)
            {
                supplier.AddBeforeSelf(qrElement);
            }
        }

        return this;
    }

    /// <summary>
    /// Inserts or re-inserts the cac:Signature element before cac:AccountingSupplierParty.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public InvoiceExtension InsertSignatureElement()
    {
        var root = _document.Root;
        if (root == null) return this;

        XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

        // Only add if not already present
        if (root.Element(cac + "Signature") != null) return this;

        var signatureElement = new XElement(cac + "Signature",
            new XElement(cbc + "ID", "urn:oasis:names:specification:ubl:signature:Invoice"),
            new XElement(cbc + "SignatureMethod", "urn:oasis:names:specification:ubl:dsig:enveloped:xades"));

        var supplier = root.Element(cac + "AccountingSupplierParty");
        if (supplier != null)
        {
            supplier.AddBeforeSelf(signatureElement);
        }

        return this;
    }

    /// <summary>
    /// Removes elements by local name and namespace.
    /// </summary>
    private void RemoveElementsByName(string localName, string namespaceUri)
    {
        var ns = XNamespace.Get(namespaceUri);
        var elements = _document.Descendants(ns + localName).ToList();

        foreach (var element in elements)
        {
            element.Remove();
        }
    }

    /// <summary>
    /// Computes the SHA-256 hash of the canonicalized XML (C14N).
    /// </summary>
    /// <returns>Base64-encoded SHA-256 hash string.</returns>
    public string ComputeHash()
    {
        var canonicalXml = GetCanonicalXml();
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalXml));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Gets the canonical form (C14N) of the XML.
    /// </summary>
    /// <returns>Canonicalized XML string.</returns>
    public string GetCanonicalXml()
    {
        var xmlDoc = new XmlDocument { PreserveWhitespace = true };
        using (var reader = _document.CreateReader())
        {
            xmlDoc.Load(reader);
        }

        var transform = new XmlDsigC14NTransform(false);
        transform.LoadInput(xmlDoc);
        var stream = (System.IO.Stream)transform.GetOutput(typeof(System.IO.Stream));

        using var reader2 = new System.IO.StreamReader(stream);
        return reader2.ReadToEnd();
    }

    /// <summary>
    /// Converts the XML document to a string.
    /// </summary>
    /// <param name="includeDeclaration">Whether to include the XML declaration.</param>
    /// <returns>XML string.</returns>
    public string ToXmlString(bool includeDeclaration = true)
    {
        if (includeDeclaration)
        {
            // Ensure we have a declaration
            if (_document.Declaration == null)
            {
                _document.Declaration = new XDeclaration("1.0", "UTF-8", null);
            }
            // Use Utf8StringWriter so XDocument.Save() emits encoding="utf-8"
            // instead of StringWriter's default encoding="utf-16".
            // The API client Base64-encodes the XML as UTF-8 bytes, so the
            // declaration must match to avoid ZATCA "Content is not allowed in prolog".
            using var writer = new Utf8StringWriter();
            _document.Save(writer, SaveOptions.DisableFormatting);
            return writer.ToString();
        }
        else
        {
            return _document.Root?.ToString(SaveOptions.DisableFormatting) ?? string.Empty;
        }
    }

    /// <summary>
    /// StringWriter subclass that reports UTF-8 encoding so that
    /// XDocument.Save() writes encoding="utf-8" in the XML declaration.
    /// </summary>
    private sealed class Utf8StringWriter : System.IO.StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    /// <summary>
    /// Gets the value of an element by XPath-like path.
    /// </summary>
    /// <param name="elementPath">Namespace-qualified element names separated by '/'.</param>
    /// <returns>Element value or empty string if not found.</returns>
    public string GetElementValue(string elementPath)
    {
        var parts = elementPath.Split('/');
        XElement? current = _document.Root;

        foreach (var part in parts)
        {
            if (current == null) return string.Empty;

            var (prefix, localName) = SplitQualifiedName(part);
            var ns = GetNamespace(prefix, current);

            current = current.Elements(ns + localName).FirstOrDefault();
        }

        return current?.Value ?? string.Empty;
    }

    /// <summary>
    /// Gets the seller name from the invoice.
    /// </summary>
    public string GetSellerName()
    {
        return GetElementValue("cac:AccountingSupplierParty/cac:Party/cac:PartyLegalEntity/cbc:RegistrationName");
    }

    /// <summary>
    /// Gets the tax number from the invoice.
    /// </summary>
    public string GetTaxNumber()
    {
        return GetElementValue("cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID");
    }

    /// <summary>
    /// Gets the invoice UUID.
    /// </summary>
    public string GetUuid()
    {
        return GetElementValue("cbc:UUID");
    }

    /// <summary>
    /// Gets the invoice issue date.
    /// </summary>
    public string GetIssueDate()
    {
        return GetElementValue("cbc:IssueDate");
    }

    /// <summary>
    /// Gets the invoice issue time. Ensures UTC 'Z' suffix is present.
    /// </summary>
    public string GetIssueTime()
    {
        var time = GetElementValue("cbc:IssueTime");
        if (!string.IsNullOrEmpty(time) && !time.EndsWith('Z'))
        {
            time += "Z";
        }
        return time;
    }

    /// <summary>
    /// Gets the invoice total amount including tax.
    /// </summary>
    public string GetTaxInclusiveAmount()
    {
        return GetElementValue("cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount");
    }

    /// <summary>
    /// Gets the total tax amount.
    /// </summary>
    public string GetTaxAmount()
    {
        return GetElementValue("cac:TaxTotal/cbc:TaxAmount");
    }

    /// <summary>
    /// Gets the invoice type code name attribute.
    /// </summary>
    public string GetInvoiceTypeCodeName()
    {
        var ns = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
        var element = _document.Descendants(ns + "InvoiceTypeCode").FirstOrDefault();
        return element?.Attribute("name")?.Value ?? string.Empty;
    }

    /// <summary>
    /// Checks if this is a simplified invoice (type code name starts with "02").
    /// </summary>
    public bool IsSimplifiedInvoice()
    {
        var typeCodeName = GetInvoiceTypeCodeName();
        return typeCodeName.StartsWith("02");
    }

    /// <summary>
    /// Generates QR tags array for the invoice.
    /// </summary>
    /// <param name="certificate">The X509 certificate used for signing.</param>
    /// <param name="invoiceHash">The base64-encoded invoice hash.</param>
    /// <param name="digitalSignature">The base64-encoded digital signature.</param>
    /// <param name="publicKeyBytes">The public key bytes from the certificate.</param>
    /// <param name="certificateSignatureBytes">The certificate signature bytes (for simplified invoices).</param>
    /// <returns>Array of Tag objects.</returns>
    public Tag[] GenerateQrTags(
        X509Certificate2 certificate,
        string invoiceHash,
        string digitalSignature,
        byte[] publicKeyBytes,
        byte[]? certificateSignatureBytes = null)
    {
        var issueDate = GetIssueDate();
        var issueTime = GetIssueTime();
        var dateTime = $"{issueDate}T{issueTime}";

        var tags = new List<Tag>
        {
            new SellerTag(GetSellerName()),
            new TaxNumberTag(GetTaxNumber()),
            new InvoiceDateTag(dateTime),
            new InvoiceTotalTag(GetTaxInclusiveAmount()),
            new TaxAmountTag(GetTaxAmount()),
            new InvoiceHashTag(invoiceHash),
            new DigitalSignatureTag(digitalSignature),
            new PublicKeyTag(publicKeyBytes)
        };

        // For simplified invoices, add the certificate signature tag
        if (IsSimplifiedInvoice() && certificateSignatureBytes != null)
        {
            tags.Add(new CertificateSignatureTag(certificateSignatureBytes));
        }

        return tags.ToArray();
    }

    /// <summary>
    /// Splits a qualified name (prefix:localName) into its components.
    /// </summary>
    private static (string prefix, string localName) SplitQualifiedName(string qualifiedName)
    {
        var parts = qualifiedName.Split(':');
        if (parts.Length == 2)
        {
            return (parts[0], parts[1]);
        }
        return (string.Empty, qualifiedName);
    }

    /// <summary>
    /// Gets the namespace URI for a given prefix.
    /// </summary>
    private static XNamespace GetNamespace(string prefix, XElement context)
    {
        return prefix switch
        {
            "cac" => XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
            "cbc" => XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"),
            "ext" => XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"),
            _ => context.GetDefaultNamespace()
        };
    }

    /// <summary>
    /// Gets the underlying XDocument.
    /// </summary>
    public XDocument GetDocument()
    {
        return _document;
    }
}
