namespace Zatca.EInvoice.Models.Signature;

/// <summary>
/// Represents signature information used in digital signatures.
/// </summary>
public class SignatureInformation
{
    private string? _id;
    private string? _referencedSignatureID;

    /// <summary>
    /// Gets or sets the signature identifier.
    /// </summary>
    public string? Id
    {
        get => _id;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Signature ID cannot be empty.");
            _id = value;
        }
    }

    /// <summary>
    /// Gets or sets the referenced signature identifier.
    /// </summary>
    public string? ReferencedSignatureID
    {
        get => _referencedSignatureID;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Referenced Signature ID cannot be empty.");
            _referencedSignatureID = value;
        }
    }
}
