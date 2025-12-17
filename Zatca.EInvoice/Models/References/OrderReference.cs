namespace Zatca.EInvoice.Models.References;

/// <summary>
/// Represents an order reference for an invoice.
/// </summary>
public class OrderReference
{
    private string? _id;
    private string? _salesOrderId;

    /// <summary>
    /// Gets or sets the order reference identifier.
    /// </summary>
    public string? Id
    {
        get => _id;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Order reference ID cannot be empty.");
            _id = value;
        }
    }

    /// <summary>
    /// Gets or sets the sales order identifier.
    /// </summary>
    public string? SalesOrderId
    {
        get => _salesOrderId;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Sales order ID cannot be empty.");
            _salesOrderId = value;
        }
    }
}
