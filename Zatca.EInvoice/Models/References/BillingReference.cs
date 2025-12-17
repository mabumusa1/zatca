namespace Zatca.EInvoice.Models.References;

/// <summary>
/// Represents a billing reference for an invoice.
/// </summary>
public class BillingReference
{
    private string? _id;

    /// <summary>
    /// Gets or sets the billing reference identifier.
    /// </summary>
    public string? Id
    {
        get => _id;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ID cannot be empty.");
            _id = value;
        }
    }
}
