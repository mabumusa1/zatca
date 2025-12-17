namespace Zatca.EInvoice.Models.Financial;

/// <summary>
/// Represents the total tax details for an invoice.
/// </summary>
public class TaxTotal
{
    private decimal? _taxAmount;
    private decimal? _roundingAmount;

    /// <summary>
    /// Gets or sets the total tax amount.
    /// </summary>
    public decimal? TaxAmount
    {
        get => _taxAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Tax amount must be non-negative.");
            _taxAmount = value;
        }
    }

    /// <summary>
    /// Gets or sets the rounding amount.
    /// </summary>
    public decimal? RoundingAmount
    {
        get => _roundingAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Rounding amount must be non-negative.");
            _roundingAmount = value;
        }
    }

    /// <summary>
    /// Gets or sets the array of tax subtotals.
    /// </summary>
    public List<TaxSubTotal> TaxSubTotals { get; set; } = new();

    /// <summary>
    /// Adds a TaxSubTotal object to the tax subtotals list.
    /// </summary>
    public void AddTaxSubTotal(TaxSubTotal taxSubTotal)
    {
        TaxSubTotals.Add(taxSubTotal);
    }

    /// <summary>
    /// Validates that required fields are set.
    /// </summary>
    public void Validate()
    {
        if (!TaxAmount.HasValue)
            throw new ArgumentException("Missing TaxTotal taxAmount.");
    }
}
