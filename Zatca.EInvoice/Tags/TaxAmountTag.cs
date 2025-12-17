namespace Zatca.EInvoice.Tags;

/// <summary>
/// Tag 5: VAT amount.
/// </summary>
public class TaxAmountTag : Tag
{
    /// <summary>
    /// Initializes a new instance of the TaxAmountTag class.
    /// </summary>
    /// <param name="taxAmount">The VAT amount.</param>
    public TaxAmountTag(string taxAmount) : base(5, taxAmount)
    {
    }
}
