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
    private XDocument _document;

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
    /// <returns>Base64-encoded hash string.</returns>
    public string ComputeHash()
    {
        var canonicalXml = GetCanonicalXml();
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalXml));
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
            // Use StringWriter to properly include the declaration
            using var writer = new System.IO.StringWriter();
            _document.Save(writer, SaveOptions.DisableFormatting);
            return writer.ToString();
        }
        else
        {
            return _document.Root?.ToString(SaveOptions.DisableFormatting) ?? string.Empty;
        }
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
    /// Gets the invoice issue date.
    /// </summary>
    public string GetIssueDate()
    {
        return GetElementValue("cbc:IssueDate");
    }

    /// <summary>
    /// Gets the invoice issue time.
    /// </summary>
    public string GetIssueTime()
    {
        var time = GetElementValue("cbc:IssueTime");
        return time.EndsWith("Z") ? time : time + "Z";
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
    private (string prefix, string localName) SplitQualifiedName(string qualifiedName)
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
    private XNamespace GetNamespace(string prefix, XElement context)
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
