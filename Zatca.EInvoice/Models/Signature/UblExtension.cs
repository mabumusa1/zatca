namespace Zatca.EInvoice.Models.Signature;

/// <summary>
/// Represents an individual UBL extension.
/// </summary>
public class UblExtension
{
    private string? _extensionURI;

    /// <summary>
    /// Gets or sets the extension URI.
    /// </summary>
    public string? ExtensionURI
    {
        get => _extensionURI;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Extension URI cannot be empty.");
            _extensionURI = value;
        }
    }

    /// <summary>
    /// Gets or sets the extension content.
    /// </summary>
    public ExtensionContent? ExtensionContent { get; set; }

    /// <summary>
    /// Validates that required fields are set.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(ExtensionURI))
            throw new ArgumentException("Extension URI is required.");
        if (ExtensionContent == null)
            throw new ArgumentException("Extension content is required.");
    }
}
