namespace Zatca.EInvoice.Models.References;

/// <summary>
/// Represents an invoice period with start and end dates.
/// </summary>
public class InvoicePeriod
{
    /// <summary>
    /// Gets or sets the start date of the invoice period.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the invoice period.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the description of the invoice period.
    /// </summary>
    public string? Description { get; set; }
}
