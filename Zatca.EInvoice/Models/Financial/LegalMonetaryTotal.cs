namespace Zatca.EInvoice.Models.Financial;

/// <summary>
/// Represents the legal monetary totals in an invoice.
/// </summary>
public class LegalMonetaryTotal
{
    private decimal? _lineExtensionAmount;
    private decimal? _taxExclusiveAmount;
    private decimal? _taxInclusiveAmount;
    private decimal? _allowanceTotalAmount;
    private decimal? _chargeTotalAmount;
    private decimal? _prepaidAmount;
    private decimal? _payableAmount;

    /// <summary>
    /// Gets or sets the total amount of line extensions.
    /// </summary>
    public decimal? LineExtensionAmount
    {
        get => _lineExtensionAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Line extension amount must be non-negative.");
            _lineExtensionAmount = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax exclusive amount.
    /// </summary>
    public decimal? TaxExclusiveAmount
    {
        get => _taxExclusiveAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Tax exclusive amount must be non-negative.");
            _taxExclusiveAmount = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax inclusive amount.
    /// </summary>
    public decimal? TaxInclusiveAmount
    {
        get => _taxInclusiveAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Tax inclusive amount must be non-negative.");
            _taxInclusiveAmount = value;
        }
    }

    /// <summary>
    /// Gets or sets the total allowance amount. Defaults to 0.0 if not set.
    /// </summary>
    public decimal? AllowanceTotalAmount
    {
        get => _allowanceTotalAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Allowance total amount must be non-negative.");
            _allowanceTotalAmount = value ?? 0.0m;
        }
    }

    /// <summary>
    /// Gets or sets the total charge amount. Defaults to 0.0 if not set.
    /// </summary>
    public decimal? ChargeTotalAmount
    {
        get => _chargeTotalAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Charge total amount must be non-negative.");
            _chargeTotalAmount = value ?? 0.0m;
        }
    }

    /// <summary>
    /// Gets or sets the prepaid amount.
    /// </summary>
    public decimal? PrepaidAmount
    {
        get => _prepaidAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Prepaid amount must be non-negative.");
            _prepaidAmount = value;
        }
    }

    /// <summary>
    /// Gets or sets the payable amount.
    /// </summary>
    public decimal? PayableAmount
    {
        get => _payableAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Payable amount must be non-negative.");
            _payableAmount = value;
        }
    }
}
