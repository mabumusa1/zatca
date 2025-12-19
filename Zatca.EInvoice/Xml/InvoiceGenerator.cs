using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Zatca.EInvoice.Xml
{
    /// <summary>
    /// Generates UBL 2.1 XML representation of an invoice for ZATCA e-invoicing.
    /// </summary>
    public class InvoiceGenerator
    {
        // String constants for repeated literals
        private const string TaxAmountKey = "taxAmount";
        private const string TaxAmountElementName = "TaxAmount";
        private const string TaxSchemeKey = "taxScheme";
        private const string TaxTotalKey = "taxTotal";
        private const string SchemeIdKey = "schemeID";
        private const string PercentKey = "percent";
        private const string AmountKey = "amount";
        private const string LineExtensionAmountKey = "lineExtensionAmount";

        private readonly string _currencyId;
        private readonly XNamespace _invoice = UblNamespaces.Invoice;
        private readonly XNamespace _cac = UblNamespaces.Cac;
        private readonly XNamespace _cbc = UblNamespaces.Cbc;
        private readonly XNamespace _ext = UblNamespaces.Ext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceGenerator"/> class.
        /// </summary>
        /// <param name="currencyId">The currency identifier (default is "SAR").</param>
        public InvoiceGenerator(string currencyId = "SAR")
        {
            _currencyId = currencyId ?? "SAR";
        }

        /// <summary>
        /// Generates XML from invoice data dictionary.
        /// </summary>
        /// <param name="invoiceData">The invoice data dictionary.</param>
        /// <returns>A formatted XML string representing the invoice.</returns>
        public string Generate(Dictionary<string, object> invoiceData)
        {
            var invoiceElement = GenerateInvoiceElement(invoiceData);
            return FormatXml(invoiceElement);
        }

        /// <summary>
        /// Generates an XElement from invoice data dictionary.
        /// </summary>
        /// <param name="invoiceData">The invoice data dictionary.</param>
        /// <returns>An <see cref="XElement"/> representing the invoice.</returns>
        public XElement GenerateInvoiceElement(Dictionary<string, object> invoiceData)
        {
            var invoice = new XElement(_invoice + "Invoice",
                new XAttribute(XNamespace.Xmlns + "cac", _cac),
                new XAttribute(XNamespace.Xmlns + "cbc", _cbc),
                new XAttribute(XNamespace.Xmlns + "ext", _ext)
            );

            AddUblExtensions(invoice, invoiceData);
            AddBasicElements(invoice, invoiceData);
            AddInvoiceTypeCode(invoice, invoiceData);
            AddNoteElement(invoice, invoiceData);
            AddCurrencyElements(invoice, invoiceData);
            AddReferenceElements(invoice, invoiceData);
            AddPartyElements(invoice, invoiceData);
            AddDeliveryAndPayment(invoice, invoiceData);
            AddAllowanceCharges(invoice, invoiceData);
            AddTaxAndMonetaryTotals(invoice, invoiceData);
            AddInvoiceLines(invoice, invoiceData);

            return invoice;
        }

        private void AddUblExtensions(XElement invoice, Dictionary<string, object> invoiceData)
        {
            if (!invoiceData.TryGetValue("ublExtensions", out var ublExtensionsValue) || ublExtensionsValue == null)
                return;

            var ublExtensions = GenerateUBLExtensions();
            if (ublExtensions != null)
            {
                invoice.Add(ublExtensions);
            }
        }

        private void AddBasicElements(XElement invoice, Dictionary<string, object> invoiceData)
        {
            invoice.Add(new XElement(_cbc + "ProfileID", GetString(invoiceData, "profileID", "reporting:1.0")));

            if (invoiceData.TryGetValue("id", out _))
                invoice.Add(new XElement(_cbc + "ID", GetString(invoiceData, "id")));

            if (invoiceData.TryGetValue("uuid", out _))
                invoice.Add(new XElement(_cbc + "UUID", GetString(invoiceData, "uuid")));

            if (invoiceData.TryGetValue("issueDate", out var issueDateValue))
                invoice.Add(new XElement(_cbc + "IssueDate", GetDateTime(issueDateValue).ToString("yyyy-MM-dd")));

            if (invoiceData.TryGetValue("issueTime", out var issueTimeValue))
                invoice.Add(new XElement(_cbc + "IssueTime", GetDateTime(issueTimeValue).ToString("HH:mm:ss")));
        }

        private void AddInvoiceTypeCode(XElement invoice, Dictionary<string, object> invoiceData)
        {
            if (!invoiceData.TryGetValue("invoiceType", out var invoiceTypeValue))
                return;

            var invoiceType = TryGetDictionary(invoiceTypeValue);
            if (invoiceType == null)
                return;

            var typeCode = GetInvoiceTypeCode(invoiceType);
            var invoiceTypeName = GetInvoiceTypeName(invoiceType);

            invoice.Add(new XElement(_cbc + "InvoiceTypeCode",
                new XAttribute("name", invoiceTypeName),
                typeCode));
        }

        private static string GetInvoiceTypeCode(Dictionary<string, object> invoiceType)
        {
            var typeCode = GetString(invoiceType, "typeCode", "") != ""
                ? GetString(invoiceType, "typeCode")
                : GetString(invoiceType, "type", "388");

            if (int.TryParse(typeCode, out _))
                return typeCode;

            return typeCode.ToLowerInvariant() switch
            {
                "credit" => "381",
                "debit" => "383",
                _ => "388"
            };
        }

        private static string GetInvoiceTypeName(Dictionary<string, object> invoiceType)
        {
            return GetString(invoiceType, "name", "") != ""
                ? GetString(invoiceType, "name")
                : GetString(invoiceType, "invoice", "0100000");
        }

        private void AddNoteElement(XElement invoice, Dictionary<string, object> invoiceData)
        {
            if (!invoiceData.TryGetValue("note", out _) || string.IsNullOrWhiteSpace(GetString(invoiceData, "note")))
                return;

            invoice.Add(new XElement(_cbc + "Note",
                new XAttribute("languageID", GetString(invoiceData, "languageID", "en")),
                GetString(invoiceData, "note")));
        }

        private void AddCurrencyElements(XElement invoice, Dictionary<string, object> invoiceData)
        {
            invoice.Add(new XElement(_cbc + "DocumentCurrencyCode", GetString(invoiceData, "currencyCode", _currencyId)));
            invoice.Add(new XElement(_cbc + "TaxCurrencyCode", GetString(invoiceData, "taxCurrencyCode", _currencyId)));
        }

        private void AddReferenceElements(XElement invoice, Dictionary<string, object> invoiceData)
        {
            AddOptionalElement(invoice, invoiceData, "orderReference", GenerateOrderReference);
            AddListElements(invoice, invoiceData, "billingReferences", GenerateBillingReference);
            AddOptionalElement(invoice, invoiceData, "contract", GenerateContractReference);
            AddListElements(invoice, invoiceData, "additionalDocuments", GenerateAdditionalDocumentReference);
            AddOptionalElement(invoice, invoiceData, "signature", GenerateSignature);
        }

        private void AddPartyElements(XElement invoice, Dictionary<string, object> invoiceData)
        {
            AddOptionalElement(invoice, invoiceData, "supplier", GenerateAccountingSupplierParty);
            AddOptionalElement(invoice, invoiceData, "customer", GenerateAccountingCustomerParty);
        }

        private void AddDeliveryAndPayment(XElement invoice, Dictionary<string, object> invoiceData)
        {
            if (invoiceData.TryGetValue("delivery", out var deliveryValue))
            {
                var delivery = TryGetDictionary(deliveryValue);
                var deliveryElement = delivery != null ? GenerateDelivery(delivery) : null;
                if (deliveryElement != null)
                    invoice.Add(deliveryElement);
            }

            AddOptionalElement(invoice, invoiceData, "paymentMeans", GeneratePaymentMeans);
        }

        private void AddAllowanceCharges(XElement invoice, Dictionary<string, object> invoiceData)
        {
            AddListElements(invoice, invoiceData, "allowanceCharges", GenerateAllowanceCharge);
        }

        private void AddTaxAndMonetaryTotals(XElement invoice, Dictionary<string, object> invoiceData)
        {
            if (invoiceData.TryGetValue(TaxTotalKey, out var taxTotalValue))
            {
                var taxTotal = TryGetDictionary(taxTotalValue);
                if (taxTotal != null)
                {
                    AddTaxAmountElement(invoice, taxTotal);
                    invoice.Add(GenerateTaxTotal(taxTotal));
                }
            }

            AddOptionalElement(invoice, invoiceData, "legalMonetaryTotal", GenerateLegalMonetaryTotal);
        }

        private void AddTaxAmountElement(XElement invoice, Dictionary<string, object> taxTotal)
        {
            if (!taxTotal.TryGetValue(TaxAmountKey, out var taxAmountValue))
                return;

            invoice.Add(new XElement(_cac + "TaxTotal",
                XmlSerializationExtensions.CreateAmountElement(_cbc + TaxAmountElementName, GetDecimal(taxAmountValue), _currencyId)));
        }

        private void AddInvoiceLines(XElement invoice, Dictionary<string, object> invoiceData)
        {
            AddListElements(invoice, invoiceData, "invoiceLines", GenerateInvoiceLine);
        }

        private void AddOptionalElement(XElement invoice, Dictionary<string, object> invoiceData, string key, Func<Dictionary<string, object>, XElement> generator)
        {
            if (!invoiceData.TryGetValue(key, out var value))
                return;

            var dict = TryGetDictionary(value);
            if (dict != null)
                invoice.Add(generator(dict));
        }

        private void AddListElements(XElement invoice, Dictionary<string, object> invoiceData, string key, Func<Dictionary<string, object>, XElement> generator)
        {
            if (!invoiceData.TryGetValue(key, out var value))
                return;

            var list = TryGetList(value);
            if (list == null)
                return;

            foreach (var item in list)
            {
                var dict = TryGetDictionary(item);
                if (dict != null)
                    invoice.Add(generator(dict));
            }
        }

        private XElement GenerateUBLExtensions()
        {
            // Placeholder for UBL Extensions - would need full implementation based on requirements
            return new XElement(_ext + "UBLExtensions");
        }

        private XElement GenerateOrderReference(Dictionary<string, object> orderRef)
        {
            var element = new XElement(_cac + "OrderReference");

            if (orderRef.TryGetValue("id", out _))
            {
                element.Add(new XElement(_cbc + "ID", GetString(orderRef, "id")));
            }

            return element;
        }

        private XElement GenerateBillingReference(Dictionary<string, object> billingRef)
        {
            var element = new XElement(_cac + "BillingReference");

            if (billingRef.TryGetValue("invoiceDocumentReference", out var invoiceDocRefValue))
            {
                var invoiceDocRef = TryGetDictionary(invoiceDocRefValue);
                if (invoiceDocRef != null)
                {
                    var invoiceDocRefElement = new XElement(_cac + "InvoiceDocumentReference");

                    if (invoiceDocRef.TryGetValue("id", out _))
                    {
                        invoiceDocRefElement.Add(new XElement(_cbc + "ID", GetString(invoiceDocRef, "id")));
                    }

                    element.Add(invoiceDocRefElement);
                }
            }
            // Also support direct ID for billing reference
            else if (billingRef.TryGetValue("id", out _))
            {
                var invoiceDocRefElement = new XElement(_cac + "InvoiceDocumentReference");
                invoiceDocRefElement.Add(new XElement(_cbc + "ID", GetString(billingRef, "id")));
                element.Add(invoiceDocRefElement);
            }

            return element;
        }

        private XElement GenerateContractReference(Dictionary<string, object> contract)
        {
            var element = new XElement(_cac + "ContractDocumentReference");

            if (contract.TryGetValue("id", out _))
            {
                element.Add(new XElement(_cbc + "ID", GetString(contract, "id")));
            }

            return element;
        }

        private XElement GenerateAdditionalDocumentReference(Dictionary<string, object> doc)
        {
            var element = new XElement(_cac + "AdditionalDocumentReference");

            if (doc.TryGetValue("id", out _))
            {
                element.Add(new XElement(_cbc + "ID", GetString(doc, "id")));
            }

            if (doc.TryGetValue("uuid", out _))
            {
                element.Add(new XElement(_cbc + "UUID", GetString(doc, "uuid")));
            }

            if (doc.TryGetValue("attachment", out var attachmentValue))
            {
                var attachment = TryGetDictionary(attachmentValue);
                if (attachment != null)
                {
                    var attachmentElement = new XElement(_cac + "Attachment");

                    if (attachment.TryGetValue("embeddedDocument", out _))
                    {
                        var embeddedDoc = GetString(attachment, "embeddedDocument");
                        var mimeCode = GetString(attachment, "mimeCode", "text/plain");

                        attachmentElement.Add(new XElement(_cbc + "EmbeddedDocumentBinaryObject",
                            new XAttribute("mimeCode", mimeCode),
                            embeddedDoc));
                    }

                    element.Add(attachmentElement);
                }
            }

            return element;
        }

        private XElement GenerateSignature(Dictionary<string, object> signature)
        {
            var element = new XElement(_cac + "Signature");

            if (signature.TryGetValue("id", out _))
            {
                element.Add(new XElement(_cbc + "ID", GetString(signature, "id")));
            }

            if (signature.TryGetValue("signatureMethod", out _))
            {
                element.Add(new XElement(_cbc + "SignatureMethod", GetString(signature, "signatureMethod")));
            }

            return element;
        }

        private XElement GenerateAccountingSupplierParty(Dictionary<string, object> supplier)
        {
            var element = new XElement(_cac + "AccountingSupplierParty",
                new XElement(_cac + "Party",
                    GenerateParty(supplier)
                )
            );

            return element;
        }

        private XElement GenerateAccountingCustomerParty(Dictionary<string, object> customer)
        {
            var element = new XElement(_cac + "AccountingCustomerParty");

            // For simplified invoices, customer may be empty but element is still required
            if (customer.Count > 0)
            {
                element.Add(new XElement(_cac + "Party", GenerateParty(customer)));
            }

            return element;
        }

        private XElement[] GenerateParty(Dictionary<string, object> party)
        {
            var elements = new List<XElement>();

            AddPartyIdentification(elements, party);
            AddPartyAddress(elements, party);
            AddPartyTaxScheme(elements, party);
            AddPartyLegalEntity(elements, party);

            return elements.ToArray();
        }

        private void AddPartyIdentification(List<XElement> elements, Dictionary<string, object> party)
        {
            if (!party.TryGetValue("partyIdentification", out _) || string.IsNullOrWhiteSpace(GetString(party, "partyIdentification")))
                return;

            var idElement = new XElement(_cbc + "ID", GetString(party, "partyIdentification"));
            if (party.TryGetValue("partyIdentificationId", out _))
                idElement.Add(new XAttribute("schemeID", GetString(party, "partyIdentificationId")));

            var partyIdElement = new XElement(_cac + "PartyIdentification", idElement);
            elements.Add(partyIdElement);
        }

        private void AddPartyAddress(List<XElement> elements, Dictionary<string, object> party)
        {
            if (!party.TryGetValue("address", out var addressValue))
                return;

            var address = TryGetDictionary(addressValue);
            if (address != null)
                elements.Add(GenerateAddress(address));
        }

        private void AddPartyTaxScheme(List<XElement> elements, Dictionary<string, object> party)
        {
            if (!party.TryGetValue("taxId", out _) && !party.TryGetValue("taxScheme", out _))
                return;

            var partyTaxScheme = new XElement(_cac + "PartyTaxScheme");

            if (party.TryGetValue("taxId", out _))
                partyTaxScheme.Add(new XElement(_cbc + "CompanyID", GetString(party, "taxId")));

            partyTaxScheme.Add(new XElement(_cac + "TaxScheme",
                new XElement(_cbc + "ID", GetPartyTaxSchemeId(party))));

            elements.Add(partyTaxScheme);
        }

        private string GetPartyTaxSchemeId(Dictionary<string, object> party)
        {
            if (!party.TryGetValue(TaxSchemeKey, out var taxSchemeValue))
                return "VAT";

            var taxScheme = TryGetDictionary(taxSchemeValue);
            if (taxScheme != null && taxScheme.TryGetValue("id", out _))
                return GetString(taxScheme, "id");

            return "VAT";
        }

        private void AddPartyLegalEntity(List<XElement> elements, Dictionary<string, object> party)
        {
            if (!party.TryGetValue("registrationName", out _))
                return;

            elements.Add(new XElement(_cac + "PartyLegalEntity",
                new XElement(_cbc + "RegistrationName", GetString(party, "registrationName"))));
        }

        private XElement GenerateAddress(Dictionary<string, object> address)
        {
            var element = new XElement(_cac + "PostalAddress");

            if (address.TryGetValue("street", out _))
            {
                element.Add(new XElement(_cbc + "StreetName", GetString(address, "street")));
            }

            if (address.TryGetValue("additionalStreet", out _))
            {
                element.Add(new XElement(_cbc + "AdditionalStreetName", GetString(address, "additionalStreet")));
            }

            if (address.TryGetValue("buildingNumber", out _))
            {
                element.Add(new XElement(_cbc + "BuildingNumber", GetString(address, "buildingNumber")));
            }

            if (address.TryGetValue("plotIdentification", out _))
            {
                element.Add(new XElement(_cbc + "PlotIdentification", GetString(address, "plotIdentification")));
            }

            // Support both citySubdivisionName and subdivision keys
            if (address.TryGetValue("citySubdivisionName", out _))
            {
                element.Add(new XElement(_cbc + "CitySubdivisionName", GetString(address, "citySubdivisionName")));
            }
            else if (address.TryGetValue("subdivision", out _))
            {
                element.Add(new XElement(_cbc + "CitySubdivisionName", GetString(address, "subdivision")));
            }

            if (address.TryGetValue("city", out _))
            {
                element.Add(new XElement(_cbc + "CityName", GetString(address, "city")));
            }

            if (address.TryGetValue("postalZone", out _))
            {
                element.Add(new XElement(_cbc + "PostalZone", GetString(address, "postalZone")));
            }

            if (address.TryGetValue("countrySubentity", out _))
            {
                element.Add(new XElement(_cbc + "CountrySubentity", GetString(address, "countrySubentity")));
            }

            if (address.TryGetValue("country", out _))
            {
                element.Add(new XElement(_cac + "Country",
                    new XElement(_cbc + "IdentificationCode", GetString(address, "country"))
                ));
            }

            return element;
        }

        private XElement? GenerateDelivery(Dictionary<string, object> delivery)
        {
            var element = new XElement(_cac + "Delivery");
            bool hasContent = false;

            if (delivery.TryGetValue("actualDeliveryDate", out var actualDeliveryDateValue))
            {
                var date = GetDateTime(actualDeliveryDateValue);
                element.Add(new XElement(_cbc + "ActualDeliveryDate", date.ToString("yyyy-MM-dd")));
                hasContent = true;
            }

            if (delivery.TryGetValue("latestDeliveryDate", out var latestDeliveryDateValue))
            {
                var date = GetDateTime(latestDeliveryDateValue);
                element.Add(new XElement(_cbc + "LatestDeliveryDate", date.ToString("yyyy-MM-dd")));
                hasContent = true;
            }

            return hasContent ? element : null;
        }

        private XElement GeneratePaymentMeans(Dictionary<string, object> paymentMeans)
        {
            var element = new XElement(_cac + "PaymentMeans");

            if (paymentMeans.TryGetValue("code", out _))
            {
                element.Add(new XElement(_cbc + "PaymentMeansCode", GetString(paymentMeans, "code")));
            }

            if (paymentMeans.TryGetValue("instructionNote", out _))
            {
                element.Add(new XElement(_cbc + "InstructionNote", GetString(paymentMeans, "instructionNote")));
            }

            return element;
        }

        private XElement GenerateAllowanceCharge(Dictionary<string, object> ac)
        {
            var element = new XElement(_cac + "AllowanceCharge");

            if (ac.TryGetValue("chargeIndicator", out _))
                element.Add(new XElement(_cbc + "ChargeIndicator", GetString(ac, "chargeIndicator")));

            if (ac.TryGetValue("allowanceChargeReason", out _))
                element.Add(new XElement(_cbc + "AllowanceChargeReason", GetString(ac, "allowanceChargeReason")));

            if (ac.TryGetValue(AmountKey, out var amountValue))
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "Amount", GetDecimal(amountValue), _currencyId));

            AddAllowanceChargeTaxCategories(element, ac);

            return element;
        }

        private void AddAllowanceChargeTaxCategories(XElement element, Dictionary<string, object> ac)
        {
            if (!ac.TryGetValue("taxCategories", out var taxCategoriesValue))
                return;

            var taxCategories = TryGetList(taxCategoriesValue);
            if (taxCategories == null)
                return;

            foreach (var category in taxCategories)
            {
                var categoryDict = TryGetDictionary(category);
                if (categoryDict != null)
                    element.Add(GenerateAllowanceChargeTaxCategory(categoryDict));
            }
        }

        private XElement GenerateAllowanceChargeTaxCategory(Dictionary<string, object> categoryDict)
        {
            var taxCategoryElement = new XElement(_cac + "TaxCategory");

            if (categoryDict.TryGetValue("id", out _))
            {
                taxCategoryElement.Add(new XElement(_cbc + "ID",
                    new XAttribute(SchemeIdKey, "UN/ECE 5305"),
                    new XAttribute("schemeAgencyID", "6"),
                    GetString(categoryDict, "id")));
            }

            if (categoryDict.TryGetValue(PercentKey, out var percentValue))
            {
                taxCategoryElement.Add(new XElement(_cbc + "Percent",
                    GetDecimal(percentValue).ToString("F2")));
            }

            AddTaxSchemeElement(taxCategoryElement, categoryDict, "UN/ECE 5153");

            return taxCategoryElement;
        }

        private void AddTaxSchemeElement(XElement parent, Dictionary<string, object> dict, string schemeId)
        {
            if (!dict.TryGetValue(TaxSchemeKey, out var taxSchemeValue))
                return;

            var taxScheme = TryGetDictionary(taxSchemeValue);
            if (taxScheme == null)
                return;

            var taxSchemeElement = new XElement(_cac + "TaxScheme");
            if (taxScheme.TryGetValue("id", out _))
            {
                taxSchemeElement.Add(new XElement(_cbc + "ID",
                    new XAttribute(SchemeIdKey, schemeId),
                    new XAttribute("schemeAgencyID", "6"),
                    GetString(taxScheme, "id")));
            }
            parent.Add(taxSchemeElement);
        }

        private XElement GenerateTaxTotal(Dictionary<string, object> taxTotal)
        {
            var element = new XElement(_cac + "TaxTotal");

            if (taxTotal.TryGetValue(TaxAmountKey, out var taxAmountValue))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + TaxAmountElementName, GetDecimal(taxAmountValue), _currencyId));
            }

            if (taxTotal.TryGetValue("subTotals", out var subTotalsValue))
            {
                var subTotals = TryGetList(subTotalsValue);
                if (subTotals != null)
                {
                    foreach (var subTotal in subTotals)
                    {
                        var subTotalDict = TryGetDictionary(subTotal);
                        if (subTotalDict != null)
                        {
                            element.Add(GenerateTaxSubTotal(subTotalDict));
                        }
                    }
                }
            }

            return element;
        }

        private XElement GenerateTaxSubTotal(Dictionary<string, object> subTotal)
        {
            var element = new XElement(_cac + "TaxSubtotal");

            if (subTotal.TryGetValue("taxableAmount", out var taxableAmountValue))
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxableAmount", GetDecimal(taxableAmountValue), _currencyId));

            if (subTotal.TryGetValue(TaxAmountKey, out var taxAmountValue))
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + TaxAmountElementName, GetDecimal(taxAmountValue), _currencyId));

            AddSubTotalTaxCategory(element, subTotal);

            return element;
        }

        private void AddSubTotalTaxCategory(XElement element, Dictionary<string, object> subTotal)
        {
            if (!subTotal.TryGetValue("taxCategory", out var taxCategoryValue))
                return;

            var taxCategory = TryGetDictionary(taxCategoryValue);
            if (taxCategory == null)
                return;

            var taxCategoryElement = GenerateSubTotalTaxCategoryElement(taxCategory);
            element.Add(taxCategoryElement);
        }

        private XElement GenerateSubTotalTaxCategoryElement(Dictionary<string, object> taxCategory)
        {
            var taxCategoryElement = new XElement(_cac + "TaxCategory");

            if (taxCategory.TryGetValue("id", out _))
            {
                taxCategoryElement.Add(new XElement(_cbc + "ID",
                    new XAttribute(SchemeIdKey, "UN/ECE 5305"),
                    new XAttribute("schemeAgencyID", "6"),
                    GetString(taxCategory, "id")));
            }

            if (taxCategory.TryGetValue(PercentKey, out var percentValue))
                taxCategoryElement.Add(new XElement(_cbc + "Percent", GetDecimal(percentValue).FormatPercent()));

            if (taxCategory.TryGetValue("taxExemptionReasonCode", out _))
                taxCategoryElement.Add(new XElement(_cbc + "TaxExemptionReasonCode", GetString(taxCategory, "taxExemptionReasonCode")));

            if (taxCategory.TryGetValue("taxExemptionReason", out _))
                taxCategoryElement.Add(new XElement(_cbc + "TaxExemptionReason", GetString(taxCategory, "taxExemptionReason")));

            AddTaxSchemeElement(taxCategoryElement, taxCategory, "UN/ECE 5153");

            return taxCategoryElement;
        }

        private XElement GenerateLegalMonetaryTotal(Dictionary<string, object> lmt)
        {
            var element = new XElement(_cac + "LegalMonetaryTotal");

            if (lmt.TryGetValue(LineExtensionAmountKey, out var lineExtensionAmountValue))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "LineExtensionAmount", GetDecimal(lineExtensionAmountValue), _currencyId));
            }

            if (lmt.TryGetValue("taxExclusiveAmount", out var taxExclusiveAmountValue))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxExclusiveAmount", GetDecimal(taxExclusiveAmountValue), _currencyId));
            }

            if (lmt.TryGetValue("taxInclusiveAmount", out var taxInclusiveAmountValue))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxInclusiveAmount", GetDecimal(taxInclusiveAmountValue), _currencyId));
            }

            if (lmt.TryGetValue("allowanceTotalAmount", out var allowanceTotalAmountValue))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "AllowanceTotalAmount", GetDecimal(allowanceTotalAmountValue), _currencyId));
            }

            if (lmt.TryGetValue("prepaidAmount", out var prepaidAmountValue))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "PrepaidAmount", GetDecimal(prepaidAmountValue), _currencyId));
            }

            if (lmt.TryGetValue("payableAmount", out var payableAmountValue))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "PayableAmount", GetDecimal(payableAmountValue), _currencyId));
            }

            return element;
        }

        private XElement GenerateInvoiceLine(Dictionary<string, object> line)
        {
            var element = new XElement(_cac + "InvoiceLine");

            if (line.TryGetValue("id", out _))
                element.Add(new XElement(_cbc + "ID", GetString(line, "id")));

            if (line.TryGetValue("note", out _))
                element.Add(new XElement(_cbc + "Note", GetString(line, "note")));

            if (line.TryGetValue("quantity", out var quantityValue))
                element.Add(XmlSerializationExtensions.CreateQuantityElement(_cbc + "InvoicedQuantity", GetDecimal(quantityValue), GetString(line, "unitCode", "PCE")));

            if (line.TryGetValue(LineExtensionAmountKey, out var lineExtensionAmountValue))
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "LineExtensionAmount", GetDecimal(lineExtensionAmountValue), _currencyId));

            AddLineTaxTotal(element, line);
            AddOptionalElement(element, line, "item", GenerateItem);
            AddOptionalElement(element, line, "price", GeneratePrice);

            return element;
        }

        private void AddLineTaxTotal(XElement element, Dictionary<string, object> line)
        {
            if (!line.TryGetValue(TaxTotalKey, out var taxTotalValue))
                return;

            var taxTotal = TryGetDictionary(taxTotalValue);
            if (taxTotal == null)
                return;

            var taxTotalElement = new XElement(_cac + "TaxTotal");

            if (taxTotal.TryGetValue(TaxAmountKey, out var taxAmountValue))
                taxTotalElement.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + TaxAmountElementName, GetDecimal(taxAmountValue), _currencyId));

            if (taxTotal.TryGetValue("roundingAmount", out var roundingAmountValue))
                taxTotalElement.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "RoundingAmount", GetDecimal(roundingAmountValue), _currencyId));

            element.Add(taxTotalElement);
        }

        private XElement GenerateItem(Dictionary<string, object> item)
        {
            var element = new XElement(_cac + "Item");

            if (item.TryGetValue("name", out _))
                element.Add(new XElement(_cbc + "Name", GetString(item, "name")));

            AddClassifiedTaxCategories(element, item);

            return element;
        }

        private void AddClassifiedTaxCategories(XElement element, Dictionary<string, object> item)
        {
            if (!item.TryGetValue("classifiedTaxCategory", out var classifiedTaxCategoryValue))
                return;

            var classifiedTaxCategories = TryGetList(classifiedTaxCategoryValue);
            if (classifiedTaxCategories == null)
                return;

            foreach (var category in classifiedTaxCategories)
            {
                var categoryDict = TryGetDictionary(category);
                if (categoryDict != null)
                    element.Add(GenerateClassifiedTaxCategory(categoryDict));
            }
        }

        private XElement GenerateClassifiedTaxCategory(Dictionary<string, object> categoryDict)
        {
            var taxCategoryElement = new XElement(_cac + "ClassifiedTaxCategory");

            if (categoryDict.TryGetValue("id", out _))
                taxCategoryElement.Add(new XElement(_cbc + "ID", GetString(categoryDict, "id")));

            if (categoryDict.TryGetValue(PercentKey, out var percentValue))
                taxCategoryElement.Add(new XElement(_cbc + "Percent", GetDecimal(percentValue).FormatPercent()));

            AddSimpleTaxScheme(taxCategoryElement, categoryDict);

            return taxCategoryElement;
        }

        private void AddSimpleTaxScheme(XElement parent, Dictionary<string, object> dict)
        {
            if (!dict.TryGetValue(TaxSchemeKey, out var taxSchemeValue))
                return;

            var taxScheme = TryGetDictionary(taxSchemeValue);
            if (taxScheme == null)
                return;

            var taxSchemeElement = new XElement(_cac + "TaxScheme");
            if (taxScheme.TryGetValue("id", out _))
                taxSchemeElement.Add(new XElement(_cbc + "ID", GetString(taxScheme, "id")));

            parent.Add(taxSchemeElement);
        }

        private XElement GeneratePrice(Dictionary<string, object> price)
        {
            var element = new XElement(_cac + "Price");

            if (price.TryGetValue(AmountKey, out var amountValue))
            {
                element.Add(XmlSerializationExtensions.CreatePriceElement(_cbc + "PriceAmount", GetDecimal(amountValue), _currencyId));
            }

            if (price.TryGetValue("baseQuantity", out var baseQuantityValue))
            {
                var unitCode = GetString(price, "baseQuantityUnitCode", "PCE");
                element.Add(XmlSerializationExtensions.CreateQuantityElement(_cbc + "BaseQuantity", GetDecimal(baseQuantityValue), unitCode));
            }

            return element;
        }

        // Helper methods
        private static string GetString(Dictionary<string, object> dict, string key, string defaultValue = "")
        {
            if (dict.TryGetValue(key, out var value) && value != null)
            {
                if (value is JsonElement jsonElement)
                {
                    return jsonElement.ValueKind == JsonValueKind.String
                        ? jsonElement.GetString() ?? defaultValue
                        : jsonElement.ToString();
                }
                return value.ToString() ?? defaultValue;
            }
            return defaultValue;
        }

        private static decimal GetDecimal(object value)
        {
            if (value == null)
            {
                return 0m;
            }

            if (value is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.Number => jsonElement.GetDecimal(),
                    JsonValueKind.String when decimal.TryParse(jsonElement.GetString(), out var parsed) => parsed,
                    _ => 0m
                };
            }

            if (value is decimal decValue)
            {
                return decValue;
            }

            if (value is int i)
            {
                return i;
            }

            if (value is long l)
            {
                return l;
            }

            if (value is float f)
            {
                return (decimal)f;
            }

            if (value is double dbl)
            {
                return (decimal)dbl;
            }

            if (value is string str && decimal.TryParse(str, out decimal result))
            {
                return result;
            }

            return 0m;
        }

        private static DateTime GetDateTime(object value)
        {
            if (value is DateTime dt)
            {
                return dt;
            }

            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(jsonElement.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime jsonResult))
            {
                return jsonResult;
            }

            if (value is string str && DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Tries to get a dictionary from an object, handling JsonElement conversion.
        /// </summary>
        private Dictionary<string, object>? TryGetDictionary(object value)
        {
            if (value is Dictionary<string, object> dict)
            {
                return dict;
            }

            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
            {
                return JsonElementToDictionary(jsonElement);
            }

            return null;
        }

        /// <summary>
        /// Tries to get a list from an object, handling JsonElement conversion.
        /// </summary>
        private List<object>? TryGetList(object value)
        {
            if (value is IList<object> list)
            {
                return list.ToList();
            }

            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                var result = new List<object>();
                foreach (var item in jsonElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        result.Add(JsonElementToDictionary(item));
                    }
                    else
                    {
                        result.Add(item);
                    }
                }
                return result;
            }

            return null;
        }

        /// <summary>
        /// Converts a JsonElement to a Dictionary.
        /// </summary>
        private Dictionary<string, object> JsonElementToDictionary(JsonElement element)
        {
            var dict = new Dictionary<string, object>();
            foreach (var property in element.EnumerateObject())
            {
                dict[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.Object => JsonElementToDictionary(property.Value),
                    JsonValueKind.Array => JsonElementToList(property.Value),
                    _ => property.Value
                };
            }
            return dict;
        }

        /// <summary>
        /// Converts a JsonElement array to a List.
        /// </summary>
        private List<object> JsonElementToList(JsonElement element)
        {
            var list = new List<object>();
            foreach (var item in element.EnumerateArray())
            {
                list.Add(item.ValueKind switch
                {
                    JsonValueKind.Object => JsonElementToDictionary(item),
                    JsonValueKind.Array => JsonElementToList(item),
                    _ => item
                });
            }
            return list;
        }

        /// <summary>
        /// Formats XML with proper indentation.
        /// </summary>
        /// <param name="element">The XML element to format.</param>
        /// <returns>A formatted XML string.</returns>
        private static string FormatXml(XElement element)
        {
            // Use UTF8 encoding WITHOUT BOM to avoid hash computation issues
            var utf8NoBom = new UTF8Encoding(false);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = utf8NoBom,
                OmitXmlDeclaration = false
            };

            // Use MemoryStream with UTF8 without BOM
            using (var memoryStream = new System.IO.MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                {
                    element.WriteTo(xmlWriter);
                    xmlWriter.Flush();
                }
                return utf8NoBom.GetString(memoryStream.ToArray());
            }
        }
    }
}
