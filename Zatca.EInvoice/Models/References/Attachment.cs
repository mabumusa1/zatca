namespace Zatca.EInvoice.Models.References;

/// <summary>
/// Represents an attachment for an invoice.
/// </summary>
public class Attachment
{
    private string _mimeCode = "base64";

    /// <summary>
    /// Gets or sets the file path.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the external reference URL.
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// Gets or sets the Base64 encoded content.
    /// Also known as EmbeddedDocumentBinaryObject in UBL.
    /// </summary>
    public string? Base64Content { get; set; }

    /// <summary>
    /// Gets or sets the embedded document binary object (alias for Base64Content).
    /// </summary>
    public string? EmbeddedDocumentBinaryObject
    {
        get => Base64Content;
        set => Base64Content = value;
    }

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME type (e.g., "text/plain").
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the MIME code (default: "base64").
    /// Used for encoding specification.
    /// </summary>
    public string MimeCode
    {
        get => _mimeCode;
        set => _mimeCode = value ?? "base64";
    }

    /// <summary>
    /// Sets the Base64 content along with file name and MIME type.
    /// </summary>
    public void SetBase64Content(string base64Content, string fileName, string? mimeType)
    {
        Base64Content = base64Content;
        FileName = fileName;
        MimeType = mimeType;
    }

    /// <summary>
    /// Validates the attachment data.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(FilePath) && string.IsNullOrEmpty(ExternalReference) && string.IsNullOrEmpty(Base64Content))
            throw new ArgumentException("Attachment must have a filePath, an externalReference, or a fileContent");

        if (!string.IsNullOrEmpty(Base64Content) && string.IsNullOrEmpty(MimeType))
            throw new ArgumentException("Using base64Content, you need to define a mimeType by also using SetBase64Content");

        if (!string.IsNullOrEmpty(FilePath) && !File.Exists(FilePath))
            throw new ArgumentException("Attachment at filePath does not exist");
    }
}
