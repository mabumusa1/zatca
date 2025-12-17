namespace Zatca.EInvoice.Models.Signature;

/// <summary>
/// Represents signature information.
/// </summary>
public class Signature
{
    private string _id = "urn:oasis:names:specification:ubl:signature:Invoice";
    private string _signatureMethod = "urn:oasis:names:specification:ubl:dsig:enveloped:xades";

    /// <summary>
    /// Gets or sets the signature identifier.
    /// </summary>
    public string Id
    {
        get => _id;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Signature ID cannot be empty.");
            _id = value;
        }
    }

    /// <summary>
    /// Gets or sets the signature method.
    /// </summary>
    public string SignatureMethod
    {
        get => _signatureMethod;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Signature method cannot be empty.");
            _signatureMethod = value;
        }
    }
}
