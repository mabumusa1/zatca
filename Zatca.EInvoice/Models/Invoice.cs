using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Models.Party;
using Zatca.EInvoice.Models.References;
using Zatca.EInvoice.Models.Signature;

namespace Zatca.EInvoice.Models;

/// <summary>
/// Represents an invoice and provides methods to manage invoice data.
/// </summary>
public class Invoice
{
    private string? _id;
    private string? _uuid;
    private string _languageID = "en";
    private string _invoiceCurrencyCode = "SAR";
    private string _taxCurrencyCode = "SAR";
    private string _documentCurrencyCode = "SAR";

    /// <summary>
    /// Gets or sets the UBL extensions.
    /// </summary>
    public UblExtensions? UblExtensions { get; set; }

    /// <summary>
    /// Gets or sets the profile ID (default: 'reporting:1.0').
    /// </summary>
    public string ProfileID { get; set; } = "reporting:1.0";

    /// <summary>
    /// Gets or sets the invoice identifier.
    /// </summary>
    public string? Id
    {
        get => _id;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Missing invoice id.");
            _id = value;
        }
    }

    /// <summary>
    /// Gets or sets the invoice UUID.
    /// </summary>
    public string? UUID
    {
        get => _uuid;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invoice UUID cannot be empty.");
            _uuid = value;
        }
    }

    /// <summary>
    /// Gets or sets the issue date.
    /// </summary>
    public DateOnly? IssueDate { get; set; }

    /// <summary>
    /// Gets or sets the issue time.
    /// </summary>
    public TimeOnly? IssueTime { get; set; }

    /// <summary>
    /// Gets or sets the invoice type.
    /// </summary>
    public InvoiceType? InvoiceType { get; set; }

    /// <summary>
    /// Gets or sets the note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the language ID (default: 'en').
    /// </summary>
    public string LanguageID
    {
        get => _languageID;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("languageID cannot be empty.");
            _languageID = value;
        }
    }

    /// <summary>
    /// Gets or sets the invoice currency code (default: 'SAR').
    /// </summary>
    public string InvoiceCurrencyCode
    {
        get => _invoiceCurrencyCode;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invoice currency code cannot be empty.");
            _invoiceCurrencyCode = value;
        }
    }

    /// <summary>
    /// Gets or sets the tax currency code (default: 'SAR').
    /// </summary>
    public string TaxCurrencyCode
    {
        get => _taxCurrencyCode;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tax currency code cannot be empty.");
            _taxCurrencyCode = value;
        }
    }

    /// <summary>
    /// Gets or sets the document currency code (default: 'SAR').
    /// </summary>
    public string DocumentCurrencyCode
    {
        get => _documentCurrencyCode;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Document currency code cannot be empty.");
            _documentCurrencyCode = value;
        }
    }

    /// <summary>
    /// Gets or sets the order reference.
    /// </summary>
    public OrderReference? OrderReference { get; set; }

    /// <summary>
    /// Gets or sets the billing references.
    /// </summary>
    public List<BillingReference>? BillingReferences { get; set; }

    /// <summary>
    /// Gets or sets the contract.
    /// </summary>
    public Contract? Contract { get; set; }

    /// <summary>
    /// Gets or sets the additional document references.
    /// </summary>
    public List<AdditionalDocumentReference>? AdditionalDocumentReferences { get; set; }

    /// <summary>
    /// Gets or sets the accounting supplier party.
    /// </summary>
    public Party.Party? AccountingSupplierParty { get; set; }

    /// <summary>
    /// Gets or sets the accounting customer party.
    /// </summary>
    public Party.Party? AccountingCustomerParty { get; set; }

    /// <summary>
    /// Gets or sets the delivery details.
    /// </summary>
    public Delivery? Delivery { get; set; }

    /// <summary>
    /// Gets or sets the payment means.
    /// </summary>
    public PaymentMeans? PaymentMeans { get; set; }

    /// <summary>
    /// Gets or sets the allowance charges.
    /// </summary>
    public List<AllowanceCharge>? AllowanceCharges { get; set; }

    /// <summary>
    /// Gets or sets the tax total details.
    /// </summary>
    public TaxTotal? TaxTotal { get; set; }

    /// <summary>
    /// Gets or sets the legal monetary total.
    /// </summary>
    public LegalMonetaryTotal? LegalMonetaryTotal { get; set; }

    /// <summary>
    /// Gets or sets the invoice lines.
    /// </summary>
    public List<InvoiceLine>? InvoiceLines { get; set; }

    /// <summary>
    /// Gets or sets the signature information.
    /// </summary>
    public Signature.Signature? Signature { get; set; }

    /// <summary>
    /// Validates required invoice data before processing.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Id))
            throw new ArgumentException("Missing invoice id.");
        if (!IssueDate.HasValue)
            throw new ArgumentException("Invalid invoice issueDate.");
        if (!IssueTime.HasValue)
            throw new ArgumentException("Invalid invoice issueTime.");
        if (AccountingSupplierParty == null)
            throw new ArgumentException("Missing invoice accountingSupplierParty.");
        if (AccountingCustomerParty == null)
            throw new ArgumentException("Missing invoice accountingCustomerParty.");
        if (AdditionalDocumentReferences == null || AdditionalDocumentReferences.Count == 0)
            throw new ArgumentException("Missing invoice additionalDocumentReferences.");
        if (InvoiceLines == null || InvoiceLines.Count == 0)
            throw new ArgumentException("Missing invoice lines.");
        if (LegalMonetaryTotal == null)
            throw new ArgumentException("Missing invoice LegalMonetaryTotal.");
    }
}
