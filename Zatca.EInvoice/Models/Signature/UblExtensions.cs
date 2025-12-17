namespace Zatca.EInvoice.Models.Signature;

/// <summary>
/// Represents a collection of UBL extensions.
/// </summary>
public class UblExtensions
{
    /// <summary>
    /// Gets or sets the UBL extensions list.
    /// </summary>
    public List<UblExtension> Extensions { get; set; } = new();

    /// <summary>
    /// Validates that UBL extensions are set.
    /// </summary>
    public void Validate()
    {
        if (Extensions == null || Extensions.Count == 0)
            throw new ArgumentException("Missing UBL Extension(s).");
    }
}
