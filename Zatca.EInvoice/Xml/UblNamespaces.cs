using System.Xml.Linq;

namespace Zatca.EInvoice.Xml
{
    /// <summary>
    /// Contains UBL XML namespace definitions.
    /// </summary>
    public static class UblNamespaces
    {
        /// <summary>
        /// UBL Invoice namespace.
        /// </summary>
        public static readonly XNamespace Invoice = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";

        /// <summary>
        /// Common Aggregate Components namespace.
        /// </summary>
        public static readonly XNamespace Cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

        /// <summary>
        /// Common Basic Components namespace.
        /// </summary>
        public static readonly XNamespace Cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

        /// <summary>
        /// Common Extension Components namespace.
        /// </summary>
        public static readonly XNamespace Ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";

        /// <summary>
        /// Common Signature Components namespace.
        /// </summary>
        public static readonly XNamespace Sig = "urn:oasis:names:specification:ubl:schema:xsd:CommonSignatureComponents-2";

        /// <summary>
        /// Signature Aggregate Components namespace.
        /// </summary>
        public static readonly XNamespace Sac = "urn:oasis:names:specification:ubl:schema:xsd:SignatureAggregateComponents-2";

        /// <summary>
        /// Signature Basic Components namespace.
        /// </summary>
        public static readonly XNamespace Sbc = "urn:oasis:names:specification:ubl:schema:xsd:SignatureBasicComponents-2";

#pragma warning disable S5332 // Using http protocol - these are W3C XML namespace URIs that must use http://
        /// <summary>
        /// XML Digital Signature namespace.
        /// </summary>
        public static readonly XNamespace Ds = "http://www.w3.org/2000/09/xmldsig#";

        /// <summary>
        /// XAdES namespace.
        /// </summary>
        public static readonly XNamespace Xades = "http://uri.etsi.org/01903/v1.3.2#";
#pragma warning restore S5332
    }
}
