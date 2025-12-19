namespace Zatca.EInvoice.Models.Financial;

/// <summary>
/// Represents a tax category for an invoice.
/// </summary>
public class TaxCategory
{
    /// <summary>
    /// UN/ECE 5305 duty or tax or fee category code.
    /// </summary>
    public const string UNCL5305 = "UN/ECE 5305";

    /// <summary>
    /// UN/ECE 5153 duty or tax or fee type name code.
    /// </summary>
    public const string UNCL5153 = "UN/ECE 5153";

    private string? _id;

    /// <summary>
    /// Gets or sets the tax category identifier.
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
                else if (Percent >= 6)
                    return "AA";
                else
                    return "Z";
            }

            return null;
        }
        set => _id = value;
    }

    /// <summary>
    /// Gets or sets the ID attributes (schemeID, schemeAgencyID).
    /// </summary>
    public Dictionary<string, string> IdAttributes { get; set; } = new()
    {
        { "schemeID", UNCL5305 },
        { "schemeAgencyID", "6" }
    };

    /// <summary>
    /// Gets or sets the tax category name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the tax percentage.
    /// </summary>
    public decimal? Percent { get; set; }

    /// <summary>
    /// Gets or sets the tax scheme.
    /// </summary>
    public TaxScheme? TaxScheme { get; set; }

    /// <summary>
    /// Gets or sets the tax scheme attributes (schemeID, schemeAgencyID).
    /// </summary>
    public Dictionary<string, string> TaxSchemeAttributes { get; set; } = new()
    {
        { "schemeID", UNCL5153 },
        { "schemeAgencyID", "6" }
    };

    /// <summary>
    /// Gets or sets the tax exemption reason.
    /// </summary>
    public string? TaxExemptionReason { get; set; }

    /// <summary>
    /// Gets or sets the tax exemption reason code.
    /// </summary>
    public string? TaxExemptionReasonCode { get; set; }

    /// <summary>
    /// Validates required fields before serialization.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Id))
            throw new ArgumentException("Missing tax category id.");
        if (!Percent.HasValue)
            throw new ArgumentException("Missing tax category percent.");
    }
}
