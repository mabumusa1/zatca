namespace Zatca.EInvoice.Tags;

/// <summary>
/// Tag 4: Invoice total amount including VAT.
/// </summary>
public class InvoiceTotalTag : Tag
{
    /// <summary>
    /// Initializes a new instance of the InvoiceTotalTag class.
    /// </summary>
    /// <param name="totalAmount">The total invoice amount including VAT.</param>
    public InvoiceTotalTag(string totalAmount) : base(4, totalAmount)
    {
    }
}
