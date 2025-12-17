namespace Zatca.EInvoice.Tags;

/// <summary>
/// Tag 7: Digital signature (base64 encoded).
/// </summary>
public class DigitalSignatureTag : Tag
{
    /// <summary>
    /// Initializes a new instance of the DigitalSignatureTag class.
    /// </summary>
    /// <param name="signature">The base64-encoded digital signature.</param>
    public DigitalSignatureTag(string signature) : base(7, signature)
    {
    }
}
