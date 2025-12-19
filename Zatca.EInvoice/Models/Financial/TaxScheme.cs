namespace Zatca.EInvoice.Models.Financial;

/// <summary>
/// Represents a tax scheme with its identifier, tax type code, and name.
/// </summary>
public class TaxScheme
{
    private string? _id;

    /// <summary>
    /// Gets or sets the tax scheme identifier.
    /// </summary>
    public string? Id
    {
        get => _id;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tax scheme ID cannot be empty.");
            _id = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax type code.
    /// </summary>
    public string? TaxTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the name of the tax scheme.
    /// </summary>
    public string? Name { get; set; }
}
