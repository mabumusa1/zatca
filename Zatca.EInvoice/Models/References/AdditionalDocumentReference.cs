namespace Zatca.EInvoice.Models.References;

/// <summary>
/// Represents an additional document reference for an invoice.
/// </summary>
public class AdditionalDocumentReference
{
    /// <summary>
    /// Gets or sets the document reference identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the document UUID.
    /// </summary>
    public string? UUID { get; set; }

    /// <summary>
    /// Gets or sets the document type.
    /// </summary>
    public string? DocumentType { get; set; }

    /// <summary>
    /// Gets or sets the document type code.
    /// </summary>
    public int? DocumentTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    public string? DocumentDescription { get; set; }

    /// <summary>
    /// Gets or sets the attachment.
    /// </summary>
    public Attachment? Attachment { get; set; }
}
