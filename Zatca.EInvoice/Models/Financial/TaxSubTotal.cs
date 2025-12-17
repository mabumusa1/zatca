namespace Zatca.EInvoice.Models.Financial;

/// <summary>
/// Represents the subtotal for tax calculations.
/// </summary>
public class TaxSubTotal
{
    /// <summary>
    /// Gets or sets the taxable amount.
    /// </summary>
    public decimal? TaxableAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax amount.
    /// </summary>
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax category.
    /// </summary>
    public TaxCategory? TaxCategory { get; set; }

    /// <summary>
    /// Gets or sets the tax percentage.
    /// </summary>
    public decimal? Percent { get; set; }

    /// <summary>
    /// Validates that the required data is present.
    /// </summary>
    public void Validate()
    {
        if (!TaxableAmount.HasValue)
            throw new ArgumentException("Missing taxsubtotal taxableAmount.");
        if (!TaxAmount.HasValue)
            throw new ArgumentException("Missing taxsubtotal taxAmount.");
        if (TaxCategory == null)
            throw new ArgumentException("Missing taxsubtotal taxCategory.");
    }
}
