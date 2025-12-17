namespace Zatca.EInvoice.Models.Enums;

/// <summary>
/// Contains constants representing invoice type codes and their subtypes.
/// </summary>
public static class InvoiceTypeCode
{
    /// <summary>
    /// Invoice type code (388)
    /// </summary>
    public const int INVOICE = 388;

    /// <summary>
    /// Credit note type code (381)
    /// </summary>
    public const int CREDIT_NOTE = 381;

    /// <summary>
    /// Debit note type code (383)
    /// </summary>
    public const int DEBIT_NOTE = 383;

    /// <summary>
    /// Prepayment type code (386)
    /// </summary>
    public const int PREPAYMENT = 386;

    /// <summary>
    /// Standard invoice type value (0100000)
    /// </summary>
    public const string STANDARD_INVOICE = "0100000";

    /// <summary>
    /// Simplified invoice type value (0200000)
    /// </summary>
    public const string SIMPLIFIED_INVOICE = "0200000";
}
