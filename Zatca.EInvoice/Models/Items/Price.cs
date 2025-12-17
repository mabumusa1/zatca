using Zatca.EInvoice.Models.Enums;
using Zatca.EInvoice.Models.Financial;

namespace Zatca.EInvoice.Models.Items;

/// <summary>
/// Represents the price details for an invoice line.
/// </summary>
public class Price
{
    private decimal? _priceAmount;
    private decimal? _baseQuantity;
    private string _unitCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="Price"/> class.
    /// </summary>
    public Price()
    {
        _unitCode = Enums.UnitCode.UNIT;
    }

    /// <summary>
    /// Gets or sets the price amount.
    /// </summary>
    public decimal? PriceAmount
    {
        get => _priceAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Price amount must be non-negative.");
            _priceAmount = value;
        }
    }

    /// <summary>
    /// Gets or sets the base quantity.
    /// </summary>
    public decimal? BaseQuantity
    {
        get => _baseQuantity;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Base quantity must be non-negative.");
            _baseQuantity = value;
        }
    }

    /// <summary>
    /// Gets or sets the unit code.
    /// </summary>
    public string UnitCode
    {
        get => _unitCode;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Unit code cannot be empty.");
            _unitCode = value ?? _unitCode;
        }
    }

    /// <summary>
    /// Gets or sets the allowance charges.
    /// </summary>
    public List<AllowanceCharge>? AllowanceCharges { get; set; }

    /// <summary>
    /// Validates that the price amount is set.
    /// </summary>
    public void Validate()
    {
        if (!PriceAmount.HasValue)
            throw new ArgumentException("Price amount must be set.");
    }
}
