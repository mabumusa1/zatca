namespace Zatca.EInvoice.Models.References;

/// <summary>
/// Represents a contract with an identifier.
/// </summary>
public class Contract
{
    private string? _id;

    /// <summary>
    /// Gets or sets the contract identifier.
    /// </summary>
    public string? Id
    {
        get => _id;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Contract ID cannot be empty.");
            _id = value;
        }
    }
}
