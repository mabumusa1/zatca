namespace Zatca.EInvoice.Tags;

/// <summary>
/// Tag 3: Invoice date and time (ISO 8601 format).
/// </summary>
public class InvoiceDateTag : Tag
{
    /// <summary>
    /// Initializes a new instance of the InvoiceDateTag class.
    /// </summary>
    /// <param name="dateTime">The invoice date and time in ISO 8601 format (e.g., "2023-12-17T14:30:00Z").</param>
    public InvoiceDateTag(string dateTime) : base(3, dateTime)
    {
    }
}
