using System.Text.Json.Serialization;

namespace Zatca.EInvoice.Pdf;

/// <summary>
/// Data model for PDF invoice generation.
/// </summary>
public class InvoicePdfData
{
    /// <summary>
    /// Gets or sets the invoice ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the universal unique identifier (UUID) for the invoice.
    /// </summary>
    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    /// <summary>
    /// Gets or sets the issue date of the invoice.
    /// </summary>
    [JsonPropertyName("issueDate")]
    public string? IssueDate { get; set; }

    /// <summary>
    /// Gets or sets the issue time of the invoice.
    /// </summary>
    [JsonPropertyName("issueTime")]
    public string? IssueTime { get; set; }

    /// <summary>
    /// Gets or sets the currency code for the invoice.
    /// </summary>
    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Gets or sets the tax currency code.
    /// </summary>
    [JsonPropertyName("taxCurrencyCode")]
    public string? TaxCurrencyCode { get; set; }

    /// <summary>
    /// Gets or sets the invoice type information.
    /// </summary>
    [JsonPropertyName("invoiceType")]
    public InvoiceTypePdfData? InvoiceType { get; set; }

    /// <summary>
    /// Gets or sets the supplier party information.
    /// </summary>
    [JsonPropertyName("supplier")]
    public PartyPdfData? Supplier { get; set; }

    /// <summary>
    /// Gets or sets the customer party information.
    /// </summary>
    [JsonPropertyName("customer")]
    public PartyPdfData? Customer { get; set; }

    /// <summary>
    /// Gets or sets the list of invoice line items.
    /// </summary>
    [JsonPropertyName("invoiceLines")]
    public List<InvoiceLinePdfData>? InvoiceLines { get; set; }

    /// <summary>
    /// Gets or sets the tax total information.
    /// </summary>
    [JsonPropertyName("taxTotal")]
    public TaxTotalPdfData? TaxTotal { get; set; }

    /// <summary>
    /// Gets or sets the legal monetary total information.
    /// </summary>
    [JsonPropertyName("legalMonetaryTotal")]
    public LegalMonetaryTotalPdfData? LegalMonetaryTotal { get; set; }

    /// <summary>
    /// Gets or sets the list of allowances and charges.
    /// </summary>
    [JsonPropertyName("allowanceCharges")]
    public List<AllowanceChargePdfData>? AllowanceCharges { get; set; }

    /// <summary>
    /// Gets or sets the list of billing references.
    /// </summary>
    [JsonPropertyName("billingReferences")]
    public List<BillingReferencePdfData>? BillingReferences { get; set; }

    /// <summary>
    /// Gets or sets additional notes for the invoice.
    /// </summary>
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the language identifier.
    /// </summary>
    [JsonPropertyName("languageID")]
    public string? LanguageId { get; set; }

    /// <summary>
    /// Gets or sets the delivery information.
    /// </summary>
    [JsonPropertyName("delivery")]
    public DeliveryPdfData? Delivery { get; set; }

    /// <summary>
    /// Gets or sets the payment means information.
    /// </summary>
    [JsonPropertyName("paymentMeans")]
    public PaymentMeansPdfData? PaymentMeans { get; set; }
}

/// <summary>
/// Data model for invoice type information in PDF generation.
/// </summary>
public class InvoiceTypePdfData
{
    /// <summary>
    /// Gets or sets the invoice type code.
    /// </summary>
    [JsonPropertyName("typeCode")]
    public int? TypeCode { get; set; }

    /// <summary>
    /// Gets or sets the invoice type name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Data model for party (supplier or customer) information in PDF generation.
/// </summary>
public class PartyPdfData
{
    /// <summary>
    /// Gets or sets the party registration name.
    /// </summary>
    [JsonPropertyName("registrationName")]
    public string? RegistrationName { get; set; }

    /// <summary>
    /// Gets or sets the tax ID.
    /// </summary>
    [JsonPropertyName("taxId")]
    public string? TaxId { get; set; }

    /// <summary>
    /// Gets or sets the party identification.
    /// </summary>
    [JsonPropertyName("partyIdentification")]
    public string? PartyIdentification { get; set; }

    /// <summary>
    /// Gets or sets the party identification ID.
    /// </summary>
    [JsonPropertyName("partyIdentificationId")]
    public string? PartyIdentificationId { get; set; }

    /// <summary>
    /// Gets or sets the party address information.
    /// </summary>
    [JsonPropertyName("address")]
    public AddressPdfData? Address { get; set; }

    /// <summary>
    /// Gets or sets the tax scheme information.
    /// </summary>
    [JsonPropertyName("taxScheme")]
    public TaxSchemePdfData? TaxScheme { get; set; }
}

/// <summary>
/// Data model for address information in PDF generation.
/// </summary>
public class AddressPdfData
{
    /// <summary>
    /// Gets or sets the street name.
    /// </summary>
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the building number.
    /// </summary>
    [JsonPropertyName("buildingNumber")]
    public string? BuildingNumber { get; set; }

    /// <summary>
    /// Gets or sets the city subdivision name.
    /// </summary>
    [JsonPropertyName("citySubdivisionName")]
    public string? CitySubdivisionName { get; set; }

    /// <summary>
    /// Gets or sets the city name.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the postal zone (zip code).
    /// </summary>
    [JsonPropertyName("postalZone")]
    public string? PostalZone { get; set; }

    /// <summary>
    /// Gets or sets the country code.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

/// <summary>
/// Data model for invoice line item information in PDF generation.
/// </summary>
public class InvoiceLinePdfData
{
    /// <summary>
    /// Gets or sets the line item ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the quantity of items.
    /// </summary>
    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit code for the quantity.
    /// </summary>
    [JsonPropertyName("unitCode")]
    public string? UnitCode { get; set; }

    /// <summary>
    /// Gets or sets the line extension amount (total before tax).
    /// </summary>
    [JsonPropertyName("lineExtensionAmount")]
    public decimal? LineExtensionAmount { get; set; }

    /// <summary>
    /// Gets or sets the item information.
    /// </summary>
    [JsonPropertyName("item")]
    public ItemPdfData? Item { get; set; }

    /// <summary>
    /// Gets or sets the price information.
    /// </summary>
    [JsonPropertyName("price")]
    public PricePdfData? Price { get; set; }

    /// <summary>
    /// Gets or sets the tax total for this line.
    /// </summary>
    [JsonPropertyName("taxTotal")]
    public TaxTotalPdfData? TaxTotal { get; set; }

    /// <summary>
    /// Gets or sets the allowances and charges for this line.
    /// </summary>
    [JsonPropertyName("allowanceCharges")]
    public List<AllowanceChargePdfData>? AllowanceCharges { get; set; }
}

/// <summary>
/// Data model for item information in PDF generation.
/// </summary>
public class ItemPdfData
{
    /// <summary>
    /// Gets or sets the item name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the item description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the classified tax categories for this item.
    /// </summary>
    [JsonPropertyName("classifiedTaxCategory")]
    public List<TaxCategoryPdfData>? ClassifiedTaxCategory { get; set; }
}

/// <summary>
/// Data model for price information in PDF generation.
/// </summary>
public class PricePdfData
{
    /// <summary>
    /// Gets or sets the price amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Gets or sets the base quantity for the price.
    /// </summary>
    [JsonPropertyName("baseQuantity")]
    public decimal? BaseQuantity { get; set; }
}

/// <summary>
/// Data model for tax total information in PDF generation.
/// </summary>
public class TaxTotalPdfData
{
    /// <summary>
    /// Gets or sets the total tax amount.
    /// </summary>
    [JsonPropertyName("taxAmount")]
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the rounding amount applied to the tax.
    /// </summary>
    [JsonPropertyName("roundingAmount")]
    public decimal? RoundingAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax subtotals breakdown.
    /// </summary>
    [JsonPropertyName("subTotals")]
    public List<TaxSubTotalPdfData>? SubTotals { get; set; }
}

/// <summary>
/// Data model for tax subtotal information in PDF generation.
/// </summary>
public class TaxSubTotalPdfData
{
    /// <summary>
    /// Gets or sets the taxable amount.
    /// </summary>
    [JsonPropertyName("taxableAmount")]
    public decimal? TaxableAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax amount for this subtotal.
    /// </summary>
    [JsonPropertyName("taxAmount")]
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax category information.
    /// </summary>
    [JsonPropertyName("taxCategory")]
    public TaxCategoryPdfData? TaxCategory { get; set; }
}

/// <summary>
/// Data model for tax category information in PDF generation.
/// </summary>
public class TaxCategoryPdfData
{
    /// <summary>
    /// Gets or sets the tax category ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the tax percentage.
    /// </summary>
    [JsonPropertyName("percent")]
    public decimal? Percent { get; set; }

    /// <summary>
    /// Gets or sets the tax scheme information.
    /// </summary>
    [JsonPropertyName("taxScheme")]
    public TaxSchemePdfData? TaxScheme { get; set; }
}

/// <summary>
/// Data model for tax scheme information in PDF generation.
/// </summary>
public class TaxSchemePdfData
{
    /// <summary>
    /// Gets or sets the tax scheme ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

/// <summary>
/// Data model for legal monetary total information in PDF generation.
/// </summary>
public class LegalMonetaryTotalPdfData
{
    /// <summary>
    /// Gets or sets the line extension amount (subtotal before tax).
    /// </summary>
    [JsonPropertyName("lineExtensionAmount")]
    public decimal? LineExtensionAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax exclusive amount.
    /// </summary>
    [JsonPropertyName("taxExclusiveAmount")]
    public decimal? TaxExclusiveAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax inclusive amount (total including tax).
    /// </summary>
    [JsonPropertyName("taxInclusiveAmount")]
    public decimal? TaxInclusiveAmount { get; set; }

    /// <summary>
    /// Gets or sets the total allowance amount.
    /// </summary>
    [JsonPropertyName("allowanceTotalAmount")]
    public decimal? AllowanceTotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the prepaid amount.
    /// </summary>
    [JsonPropertyName("prepaidAmount")]
    public decimal? PrepaidAmount { get; set; }

    /// <summary>
    /// Gets or sets the payable amount (final amount due).
    /// </summary>
    [JsonPropertyName("payableAmount")]
    public decimal? PayableAmount { get; set; }
}

/// <summary>
/// Data model for allowance or charge information in PDF generation.
/// </summary>
public class AllowanceChargePdfData
{
    /// <summary>
    /// Gets or sets whether this is a charge (true) or allowance (false).
    /// </summary>
    [JsonPropertyName("chargeIndicator")]
    public string? ChargeIndicator { get; set; }

    /// <summary>
    /// Gets or sets the reason for the allowance or charge.
    /// </summary>
    [JsonPropertyName("allowanceChargeReason")]
    public string? AllowanceChargeReason { get; set; }

    /// <summary>
    /// Gets or sets the amount of the allowance or charge.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }
}

/// <summary>
/// Data model for billing reference information in PDF generation.
/// </summary>
public class BillingReferencePdfData
{
    /// <summary>
    /// Gets or sets the invoice document reference.
    /// </summary>
    [JsonPropertyName("invoiceDocumentReference")]
    public DocumentReferencePdfData? InvoiceDocumentReference { get; set; }
}

/// <summary>
/// Data model for document reference information in PDF generation.
/// </summary>
public class DocumentReferencePdfData
{
    /// <summary>
    /// Gets or sets the document ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the document UUID.
    /// </summary>
    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }
}

/// <summary>
/// Data model for delivery information in PDF generation.
/// </summary>
public class DeliveryPdfData
{
    /// <summary>
    /// Gets or sets the actual delivery date.
    /// </summary>
    [JsonPropertyName("actualDeliveryDate")]
    public string? ActualDeliveryDate { get; set; }
}

/// <summary>
/// Data model for payment means information in PDF generation.
/// </summary>
public class PaymentMeansPdfData
{
    /// <summary>
    /// Gets or sets the payment means code.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
