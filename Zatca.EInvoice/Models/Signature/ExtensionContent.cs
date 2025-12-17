namespace Zatca.EInvoice.Models.Signature;

/// <summary>
/// Represents extension content containing UBL document signatures.
/// </summary>
public class ExtensionContent
{
    /// <summary>
    /// Gets or sets the UBL document signatures.
    /// </summary>
    public UblDocumentSignatures? UblDocumentSignatures { get; set; }

    /// <summary>
    /// Validates that UBL document signatures are set.
    /// </summary>
    public void Validate()
    {
        if (UblDocumentSignatures == null)
            throw new ArgumentException("UBLDocumentSignatures must be set.");
    }
}
