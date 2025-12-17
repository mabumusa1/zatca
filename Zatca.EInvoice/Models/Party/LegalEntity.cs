namespace Zatca.EInvoice.Models.Party;

/// <summary>
/// Represents a legal entity with registration details.
/// </summary>
public class LegalEntity
{
    private string? _registrationName;

    /// <summary>
    /// Gets or sets the registration name of the legal entity.
    /// </summary>
    public string? RegistrationName
    {
        get => _registrationName;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Registration name cannot be empty.");
            _registrationName = value;
        }
    }
}
