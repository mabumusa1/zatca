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

        string invoiceTypeValue = Invoice?.ToLower() switch
        {
            "standard" => InvoiceSubType?.ToLower() switch
            {
                "invoice" => InvoiceTypeCode.STANDARD_INVOICE,
                "debit" => InvoiceTypeCode.STANDARD_INVOICE,
                "credit" => InvoiceTypeCode.STANDARD_INVOICE,
                "prepayment" => InvoiceTypeCode.STANDARD_INVOICE,
                _ => throw new ArgumentException("Invalid invoice type provided.")
            },
            "simplified" => InvoiceSubType?.ToLower() switch
            {
                "invoice" => InvoiceTypeCode.SIMPLIFIED_INVOICE,
                "debit" => InvoiceTypeCode.SIMPLIFIED_INVOICE,
                "credit" => InvoiceTypeCode.SIMPLIFIED_INVOICE,
                "prepayment" => InvoiceTypeCode.STANDARD_INVOICE,
                _ => throw new ArgumentException("Invalid invoice type provided.")
            },
            _ => throw new ArgumentException("Invalid invoice category provided.")
        };

        // Adjust type value based on additional flags [PNESB]
        if (invoiceTypeValue.Length >= 7)
        {
            string prefix = invoiceTypeValue.Substring(0, 2);
            char p = IsThirdParty ? '1' : '0';     // Third-party transaction
            char n = IsNominal ? '1' : '0';         // Nominal transaction
            char e = IsExportInvoice ? '1' : '0';   // Export invoice
            char s = IsSummary ? '1' : '0';         // Summary invoice
            char b = IsSelfBilled ? '1' : '0';      // Self-billed invoice

            invoiceTypeValue = $"{prefix}{p}{n}{e}{s}{b}";
        }

        return invoiceTypeValue;
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
