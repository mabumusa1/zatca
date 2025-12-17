using System.Text;
using Zatca.EInvoice.Tags;

namespace Zatca.EInvoice.Signing;

/// <summary>
/// Generates QR codes for ZATCA e-invoices using TLV (Tag-Length-Value) encoding.
/// </summary>
public class QrCodeGenerator
{
    private readonly List<Tag> _tags;

    /// <summary>
    /// Initializes a new instance of the QrCodeGenerator class.
    /// </summary>
    /// <param name="tags">Array of Tag instances to encode.</param>
    /// <exception cref="ArgumentException">Thrown when no valid Tag instances are provided.</exception>
    private QrCodeGenerator(IEnumerable<Tag> tags)
    {
        _tags = tags.Where(t => t != null).ToList();

        if (_tags.Count == 0)
        {
            throw new ArgumentException("No valid Tag instances found.", nameof(tags));
        }
    }

    /// <summary>
    /// Creates a QrCodeGenerator instance from an array of Tag objects.
    /// </summary>
    /// <param name="tags">Array of Tag objects.</param>
    /// <returns>A new QrCodeGenerator instance.</returns>
    public static QrCodeGenerator CreateFromTags(params Tag[] tags)
    {
        return new QrCodeGenerator(tags);
    }

    /// <summary>
    /// Creates a QrCodeGenerator instance from a list of Tag objects.
    /// </summary>
    /// <param name="tags">List of Tag objects.</param>
    /// <returns>A new QrCodeGenerator instance.</returns>
    public static QrCodeGenerator CreateFromTags(IEnumerable<Tag> tags)
    {
        return new QrCodeGenerator(tags);
    }

    /// <summary>
    /// Encodes the tags into a TLV (Tag-Length-Value) formatted byte array.
    /// </summary>
    /// <returns>TLV encoded byte array.</returns>
    public byte[] EncodeTlv()
    {
        var tlvBytes = new List<byte>();

        foreach (var tag in _tags)
        {
            tlvBytes.AddRange(tag.ToBytes());
        }

        return tlvBytes.ToArray();
    }

    /// <summary>
    /// Encodes the TLV data into a base64 string.
    /// </summary>
    /// <returns>Base64 encoded TLV string suitable for QR code generation.</returns>
    public string EncodeBase64()
    {
        var tlvBytes = EncodeTlv();
        return Convert.ToBase64String(tlvBytes);
    }
}
