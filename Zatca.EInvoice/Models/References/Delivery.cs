using Zatca.EInvoice.Models.Party;

namespace Zatca.EInvoice.Models.References;

/// <summary>
/// Represents a delivery with actual/latest delivery dates and a location.
/// </summary>
public class Delivery
{
    /// <summary>
    /// Gets or sets the actual delivery date.
    /// </summary>
    public DateOnly? ActualDeliveryDate { get; set; }

    /// <summary>
    /// Gets or sets the latest delivery date.
    /// </summary>
    public DateOnly? LatestDeliveryDate { get; set; }

    /// <summary>
    /// Gets or sets the delivery location.
    /// </summary>
    public Address? DeliveryLocation { get; set; }
}
