namespace Zatca.EInvoice.Tags;

/// <summary>
/// Tag 2: Tax number (VAT registration number).
/// </summary>
public class TaxNumberTag : Tag
{
    /// <summary>
    /// Initializes a new instance of the TaxNumberTag class.
    /// </summary>
    /// <param name="taxNumber">The tax registration number (VAT number).</param>
    public TaxNumberTag(string taxNumber) : base(2, taxNumber)
    {
    }
}
