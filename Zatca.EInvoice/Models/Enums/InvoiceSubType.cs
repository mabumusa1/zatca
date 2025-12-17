namespace Zatca.EInvoice.Models.Enums;

/// <summary>
/// Represents the invoice sub-type.
/// </summary>
public enum InvoiceSubType
{
    /// <summary>
    /// Standard invoice
    /// </summary>
    Invoice,

    /// <summary>
    /// Debit note
    /// </summary>
    Debit,

    /// <summary>
    /// Credit note
    /// </summary>
    Credit,

    /// <summary>
    /// Prepayment invoice
    /// </summary>
    Prepayment
}
