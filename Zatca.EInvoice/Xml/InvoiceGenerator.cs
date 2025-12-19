using System;
using System.Collections.Generic;
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
            if (!invoiceData.ContainsKey("ublExtensions") || invoiceData["ublExtensions"] == null)
                return;

            var ublExtensions = GenerateUBLExtensions(invoiceData["ublExtensions"]);
            if (ublExtensions != null)
            {
                invoice.Add(ublExtensions);
            }
        }

        private void AddBasicElements(XElement invoice, Dictionary<string, object> invoiceData)
        {
            invoice.Add(new XElement(_cbc + "ProfileID", GetString(invoiceData, "profileID", "reporting:1.0")));

            if (invoiceData.ContainsKey("id"))
                invoice.Add(new XElement(_cbc + "ID", GetString(invoiceData, "id")));

            if (invoiceData.ContainsKey("uuid"))
                invoice.Add(new XElement(_cbc + "UUID", GetString(invoiceData, "uuid")));

            if (invoiceData.ContainsKey("issueDate"))
                invoice.Add(new XElement(_cbc + "IssueDate", GetDateTime(invoiceData["issueDate"]).ToString("yyyy-MM-dd")));

            if (invoiceData.ContainsKey("issueTime"))
                invoice.Add(new XElement(_cbc + "IssueTime", GetDateTime(invoiceData["issueTime"]).ToString("HH:mm:ss")));
        }

        private void AddInvoiceTypeCode(XElement invoice, Dictionary<string, object> invoiceData)
        {
            if (!invoiceData.ContainsKey("invoiceType"))
                return;

            var invoiceType = TryGetDictionary(invoiceData["invoiceType"]);
            if (invoiceType == null)
                return;

            var typeCode = GetInvoiceTypeCode(invoiceType);
            var invoiceTypeName = GetInvoiceTypeName(invoiceType);

            invoice.Add(new XElement(_cbc + "InvoiceTypeCode",
                new XAttribute("name", invoiceTypeName),
                typeCode));
        }

        private string GetInvoiceTypeCode(Dictionary<string, object> invoiceType)
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

        private string GetInvoiceTypeName(Dictionary<string, object> invoiceType)
        {
            return GetString(invoiceType, "name", "") != ""
                ? GetString(invoiceType, "name")
                : GetString(invoiceType, "invoice", "0100000");
        }

        private void AddNoteElement(XElement invoice, Dictionary<string, object> invoiceData)
        {
            if (!invoiceData.ContainsKey("note") || string.IsNullOrWhiteSpace(GetString(invoiceData, "note")))
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
            if (invoiceData.ContainsKey("delivery"))
            {
                var delivery = TryGetDictionary(invoiceData["delivery"]);
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
            if (invoiceData.ContainsKey("taxTotal"))
            {
                var taxTotal = TryGetDictionary(invoiceData["taxTotal"]);
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
            if (!taxTotal.ContainsKey("taxAmount"))
                return;

            invoice.Add(new XElement(_cac + "TaxTotal",
                XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxAmount", GetDecimal(taxTotal["taxAmount"]), _currencyId)));
        }

        private void AddInvoiceLines(XElement invoice, Dictionary<string, object> invoiceData)
        {
            AddListElements(invoice, invoiceData, "invoiceLines", GenerateInvoiceLine);
        }

        private void AddOptionalElement(XElement invoice, Dictionary<string, object> invoiceData, string key, Func<Dictionary<string, object>, XElement> generator)
        {
            if (!invoiceData.ContainsKey(key))
                return;

            var dict = TryGetDictionary(invoiceData[key]);
            if (dict != null)
                invoice.Add(generator(dict));
        }

        private void AddListElements(XElement invoice, Dictionary<string, object> invoiceData, string key, Func<Dictionary<string, object>, XElement> generator)
        {
            if (!invoiceData.ContainsKey(key))
                return;

            var list = TryGetList(invoiceData[key]);
            if (list == null)
                return;

            foreach (var item in list)
            {
                var dict = TryGetDictionary(item);
                if (dict != null)
                    invoice.Add(generator(dict));
            }
        }

        private XElement GenerateUBLExtensions(object ublExtensions)
        {
            // Placeholder for UBL Extensions - would need full implementation based on requirements
            return new XElement(_ext + "UBLExtensions");
        }

        private XElement GenerateOrderReference(Dictionary<string, object> orderRef)
        {
            var element = new XElement(_cac + "OrderReference");

            if (orderRef.ContainsKey("id"))
            {
                element.Add(new XElement(_cbc + "ID", GetString(orderRef, "id")));
            }

            return element;
        }

        private XElement GenerateBillingReference(Dictionary<string, object> billingRef)
        {
            var element = new XElement(_cac + "BillingReference");

            if (billingRef.ContainsKey("invoiceDocumentReference"))
            {
                var invoiceDocRef = TryGetDictionary(billingRef["invoiceDocumentReference"]);
                if (invoiceDocRef != null)
                {
                    var invoiceDocRefElement = new XElement(_cac + "InvoiceDocumentReference");

                    if (invoiceDocRef.ContainsKey("id"))
                    {
                        invoiceDocRefElement.Add(new XElement(_cbc + "ID", GetString(invoiceDocRef, "id")));
                    }

                    element.Add(invoiceDocRefElement);
                }
            }
            // Also support direct ID for billing reference
            else if (billingRef.ContainsKey("id"))
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

            if (contract.ContainsKey("id"))
            {
                element.Add(new XElement(_cbc + "ID", GetString(contract, "id")));
            }

            return element;
        }

        private XElement GenerateAdditionalDocumentReference(Dictionary<string, object> doc)
        {
            var element = new XElement(_cac + "AdditionalDocumentReference");

            if (doc.ContainsKey("id"))
            {
                element.Add(new XElement(_cbc + "ID", GetString(doc, "id")));
            }

            if (doc.ContainsKey("uuid"))
            {
                element.Add(new XElement(_cbc + "UUID", GetString(doc, "uuid")));
            }

            if (doc.ContainsKey("attachment"))
            {
                var attachment = TryGetDictionary(doc["attachment"]);
                if (attachment != null)
                {
                    var attachmentElement = new XElement(_cac + "Attachment");

                    if (attachment.ContainsKey("embeddedDocument"))
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

            if (signature.ContainsKey("id"))
            {
                element.Add(new XElement(_cbc + "ID", GetString(signature, "id")));
            }

            if (signature.ContainsKey("signatureMethod"))
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
            if (!party.ContainsKey("partyIdentification") || string.IsNullOrWhiteSpace(GetString(party, "partyIdentification")))
                return;

            var idElement = new XElement(_cbc + "ID", GetString(party, "partyIdentification"));
            if (party.ContainsKey("partyIdentificationId"))
                idElement.Add(new XAttribute("schemeID", GetString(party, "partyIdentificationId")));

            var partyIdElement = new XElement(_cac + "PartyIdentification", idElement);
            elements.Add(partyIdElement);
        }

        private void AddPartyAddress(List<XElement> elements, Dictionary<string, object> party)
        {
            if (!party.ContainsKey("address"))
                return;

            var address = TryGetDictionary(party["address"]);
            if (address != null)
                elements.Add(GenerateAddress(address));
        }

        private void AddPartyTaxScheme(List<XElement> elements, Dictionary<string, object> party)
        {
            if (!party.ContainsKey("taxId") && !party.ContainsKey("taxScheme"))
                return;

            var partyTaxScheme = new XElement(_cac + "PartyTaxScheme");

            if (party.ContainsKey("taxId"))
                partyTaxScheme.Add(new XElement(_cbc + "CompanyID", GetString(party, "taxId")));

            partyTaxScheme.Add(new XElement(_cac + "TaxScheme",
                new XElement(_cbc + "ID", GetPartyTaxSchemeId(party))));

            elements.Add(partyTaxScheme);
        }

        private string GetPartyTaxSchemeId(Dictionary<string, object> party)
        {
            if (!party.ContainsKey("taxScheme"))
                return "VAT";

            var taxScheme = TryGetDictionary(party["taxScheme"]);
            if (taxScheme != null && taxScheme.ContainsKey("id"))
                return GetString(taxScheme, "id");

            return "VAT";
        }

        private void AddPartyLegalEntity(List<XElement> elements, Dictionary<string, object> party)
        {
            if (!party.ContainsKey("registrationName"))
                return;

            elements.Add(new XElement(_cac + "PartyLegalEntity",
                new XElement(_cbc + "RegistrationName", GetString(party, "registrationName"))));
        }

        private XElement GenerateAddress(Dictionary<string, object> address)
        {
            var element = new XElement(_cac + "PostalAddress");

            if (address.ContainsKey("street"))
            {
                element.Add(new XElement(_cbc + "StreetName", GetString(address, "street")));
            }

            if (address.ContainsKey("additionalStreet"))
            {
                element.Add(new XElement(_cbc + "AdditionalStreetName", GetString(address, "additionalStreet")));
            }

            if (address.ContainsKey("buildingNumber"))
            {
                element.Add(new XElement(_cbc + "BuildingNumber", GetString(address, "buildingNumber")));
            }

            if (address.ContainsKey("plotIdentification"))
            {
                element.Add(new XElement(_cbc + "PlotIdentification", GetString(address, "plotIdentification")));
            }

            // Support both citySubdivisionName and subdivision keys
            if (address.ContainsKey("citySubdivisionName"))
            {
                element.Add(new XElement(_cbc + "CitySubdivisionName", GetString(address, "citySubdivisionName")));
            }
            else if (address.ContainsKey("subdivision"))
            {
                element.Add(new XElement(_cbc + "CitySubdivisionName", GetString(address, "subdivision")));
            }

            if (address.ContainsKey("city"))
            {
                element.Add(new XElement(_cbc + "CityName", GetString(address, "city")));
            }

            if (address.ContainsKey("postalZone"))
            {
                element.Add(new XElement(_cbc + "PostalZone", GetString(address, "postalZone")));
            }

            if (address.ContainsKey("countrySubentity"))
            {
                element.Add(new XElement(_cbc + "CountrySubentity", GetString(address, "countrySubentity")));
            }

            if (address.ContainsKey("country"))
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

            if (delivery.ContainsKey("actualDeliveryDate"))
            {
                var date = GetDateTime(delivery["actualDeliveryDate"]);
                element.Add(new XElement(_cbc + "ActualDeliveryDate", date.ToString("yyyy-MM-dd")));
                hasContent = true;
            }

            if (delivery.ContainsKey("latestDeliveryDate"))
            {
                var date = GetDateTime(delivery["latestDeliveryDate"]);
                element.Add(new XElement(_cbc + "LatestDeliveryDate", date.ToString("yyyy-MM-dd")));
                hasContent = true;
            }

            return hasContent ? element : null;
        }

        private XElement GeneratePaymentMeans(Dictionary<string, object> paymentMeans)
        {
            var element = new XElement(_cac + "PaymentMeans");

            if (paymentMeans.ContainsKey("code"))
            {
                element.Add(new XElement(_cbc + "PaymentMeansCode", GetString(paymentMeans, "code")));
            }

            if (paymentMeans.ContainsKey("instructionNote"))
            {
                element.Add(new XElement(_cbc + "InstructionNote", GetString(paymentMeans, "instructionNote")));
            }

            return element;
        }

        private XElement GenerateAllowanceCharge(Dictionary<string, object> ac)
        {
            var element = new XElement(_cac + "AllowanceCharge");

            if (ac.ContainsKey("chargeIndicator"))
                element.Add(new XElement(_cbc + "ChargeIndicator", GetString(ac, "chargeIndicator")));

            if (ac.ContainsKey("allowanceChargeReason"))
                element.Add(new XElement(_cbc + "AllowanceChargeReason", GetString(ac, "allowanceChargeReason")));

            if (ac.ContainsKey("amount"))
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "Amount", GetDecimal(ac["amount"]), _currencyId));

            AddAllowanceChargeTaxCategories(element, ac);

            return element;
        }

        private void AddAllowanceChargeTaxCategories(XElement element, Dictionary<string, object> ac)
        {
            if (!ac.ContainsKey("taxCategories"))
                return;

            var taxCategories = TryGetList(ac["taxCategories"]);
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

            if (categoryDict.ContainsKey("id"))
            {
                taxCategoryElement.Add(new XElement(_cbc + "ID",
                    new XAttribute("schemeID", "UN/ECE 5305"),
                    new XAttribute("schemeAgencyID", "6"),
                    GetString(categoryDict, "id")));
            }

            if (categoryDict.ContainsKey("percent"))
            {
                taxCategoryElement.Add(new XElement(_cbc + "Percent",
                    GetDecimal(categoryDict["percent"]).ToString("F2")));
            }

            AddTaxSchemeElement(taxCategoryElement, categoryDict, "UN/ECE 5153");

            return taxCategoryElement;
        }

        private void AddTaxSchemeElement(XElement parent, Dictionary<string, object> dict, string schemeId)
        {
            if (!dict.ContainsKey("taxScheme"))
                return;

            var taxScheme = TryGetDictionary(dict["taxScheme"]);
            if (taxScheme == null)
                return;

            var taxSchemeElement = new XElement(_cac + "TaxScheme");
            if (taxScheme.ContainsKey("id"))
            {
                taxSchemeElement.Add(new XElement(_cbc + "ID",
                    new XAttribute("schemeID", schemeId),
                    new XAttribute("schemeAgencyID", "6"),
                    GetString(taxScheme, "id")));
            }
            parent.Add(taxSchemeElement);
        }

        private XElement GenerateTaxTotal(Dictionary<string, object> taxTotal)
        {
            var element = new XElement(_cac + "TaxTotal");

            if (taxTotal.ContainsKey("taxAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxAmount", GetDecimal(taxTotal["taxAmount"]), _currencyId));
            }

            if (taxTotal.ContainsKey("subTotals"))
            {
                var subTotals = TryGetList(taxTotal["subTotals"]);
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

            if (subTotal.ContainsKey("taxableAmount"))
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxableAmount", GetDecimal(subTotal["taxableAmount"]), _currencyId));

            if (subTotal.ContainsKey("taxAmount"))
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxAmount", GetDecimal(subTotal["taxAmount"]), _currencyId));

            AddSubTotalTaxCategory(element, subTotal);

            return element;
        }

        private void AddSubTotalTaxCategory(XElement element, Dictionary<string, object> subTotal)
        {
            if (!subTotal.ContainsKey("taxCategory"))
                return;

            var taxCategory = TryGetDictionary(subTotal["taxCategory"]);
            if (taxCategory == null)
                return;

            var taxCategoryElement = GenerateSubTotalTaxCategoryElement(taxCategory);
            element.Add(taxCategoryElement);
        }

        private XElement GenerateSubTotalTaxCategoryElement(Dictionary<string, object> taxCategory)
        {
            var taxCategoryElement = new XElement(_cac + "TaxCategory");

            if (taxCategory.ContainsKey("id"))
            {
                taxCategoryElement.Add(new XElement(_cbc + "ID",
                    new XAttribute("schemeID", "UN/ECE 5305"),
                    new XAttribute("schemeAgencyID", "6"),
                    GetString(taxCategory, "id")));
            }

            if (taxCategory.ContainsKey("percent"))
                taxCategoryElement.Add(new XElement(_cbc + "Percent", GetDecimal(taxCategory["percent"]).FormatPercent()));

            if (taxCategory.ContainsKey("taxExemptionReasonCode"))
                taxCategoryElement.Add(new XElement(_cbc + "TaxExemptionReasonCode", GetString(taxCategory, "taxExemptionReasonCode")));

            if (taxCategory.ContainsKey("taxExemptionReason"))
                taxCategoryElement.Add(new XElement(_cbc + "TaxExemptionReason", GetString(taxCategory, "taxExemptionReason")));

            AddTaxSchemeElement(taxCategoryElement, taxCategory, "UN/ECE 5153");

            return taxCategoryElement;
        }

        private XElement GenerateLegalMonetaryTotal(Dictionary<string, object> lmt)
        {
            var element = new XElement(_cac + "LegalMonetaryTotal");

            if (lmt.ContainsKey("lineExtensionAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "LineExtensionAmount", GetDecimal(lmt["lineExtensionAmount"]), _currencyId));
            }

            if (lmt.ContainsKey("taxExclusiveAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxExclusiveAmount", GetDecimal(lmt["taxExclusiveAmount"]), _currencyId));
            }

            if (lmt.ContainsKey("taxInclusiveAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxInclusiveAmount", GetDecimal(lmt["taxInclusiveAmount"]), _currencyId));
            }

            if (lmt.ContainsKey("allowanceTotalAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "AllowanceTotalAmount", GetDecimal(lmt["allowanceTotalAmount"]), _currencyId));
            }

            if (lmt.ContainsKey("prepaidAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "PrepaidAmount", GetDecimal(lmt["prepaidAmount"]), _currencyId));
            }

            if (lmt.ContainsKey("payableAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "PayableAmount", GetDecimal(lmt["payableAmount"]), _currencyId));
            }

            return element;
        }

        private XElement GenerateInvoiceLine(Dictionary<string, object> line)
        {
            var element = new XElement(_cac + "InvoiceLine");

            if (line.ContainsKey("id"))
                element.Add(new XElement(_cbc + "ID", GetString(line, "id")));

            if (line.ContainsKey("note"))
                element.Add(new XElement(_cbc + "Note", GetString(line, "note")));

            if (line.ContainsKey("quantity"))
                element.Add(XmlSerializationExtensions.CreateQuantityElement(_cbc + "InvoicedQuantity", GetDecimal(line["quantity"]), GetString(line, "unitCode", "PCE")));

            if (line.ContainsKey("lineExtensionAmount"))
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "LineExtensionAmount", GetDecimal(line["lineExtensionAmount"]), _currencyId));

            AddLineTaxTotal(element, line);
            AddOptionalElement(element, line, "item", GenerateItem);
            AddOptionalElement(element, line, "price", GeneratePrice);

            return element;
        }

        private void AddLineTaxTotal(XElement element, Dictionary<string, object> line)
        {
            if (!line.ContainsKey("taxTotal"))
                return;

            var taxTotal = TryGetDictionary(line["taxTotal"]);
            if (taxTotal == null)
                return;

            var taxTotalElement = new XElement(_cac + "TaxTotal");

            if (taxTotal.ContainsKey("taxAmount"))
                taxTotalElement.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxAmount", GetDecimal(taxTotal["taxAmount"]), _currencyId));

            if (taxTotal.ContainsKey("roundingAmount"))
                taxTotalElement.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "RoundingAmount", GetDecimal(taxTotal["roundingAmount"]), _currencyId));

            element.Add(taxTotalElement);
        }

        private XElement GenerateItem(Dictionary<string, object> item)
        {
            var element = new XElement(_cac + "Item");

            if (item.ContainsKey("name"))
                element.Add(new XElement(_cbc + "Name", GetString(item, "name")));

            AddClassifiedTaxCategories(element, item);

            return element;
        }

        private void AddClassifiedTaxCategories(XElement element, Dictionary<string, object> item)
        {
            if (!item.ContainsKey("classifiedTaxCategory"))
                return;

            var classifiedTaxCategories = TryGetList(item["classifiedTaxCategory"]);
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

            if (categoryDict.ContainsKey("id"))
                taxCategoryElement.Add(new XElement(_cbc + "ID", GetString(categoryDict, "id")));

            if (categoryDict.ContainsKey("percent"))
                taxCategoryElement.Add(new XElement(_cbc + "Percent", GetDecimal(categoryDict["percent"]).FormatPercent()));

            AddSimpleTaxScheme(taxCategoryElement, categoryDict);

            return taxCategoryElement;
        }

        private void AddSimpleTaxScheme(XElement parent, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("taxScheme"))
                return;

            var taxScheme = TryGetDictionary(dict["taxScheme"]);
            if (taxScheme == null)
                return;

            var taxSchemeElement = new XElement(_cac + "TaxScheme");
            if (taxScheme.ContainsKey("id"))
                taxSchemeElement.Add(new XElement(_cbc + "ID", GetString(taxScheme, "id")));

            parent.Add(taxSchemeElement);
        }

        private XElement GeneratePrice(Dictionary<string, object> price)
        {
            var element = new XElement(_cac + "Price");

            if (price.ContainsKey("amount"))
            {
                element.Add(XmlSerializationExtensions.CreatePriceElement(_cbc + "PriceAmount", GetDecimal(price["amount"]), _currencyId));
            }

            if (price.ContainsKey("baseQuantity"))
            {
                var unitCode = GetString(price, "baseQuantityUnitCode", "PCE");
                element.Add(XmlSerializationExtensions.CreateQuantityElement(_cbc + "BaseQuantity", GetDecimal(price["baseQuantity"]), unitCode));
            }

            return element;
        }

        // Helper methods
        private string GetString(Dictionary<string, object> dict, string key, string defaultValue = "")
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                var value = dict[key];
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

        private decimal GetDecimal(object value)
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

        private DateTime GetDateTime(object value)
        {
            if (value is DateTime dt)
            {
                return dt;
            }

            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(jsonElement.GetString(), out DateTime jsonResult))
                {
                    return jsonResult;
                }
            }

            if (value is string str && DateTime.TryParse(str, out DateTime result))
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
        private string FormatXml(XElement element)
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
