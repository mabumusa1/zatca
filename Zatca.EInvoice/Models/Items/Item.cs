namespace Zatca.EInvoice.Models.Items;

/// <summary>
/// Represents an item in an invoice with its details.
/// </summary>
public class Item
{
    private string? _description;
    private string? _name;
    private string? _standardItemIdentification;
    private string? _buyersItemIdentification;
    private string? _sellersItemIdentification;

    /// <summary>
    /// Gets or sets the item description.
    /// </summary>
    public string? Description
    {
        get => _description;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Description cannot be an empty string.");
            _description = value;
        }
    }

    /// <summary>
    /// Gets or sets the item name (mandatory).
    /// </summary>
    public string? Name
    {
        get => _name;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.");
            _name = value;
        }
    }

    /// <summary>
    /// Gets or sets the standard item identification.
    /// </summary>
    public string? StandardItemIdentification
    {
        get => _standardItemIdentification;
        set => _standardItemIdentification = value;
    }

    /// <summary>
    /// Gets or sets the buyers item identification.
    /// </summary>
    public string? BuyersItemIdentification
    {
        get => _buyersItemIdentification;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Buyers item identification cannot be empty.");
            _buyersItemIdentification = value;
        }
    }

    /// <summary>
    /// Gets or sets the sellers item identification.
    /// </summary>
    public string? SellersItemIdentification
    {
        get => _sellersItemIdentification;
        set
        {
            if (value != null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Sellers item identification cannot be empty.");
            _sellersItemIdentification = value;
        }
    }

    /// <summary>
    /// Gets or sets the classified tax categories.
    /// </summary>
    public List<ClassifiedTaxCategory>? ClassifiedTaxCategories { get; set; }
}
