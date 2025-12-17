namespace Zatca.EInvoice.Models.Signature;

/// <summary>
/// Represents UBL document signatures.
/// </summary>
public class UblDocumentSignatures
{
    /// <summary>
    /// Gets or sets the signature information.
    /// </summary>
    public SignatureInformation? SignatureInformation { get; set; }

    /// <summary>
    /// Validates that signature information is set.
    /// </summary>
    public void Validate()
    {
        if (SignatureInformation == null)
            throw new ArgumentException("Signature information must be set.");
    }
}
