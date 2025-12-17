using Zatca.EInvoice.Models.Financial;

namespace Zatca.EInvoice.Models.Party;

/// <summary>
/// Represents a party's tax scheme information.
/// </summary>
public class PartyTaxScheme
{
    private string? _companyId;

    /// <summary>
    /// Gets or sets the company ID.
    /// </summary>
    public string? CompanyId
    {
        get => _companyId;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Company ID cannot be empty.");
            _companyId = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax scheme details.
    /// </summary>
    public TaxScheme? TaxScheme { get; set; }

    /// <summary>
    /// Validates that the required data is set.
    /// </summary>
    public void Validate()
    {
        if (TaxScheme == null)
            throw new ArgumentException("Missing TaxScheme.");
    }
}
