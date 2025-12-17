using Zatca.EInvoice.Models.Financial;
using Zatca.EInvoice.Models.Items;
using Zatca.EInvoice.Models.References;

namespace Zatca.EInvoice.Models;

/// <summary>
/// Represents an invoice line item.
/// </summary>
public class InvoiceLine
{
    private string? _id;
    private decimal? _invoicedQuantity;
    private decimal? _lineExtensionAmount;
    private string _unitCode = "MON";
    private string? _note;
    private string? _accountingCostCode;
    private string? _accountingCost;

    /// <summary>
    /// Gets or sets the invoice line ID.
    /// </summary>
    public string? Id
    {
        get => _id;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Invoice line ID cannot be empty.");
            _id = value;
        }
    }

    /// <summary>
    /// Gets or sets the invoiced quantity.
    /// </summary>
    public decimal? InvoicedQuantity
    {
        get => _invoicedQuantity;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Invoiced quantity must be non-negative.");
            _invoicedQuantity = value;
        }
    }

    /// <summary>
    /// Gets or sets the line extension amount.
    /// </summary>
    public decimal? LineExtensionAmount
    {
        get => _lineExtensionAmount;
        set
        {
            if (value.HasValue && value < 0)
                throw new ArgumentException("Line extension amount must be non-negative.");
            _lineExtensionAmount = value;
        }
    }

    /// <summary>
    /// Gets or sets the allowance charges.
    /// </summary>
    public List<AllowanceCharge>? AllowanceCharges { get; set; }

    /// <summary>
    /// Gets or sets the document reference.
    /// </summary>
    public DocumentReference? DocumentReference { get; set; }

    /// <summary>
    /// Gets or sets the unit code (default 'MON').
    /// </summary>
    public string UnitCode
    {
        get => _unitCode;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Unit code cannot be empty.");
            _unitCode = value ?? _unitCode;
        }
    }

    /// <summary>
    /// Gets or sets the tax total details for the line.
    /// </summary>
    public TaxTotal? TaxTotal { get; set; }

    /// <summary>
    /// Gets or sets the invoice period.
    /// </summary>
    public InvoicePeriod? InvoicePeriod { get; set; }

    /// <summary>
    /// Gets or sets the note for the line.
    /// </summary>
    public string? Note
    {
        get => _note;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Note cannot be empty if provided.");
            _note = value;
        }
    }

    /// <summary>
    /// Gets or sets the item details.
    /// </summary>
    public Item? Item { get; set; }

    /// <summary>
    /// Gets or sets the price details.
    /// </summary>
    public Price? Price { get; set; }

    /// <summary>
    /// Gets or sets the accounting cost code.
    /// </summary>
    public string? AccountingCostCode
    {
        get => _accountingCostCode;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Accounting cost code cannot be empty.");
            _accountingCostCode = value;
        }
    }

    /// <summary>
    /// Gets or sets the accounting cost.
    /// </summary>
    public string? AccountingCost
    {
        get => _accountingCost;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Accounting cost cannot be empty.");
            _accountingCost = value;
        }
    }
}
