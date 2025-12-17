using Zatca.EInvoice.Models.Financial;

namespace Zatca.EInvoice.Models.Items;

/// <summary>
/// Represents a classified tax category.
/// </summary>
public class ClassifiedTaxCategory
{
    public const string UNCL5305 = "UNCL5305";

    private string? _id;
    private string? _name;
    private decimal? _percent;
    private string? _taxExemptionReason;
    private string? _taxExemptionReasonCode;
    private string? _schemeID;
    private string? _schemeName;

    /// <summary>
    /// Gets the tax category identifier.
    /// If not explicitly set, it is auto-derived from the percent value.
    /// </summary>
    public string? Id
    {
        get
        {
            if (!string.IsNullOrEmpty(_id))
                return _id;

            // Auto-derive ID from percent
            if (Percent.HasValue)
            {
                if (Percent >= 15)
                    return "S";
                else if (Percent >= 6 && Percent < 15)
                    return "AA";
                else
                    return "Z";
            }

            return null;
        }
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tax category ID cannot be empty.");
            _id = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax category name.
    /// </summary>
    public string? Name
    {
        get => _name;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tax category name cannot be empty.");
            _name = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax percentage.
    /// </summary>
    public decimal? Percent
    {
        get => _percent;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Tax percent must be non-negative.");
            _percent = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax scheme.
    /// </summary>
    public TaxScheme? TaxScheme { get; set; }

    /// <summary>
    /// Gets or sets the scheme ID.
    /// </summary>
    public string? SchemeID
    {
        get => _schemeID;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Scheme ID cannot be empty.");
            _schemeID = value;
        }
    }

    /// <summary>
    /// Gets or sets the scheme name.
    /// </summary>
    public string? SchemeName
    {
        get => _schemeName;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Scheme name cannot be empty.");
            _schemeName = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax exemption reason.
    /// </summary>
    public string? TaxExemptionReason
    {
        get => _taxExemptionReason;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tax exemption reason cannot be empty.");
            _taxExemptionReason = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax exemption reason code.
    /// </summary>
    public string? TaxExemptionReasonCode
    {
        get => _taxExemptionReasonCode;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tax exemption reason code cannot be empty.");
            _taxExemptionReasonCode = value;
        }
    }

    /// <summary>
    /// Validates required data before serialization.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Id))
            throw new ArgumentException("Missing tax category ID.");
        if (!Percent.HasValue)
            throw new ArgumentException("Missing tax category percent.");
    }
}
