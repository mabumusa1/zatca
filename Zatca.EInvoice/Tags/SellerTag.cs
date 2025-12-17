namespace Zatca.EInvoice.Tags;

/// <summary>
/// Tag 1: Seller name (Legal entity name).
/// </summary>
public class SellerTag : Tag
{
    /// <summary>
    /// Initializes a new instance of the SellerTag class.
    /// </summary>
    /// <param name="sellerName">The seller's legal entity name.</param>
    public SellerTag(string sellerName) : base(1, sellerName)
    {
    }
}
