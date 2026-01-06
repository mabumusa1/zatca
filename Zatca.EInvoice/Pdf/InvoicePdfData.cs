using System.Text.Json.Serialization;

namespace Zatca.EInvoice.Pdf;

/// <summary>
/// Data model for PDF invoice generation.
/// </summary>
public class InvoicePdfData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    [JsonPropertyName("issueDate")]
    public string? IssueDate { get; set; }

    [JsonPropertyName("issueTime")]
    public string? IssueTime { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("taxCurrencyCode")]
    public string? TaxCurrencyCode { get; set; }

    [JsonPropertyName("invoiceType")]
    public InvoiceTypePdfData? InvoiceType { get; set; }

    [JsonPropertyName("supplier")]
    public PartyPdfData? Supplier { get; set; }

    [JsonPropertyName("customer")]
    public PartyPdfData? Customer { get; set; }

    [JsonPropertyName("invoiceLines")]
    public List<InvoiceLinePdfData>? InvoiceLines { get; set; }

    [JsonPropertyName("taxTotal")]
    public TaxTotalPdfData? TaxTotal { get; set; }

    [JsonPropertyName("legalMonetaryTotal")]
    public LegalMonetaryTotalPdfData? LegalMonetaryTotal { get; set; }

    [JsonPropertyName("allowanceCharges")]
    public List<AllowanceChargePdfData>? AllowanceCharges { get; set; }

    [JsonPropertyName("billingReferences")]
    public List<BillingReferencePdfData>? BillingReferences { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("languageID")]
    public string? LanguageId { get; set; }

    [JsonPropertyName("delivery")]
    public DeliveryPdfData? Delivery { get; set; }

    [JsonPropertyName("paymentMeans")]
    public PaymentMeansPdfData? PaymentMeans { get; set; }
}

public class InvoiceTypePdfData
{
    [JsonPropertyName("typeCode")]
    public int? TypeCode { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class PartyPdfData
{
    [JsonPropertyName("registrationName")]
    public string? RegistrationName { get; set; }

    [JsonPropertyName("taxId")]
    public string? TaxId { get; set; }

    [JsonPropertyName("partyIdentification")]
    public string? PartyIdentification { get; set; }

    [JsonPropertyName("partyIdentificationId")]
    public string? PartyIdentificationId { get; set; }

    [JsonPropertyName("address")]
    public AddressPdfData? Address { get; set; }

    [JsonPropertyName("taxScheme")]
    public TaxSchemePdfData? TaxScheme { get; set; }
}

public class AddressPdfData
{
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("buildingNumber")]
    public string? BuildingNumber { get; set; }

    [JsonPropertyName("citySubdivisionName")]
    public string? CitySubdivisionName { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("postalZone")]
    public string? PostalZone { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

public class InvoiceLinePdfData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    [JsonPropertyName("unitCode")]
    public string? UnitCode { get; set; }

    [JsonPropertyName("lineExtensionAmount")]
    public decimal? LineExtensionAmount { get; set; }

    [JsonPropertyName("item")]
    public ItemPdfData? Item { get; set; }

    [JsonPropertyName("price")]
    public PricePdfData? Price { get; set; }

    [JsonPropertyName("taxTotal")]
    public TaxTotalPdfData? TaxTotal { get; set; }

    [JsonPropertyName("allowanceCharges")]
    public List<AllowanceChargePdfData>? AllowanceCharges { get; set; }
}

public class ItemPdfData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("classifiedTaxCategory")]
    public List<TaxCategoryPdfData>? ClassifiedTaxCategory { get; set; }
}

public class PricePdfData
{
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("baseQuantity")]
    public decimal? BaseQuantity { get; set; }
}

public class TaxTotalPdfData
{
    [JsonPropertyName("taxAmount")]
    public decimal? TaxAmount { get; set; }

    [JsonPropertyName("roundingAmount")]
    public decimal? RoundingAmount { get; set; }

    [JsonPropertyName("subTotals")]
    public List<TaxSubTotalPdfData>? SubTotals { get; set; }
}

public class TaxSubTotalPdfData
{
    [JsonPropertyName("taxableAmount")]
    public decimal? TaxableAmount { get; set; }

    [JsonPropertyName("taxAmount")]
    public decimal? TaxAmount { get; set; }

    [JsonPropertyName("taxCategory")]
    public TaxCategoryPdfData? TaxCategory { get; set; }
}

public class TaxCategoryPdfData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("percent")]
    public decimal? Percent { get; set; }

    [JsonPropertyName("taxScheme")]
    public TaxSchemePdfData? TaxScheme { get; set; }
}

public class TaxSchemePdfData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class LegalMonetaryTotalPdfData
{
    [JsonPropertyName("lineExtensionAmount")]
    public decimal? LineExtensionAmount { get; set; }

    [JsonPropertyName("taxExclusiveAmount")]
    public decimal? TaxExclusiveAmount { get; set; }

    [JsonPropertyName("taxInclusiveAmount")]
    public decimal? TaxInclusiveAmount { get; set; }

    [JsonPropertyName("allowanceTotalAmount")]
    public decimal? AllowanceTotalAmount { get; set; }

    [JsonPropertyName("prepaidAmount")]
    public decimal? PrepaidAmount { get; set; }

    [JsonPropertyName("payableAmount")]
    public decimal? PayableAmount { get; set; }
}

public class AllowanceChargePdfData
{
    [JsonPropertyName("chargeIndicator")]
    public string? ChargeIndicator { get; set; }

    [JsonPropertyName("allowanceChargeReason")]
    public string? AllowanceChargeReason { get; set; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }
}

public class BillingReferencePdfData
{
    [JsonPropertyName("invoiceDocumentReference")]
    public DocumentReferencePdfData? InvoiceDocumentReference { get; set; }
}

public class DocumentReferencePdfData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }
}

public class DeliveryPdfData
{
    [JsonPropertyName("actualDeliveryDate")]
    public string? ActualDeliveryDate { get; set; }
}

public class PaymentMeansPdfData
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
}
