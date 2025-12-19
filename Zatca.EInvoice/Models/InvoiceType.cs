using Zatca.EInvoice.Models.Enums;

namespace Zatca.EInvoice.Models;

/// <summary>
/// Represents the type of an invoice.
/// </summary>
public class InvoiceType
{
    private string? _invoice;
    private string? _invoiceType;

    /// <summary>
    /// Gets or sets the main invoice category ("standard" or "simplified").
    /// </summary>
    public string? Invoice
    {
        get => _invoice;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invoice category cannot be empty.");
            _invoice = value?.ToLower();
        }
    }

    /// <summary>
    /// Gets or sets the invoice sub-type ("invoice", "debit", "credit", or "prepayment").
    /// </summary>
    public string? InvoiceSubType
    {
        get => _invoiceType;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invoice type cannot be empty.");
            _invoiceType = value?.ToLower();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the invoice is an export invoice.
    /// </summary>
    public bool IsExportInvoice { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the invoice is a third-party transaction.
    /// </summary>
    public bool IsThirdParty { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the invoice is a nominal transaction.
    /// </summary>
    public bool IsNominal { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the invoice is a summary invoice.
    /// </summary>
    public bool IsSummary { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the invoice is self-billed.
    /// </summary>
    public bool IsSelfBilled { get; set; }

    /// <summary>
    /// Gets the invoice type code based on the invoice sub-type.
    /// </summary>
    public int GetInvoiceTypeCode()
    {
        return InvoiceSubType?.ToLower() switch
        {
            "invoice" => InvoiceTypeCode.INVOICE,
            "debit" => InvoiceTypeCode.DEBIT_NOTE,
            "credit" => InvoiceTypeCode.CREDIT_NOTE,
            "prepayment" => InvoiceTypeCode.PREPAYMENT,
            _ => throw new ArgumentException("Invalid invoice type provided.")
        };
    }

    /// <summary>
    /// Gets the complete invoice type value based on the invoice category.
    /// </summary>
    public string GetInvoiceTypeValue()
    {
        if (string.IsNullOrEmpty(Invoice) || string.IsNullOrEmpty(InvoiceSubType))
            throw new ArgumentException("Invoice category and type must be set.");

        string invoiceTypeValue = GetBaseInvoiceTypeValue();

        return ApplyInvoiceFlags(invoiceTypeValue);
    }

    private string GetBaseInvoiceTypeValue()
    {
        var invoiceLower = Invoice?.ToLower();
        var subTypeLower = InvoiceSubType?.ToLower();

        return invoiceLower switch
        {
            "standard" => GetStandardInvoiceTypeValue(subTypeLower),
            "simplified" => GetSimplifiedInvoiceTypeValue(subTypeLower),
            _ => throw new ArgumentException("Invalid invoice category provided.")
        };
    }

    private static string GetStandardInvoiceTypeValue(string? subType)
    {
        return subType switch
        {
            "invoice" or "debit" or "credit" or "prepayment" => InvoiceTypeCode.STANDARD_INVOICE,
            _ => throw new ArgumentException("Invalid invoice type provided.")
        };
    }

    private static string GetSimplifiedInvoiceTypeValue(string? subType)
    {
        return subType switch
        {
            "invoice" or "debit" or "credit" => InvoiceTypeCode.SIMPLIFIED_INVOICE,
            "prepayment" => InvoiceTypeCode.STANDARD_INVOICE,
            _ => throw new ArgumentException("Invalid invoice type provided.")
        };
    }

    private string ApplyInvoiceFlags(string invoiceTypeValue)
    {
        if (invoiceTypeValue.Length < 7)
            return invoiceTypeValue;

        string prefix = invoiceTypeValue.Substring(0, 2);
        char p = IsThirdParty ? '1' : '0';
        char n = IsNominal ? '1' : '0';
        char e = IsExportInvoice ? '1' : '0';
        char s = IsSummary ? '1' : '0';
        char b = IsSelfBilled ? '1' : '0';

        return $"{prefix}{p}{n}{e}{s}{b}";
    }

    /// <summary>
    /// Validates that invoice type is properly configured.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(InvoiceSubType) || string.IsNullOrEmpty(Invoice))
            throw new ArgumentException("Invoice category and type must be set.");
    }
}
