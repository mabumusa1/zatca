namespace Zatca.EInvoice.Signing;

/// <summary>
/// Result of the invoice signing operation.
/// </summary>
public class SignedInvoiceResult
{
    /// <summary>
    /// Gets or sets the signed invoice XML as a string.
    /// </summary>
    public string SignedXml { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice hash (SHA-256, base64 encoded).
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the QR code (TLV encoded, base64 encoded).
    /// </summary>
    public string QrCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the digital signature (base64 encoded).
    /// </summary>
    public string DigitalSignature { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice UUID extracted from the signed XML.
    /// </summary>
    public string Uuid { get; set; } = string.Empty;
}
