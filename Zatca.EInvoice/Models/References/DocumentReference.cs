namespace Zatca.EInvoice.Models.References;

/// <summary>
/// Represents a document reference.
/// </summary>
public class DocumentReference
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
    /// Gets or sets the issue date.
    /// </summary>
    public DateOnly? IssueDate { get; set; }

    /// <summary>
    /// Gets or sets the document type code.
    /// </summary>
    public string? DocumentTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    public string? DocumentDescription { get; set; }
}
