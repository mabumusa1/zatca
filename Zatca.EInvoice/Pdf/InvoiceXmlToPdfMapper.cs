using System.Xml.Linq;

namespace Zatca.EInvoice.Pdf;

/// <summary>
/// Maps UBL Invoice XML to PDF data model.
/// </summary>
public static class InvoiceXmlToPdfMapper
{
    private static readonly XNamespace Cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace Cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace Invoice = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";

    /// <summary>
    /// Maps an invoice XML string to PDF data model.
    /// </summary>
    public static InvoicePdfData MapFromXml(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        var root = doc.Root!;

        var data = new InvoicePdfData
        {
            Id = GetValue(root, Cbc + "ID"),
            Uuid = GetValue(root, Cbc + "UUID"),
            IssueDate = GetValue(root, Cbc + "IssueDate"),
            IssueTime = GetValue(root, Cbc + "IssueTime"),
            CurrencyCode = GetValue(root, Cbc + "DocumentCurrencyCode"),
            TaxCurrencyCode = GetValue(root, Cbc + "TaxCurrencyCode"),
            Note = GetValue(root, Cbc + "Note"),
            LanguageId = root.Element(Cbc + "Note")?.Attribute("languageID")?.Value
        };

        // Invoice Type
        var typeCodeElem = root.Element(Cbc + "InvoiceTypeCode");
        if (typeCodeElem != null)
        {
            data.InvoiceType = new InvoiceTypePdfData
            {
                TypeCode = int.TryParse(typeCodeElem.Value, out var code) ? code : null,
                Name = typeCodeElem.Attribute("name")?.Value
            };
        }

        // Supplier
        var supplierParty = root.Element(Cac + "AccountingSupplierParty")?.Element(Cac + "Party");
        if (supplierParty != null)
        {
            data.Supplier = MapParty(supplierParty);
        }

        // Customer
        var customerParty = root.Element(Cac + "AccountingCustomerParty")?.Element(Cac + "Party");
        if (customerParty != null)
        {
            data.Customer = MapParty(customerParty);
        }

        // Delivery
        var delivery = root.Element(Cac + "Delivery");
        if (delivery != null)
        {
            data.Delivery = new DeliveryPdfData
            {
                ActualDeliveryDate = GetValue(delivery, Cbc + "ActualDeliveryDate")
            };
        }

        // Payment Means
        var paymentMeans = root.Element(Cac + "PaymentMeans");
        if (paymentMeans != null)
        {
            data.PaymentMeans = new PaymentMeansPdfData
            {
                Code = GetValue(paymentMeans, Cbc + "PaymentMeansCode")
            };
        }

        // Allowance/Charges (document level)
        data.AllowanceCharges = root.Elements(Cac + "AllowanceCharge")
            .Select(MapAllowanceCharge)
            .ToList();

        // Tax Total
        var taxTotals = root.Elements(Cac + "TaxTotal").ToList();
        if (taxTotals.Count > 0)
        {
            // Use the tax total with subtotals if available
            var taxTotalWithSubtotals = taxTotals.FirstOrDefault(t => t.Elements(Cac + "TaxSubtotal").Any())
                                        ?? taxTotals.First();
            data.TaxTotal = MapTaxTotal(taxTotalWithSubtotals);
        }

        // Legal Monetary Total
        var legalMonetary = root.Element(Cac + "LegalMonetaryTotal");
        if (legalMonetary != null)
        {
            data.LegalMonetaryTotal = new LegalMonetaryTotalPdfData
            {
                LineExtensionAmount = GetDecimalValue(legalMonetary, Cbc + "LineExtensionAmount"),
                TaxExclusiveAmount = GetDecimalValue(legalMonetary, Cbc + "TaxExclusiveAmount"),
                TaxInclusiveAmount = GetDecimalValue(legalMonetary, Cbc + "TaxInclusiveAmount"),
                AllowanceTotalAmount = GetDecimalValue(legalMonetary, Cbc + "AllowanceTotalAmount"),
                PrepaidAmount = GetDecimalValue(legalMonetary, Cbc + "PrepaidAmount"),
                PayableAmount = GetDecimalValue(legalMonetary, Cbc + "PayableAmount")
            };
        }

        // Invoice Lines
        data.InvoiceLines = root.Elements(Cac + "InvoiceLine")
            .Select(MapInvoiceLine)
            .ToList();

        // Billing References (for credit/debit notes)
        data.BillingReferences = root.Elements(Cac + "BillingReference")
            .Select(br => new BillingReferencePdfData
            {
                InvoiceDocumentReference = new DocumentReferencePdfData
                {
                    Id = GetValue(br.Element(Cac + "InvoiceDocumentReference"), Cbc + "ID"),
                    Uuid = GetValue(br.Element(Cac + "InvoiceDocumentReference"), Cbc + "UUID")
                }
            })
            .ToList();

        return data;
    }

    /// <summary>
    /// Extracts QR code from signed XML.
    /// </summary>
    public static string? ExtractQrCode(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        var root = doc.Root!;

        var qrRef = root.Elements(Cac + "AdditionalDocumentReference")
            .FirstOrDefault(r => GetValue(r, Cbc + "ID") == "QR");

        return qrRef?.Element(Cac + "Attachment")
            ?.Element(Cbc + "EmbeddedDocumentBinaryObject")?.Value;
    }

    private static PartyPdfData MapParty(XElement party)
    {
        var result = new PartyPdfData
        {
            RegistrationName = GetValue(party.Element(Cac + "PartyLegalEntity"), Cbc + "RegistrationName"),
            TaxId = GetValue(party.Element(Cac + "PartyTaxScheme"), Cbc + "CompanyID")
        };

        var partyId = party.Element(Cac + "PartyIdentification")?.Element(Cbc + "ID");
        if (partyId != null)
        {
            result.PartyIdentification = partyId.Attribute("schemeID")?.Value;
            result.PartyIdentificationId = partyId.Value;
        }

        var postalAddress = party.Element(Cac + "PostalAddress");
        if (postalAddress != null)
        {
            result.Address = new AddressPdfData
            {
                Street = GetValue(postalAddress, Cbc + "StreetName"),
                BuildingNumber = GetValue(postalAddress, Cbc + "BuildingNumber"),
                CitySubdivisionName = GetValue(postalAddress, Cbc + "CitySubdivisionName"),
                City = GetValue(postalAddress, Cbc + "CityName"),
                PostalZone = GetValue(postalAddress, Cbc + "PostalZone"),
                Country = GetValue(postalAddress.Element(Cac + "Country"), Cbc + "IdentificationCode")
            };
        }

        var taxScheme = party.Element(Cac + "PartyTaxScheme")?.Element(Cac + "TaxScheme");
        if (taxScheme != null)
        {
            result.TaxScheme = new TaxSchemePdfData
            {
                Id = GetValue(taxScheme, Cbc + "ID")
            };
        }

        return result;
    }

    private static InvoiceLinePdfData MapInvoiceLine(XElement line)
    {
        var result = new InvoiceLinePdfData
        {
            Id = GetValue(line, Cbc + "ID"),
            LineExtensionAmount = GetDecimalValue(line, Cbc + "LineExtensionAmount")
        };

        var quantityElem = line.Element(Cbc + "InvoicedQuantity");
        if (quantityElem != null)
        {
            result.Quantity = decimal.TryParse(quantityElem.Value, out var qty) ? qty : null;
            result.UnitCode = quantityElem.Attribute("unitCode")?.Value;
        }

        // Item
        var item = line.Element(Cac + "Item");
        if (item != null)
        {
            result.Item = new ItemPdfData
            {
                Name = GetValue(item, Cbc + "Name"),
                Description = GetValue(item, Cbc + "Description"),
                ClassifiedTaxCategory = item.Elements(Cac + "ClassifiedTaxCategory")
                    .Select(MapTaxCategory)
                    .ToList()
            };
        }

        // Price
        var price = line.Element(Cac + "Price");
        if (price != null)
        {
            result.Price = new PricePdfData
            {
                Amount = GetDecimalValue(price, Cbc + "PriceAmount"),
                BaseQuantity = GetDecimalValue(price, Cbc + "BaseQuantity")
            };
        }

        // Tax Total
        var taxTotal = line.Element(Cac + "TaxTotal");
        if (taxTotal != null)
        {
            result.TaxTotal = MapTaxTotal(taxTotal);
        }

        // Allowance/Charges
        result.AllowanceCharges = line.Elements(Cac + "AllowanceCharge")
            .Select(MapAllowanceCharge)
            .ToList();

        return result;
    }

    private static TaxTotalPdfData MapTaxTotal(XElement taxTotal)
    {
        return new TaxTotalPdfData
        {
            TaxAmount = GetDecimalValue(taxTotal, Cbc + "TaxAmount"),
            RoundingAmount = GetDecimalValue(taxTotal, Cbc + "RoundingAmount"),
            SubTotals = taxTotal.Elements(Cac + "TaxSubtotal")
                .Select(sub => new TaxSubTotalPdfData
                {
                    TaxableAmount = GetDecimalValue(sub, Cbc + "TaxableAmount"),
                    TaxAmount = GetDecimalValue(sub, Cbc + "TaxAmount"),
                    TaxCategory = MapTaxCategory(sub.Element(Cac + "TaxCategory")!)
                })
                .ToList()
        };
    }

    private static TaxCategoryPdfData MapTaxCategory(XElement taxCategory)
    {
        return new TaxCategoryPdfData
        {
            Id = GetValue(taxCategory, Cbc + "ID"),
            Percent = GetDecimalValue(taxCategory, Cbc + "Percent"),
            TaxScheme = new TaxSchemePdfData
            {
                Id = GetValue(taxCategory.Element(Cac + "TaxScheme"), Cbc + "ID")
            }
        };
    }

    private static AllowanceChargePdfData MapAllowanceCharge(XElement ac)
    {
        return new AllowanceChargePdfData
        {
            ChargeIndicator = GetValue(ac, Cbc + "ChargeIndicator"),
            AllowanceChargeReason = GetValue(ac, Cbc + "AllowanceChargeReason"),
            Amount = GetDecimalValue(ac, Cbc + "Amount")
        };
    }

    private static string? GetValue(XElement? element, XName name)
    {
        return element?.Element(name)?.Value;
    }

    private static decimal? GetDecimalValue(XElement? element, XName name)
    {
        var value = GetValue(element, name);
        return decimal.TryParse(value, out var result) ? result : null;
    }
}
