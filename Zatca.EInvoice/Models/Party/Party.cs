namespace Zatca.EInvoice.Models.Party;

/// <summary>
/// Represents a party with identification, address, tax scheme, and legal entity information.
/// </summary>
public class Party
{
    private string? _partyIdentification;
    private string? _partyIdentificationId;

    /// <summary>
    /// Gets or sets the party identification value.
    /// </summary>
    public string? PartyIdentification
    {
        get => _partyIdentification;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Party identification cannot be empty.");
            _partyIdentification = value;
        }
    }

    /// <summary>
    /// Gets or sets the party identification scheme identifier.
    /// </summary>
    public string? PartyIdentificationId
    {
        get => _partyIdentificationId;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Party identification scheme ID cannot be empty.");
            _partyIdentificationId = value;
        }
    }

    /// <summary>
    /// Gets or sets the postal address.
    /// </summary>
    public Address? PostalAddress { get; set; }

    /// <summary>
    /// Gets or sets the party tax scheme details.
    /// </summary>
    public PartyTaxScheme? PartyTaxScheme { get; set; }

    /// <summary>
    /// Gets or sets the legal entity details.
    /// </summary>
    public LegalEntity? LegalEntity { get; set; }
}
