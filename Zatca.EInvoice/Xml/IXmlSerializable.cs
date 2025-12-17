using System.Xml.Linq;

namespace Zatca.EInvoice.Xml
{
    /// <summary>
    /// Interface for objects that can be serialized to XML.
    /// </summary>
    public interface IXmlSerializable
    {
        /// <summary>
        /// Converts the object to an XML element.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the object.</returns>
        XElement ToXml();
    }
}
