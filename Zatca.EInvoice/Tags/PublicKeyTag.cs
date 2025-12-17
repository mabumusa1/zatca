namespace Zatca.EInvoice.Tags;

/// <summary>
/// Tag 8: Public key (from certificate).
/// </summary>
public class PublicKeyTag : Tag
{
    /// <summary>
    /// Initializes a new instance of the PublicKeyTag class.
    /// </summary>
    /// <param name="publicKey">The public key bytes.</param>
    public PublicKeyTag(byte[] publicKey) : base(8, publicKey)
    {
    }
}
