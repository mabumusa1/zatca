namespace Zatca.EInvoice.Models.Financial;

/// <summary>
/// Represents an allowance or charge.
/// </summary>
public class AllowanceCharge
{
    private string? _allowanceChargeReasonCode;
    private string? _allowanceChargeReason;
    private int? _multiplierFactorNumeric;
    private decimal? _baseAmount;
    private decimal? _amount;

    /// <summary>
    /// Gets or sets a value indicating whether this is a charge (true) or an allowance (false).
    /// </summary>
    public bool ChargeIndicator { get; set; }

    /// <summary>
    /// Gets or sets the reason code for the allowance/charge.
    /// </summary>
    public string? AllowanceChargeReasonCode
    {
        get => _allowanceChargeReasonCode;
        set
        {
            if (value != null && !string.IsNullOrWhiteSpace(value) && decimal.TryParse(value, out var numericValue) && numericValue < 0)
                throw new ArgumentException("Allowance charge reason code must be non-negative.");
            _allowanceChargeReasonCode = value;
        }
    }

    /// <summary>
    /// Gets or sets the reason description for the allowance/charge.
    /// </summary>
    public string? AllowanceChargeReason
    {
        get => _allowanceChargeReason;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Allowance charge reason cannot be an empty string.");
            _allowanceChargeReason = value;
        }
    }

    /// <summary>
    /// Gets or sets the multiplier factor numeric value.
    /// </summary>
    public int? MultiplierFactorNumeric
    {
        get => _multiplierFactorNumeric;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Multiplier factor numeric must be non-negative.");
            _multiplierFactorNumeric = value;
        }
    }

    /// <summary>
    /// Gets or sets the base amount.
    /// </summary>
    public decimal? BaseAmount
    {
        get => _baseAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Base amount must be non-negative.");
            _baseAmount = value;
        }
    }

    /// <summary>
    /// Gets or sets the amount value.
    /// </summary>
    public decimal? Amount
    {
        get => _amount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Amount must be non-negative.");
            _amount = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax total information.
    /// </summary>
    public TaxTotal? TaxTotal { get; set; }

    /// <summary>
    /// Gets or sets the list of tax categories.
    /// </summary>
    public List<TaxCategory>? TaxCategories { get; set; }
}
