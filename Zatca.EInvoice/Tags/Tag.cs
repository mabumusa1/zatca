using System.Text;

namespace Zatca.EInvoice.Tags;

/// <summary>
/// Abstract base class for TLV (Tag-Length-Value) encoded tags used in QR code generation.
/// </summary>
public abstract class Tag
{
    /// <summary>
    /// Gets the tag number (1 byte).
    /// </summary>
    public byte TagNumber { get; }

    /// <summary>
    /// Gets the tag value.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Initializes a new instance of the Tag class.
    /// </summary>
    /// <param name="tagNumber">The tag number (1-255).</param>
    /// <param name="value">The tag value.</param>
    protected Tag(byte tagNumber, object value)
    {
        TagNumber = tagNumber;
        Value = value ?? string.Empty;
    }

    /// <summary>
    /// Gets the byte representation of the value.
    /// </summary>
    /// <returns>Byte array of the value.</returns>
    protected virtual byte[] GetValueBytes()
    {
        if (Value is byte[] bytes)
        {
            return bytes;
        }

        if (Value is string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        return Encoding.UTF8.GetBytes(Value.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Encodes the tag in TLV (Tag-Length-Value) format.
    /// Format: [Tag:1byte][Length:1byte][Value:N bytes]
    /// </summary>
    /// <returns>TLV encoded byte array.</returns>
    public byte[] ToBytes()
    {
        var valueBytes = GetValueBytes();
        var length = (byte)valueBytes.Length;

        var result = new byte[2 + valueBytes.Length];
        result[0] = TagNumber;
        result[1] = length;
        Array.Copy(valueBytes, 0, result, 2, valueBytes.Length);

        return result;
    }

    /// <summary>
    /// Returns the TLV encoded string representation.
    /// </summary>
    /// <returns>TLV encoded string.</returns>
    public override string ToString()
    {
        return Encoding.UTF8.GetString(ToBytes());
    }
}
