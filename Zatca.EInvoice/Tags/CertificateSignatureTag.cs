namespace Zatca.EInvoice.Tags;

/// <summary>
/// Tag 9: Certificate signature stamp.
/// Used for simplified tax invoices (Invoice type code starting with "02").
/// </summary>
public class CertificateSignatureTag : Tag
{
    /// <summary>
    /// Initializes a new instance of the CertificateSignatureTag class.
    /// </summary>
    /// <param name="certificateSignature">The certificate signature bytes.</param>
    public CertificateSignatureTag(byte[] certificateSignature) : base(9, certificateSignature)
    {
    }
}
