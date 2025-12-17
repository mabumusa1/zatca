namespace Zatca.EInvoice.Tags;

/// <summary>
/// Tag 6: Invoice hash (SHA-256, base64 encoded).
/// </summary>
public class InvoiceHashTag : Tag
{
    /// <summary>
    /// Initializes a new instance of the InvoiceHashTag class.
    /// </summary>
    /// <param name="hash">The base64-encoded SHA-256 hash of the invoice.</param>
    public InvoiceHashTag(string hash) : base(6, hash)
    {
    }
}
