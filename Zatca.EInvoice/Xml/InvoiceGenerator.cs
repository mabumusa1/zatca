using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            // UBL Extensions
            if (invoiceData.ContainsKey("ublExtensions") && invoiceData["ublExtensions"] != null)
            {
                var ublExtensions = GenerateUBLExtensions(invoiceData["ublExtensions"]);
                if (ublExtensions != null)
                {
                    invoice.Add(ublExtensions);
                }
            }

            // ProfileID
            var profileId = GetString(invoiceData, "profileID", "reporting:1.0");
            invoice.Add(new XElement(_cbc + "ProfileID", profileId));

            // ID
            if (invoiceData.ContainsKey("id"))
            {
                invoice.Add(new XElement(_cbc + "ID", GetString(invoiceData, "id")));
            }

            // UUID
            if (invoiceData.ContainsKey("uuid"))
            {
                invoice.Add(new XElement(_cbc + "UUID", GetString(invoiceData, "uuid")));
            }

            // IssueDate
            if (invoiceData.ContainsKey("issueDate"))
            {
                var issueDate = GetDateTime(invoiceData["issueDate"]);
                invoice.Add(new XElement(_cbc + "IssueDate", issueDate.ToString("yyyy-MM-dd")));
            }

            // IssueTime
            if (invoiceData.ContainsKey("issueTime"))
            {
                var issueTime = GetDateTime(invoiceData["issueTime"]);
                invoice.Add(new XElement(_cbc + "IssueTime", issueTime.ToString("HH:mm:ss")));
            }

            // InvoiceTypeCode
            if (invoiceData.ContainsKey("invoiceType") && invoiceData["invoiceType"] is Dictionary<string, object> invoiceType)
            {
                var typeCode = GetString(invoiceType, "type", "388");
                var invoiceTypeValue = GetString(invoiceType, "invoice", "standard");

                invoice.Add(new XElement(_cbc + "InvoiceTypeCode",
                    new XAttribute("name", invoiceTypeValue),
                    typeCode));
            }

            // Note
            if (invoiceData.ContainsKey("note") && !string.IsNullOrWhiteSpace(GetString(invoiceData, "note")))
            {
                var languageId = GetString(invoiceData, "languageID", "en");
                invoice.Add(new XElement(_cbc + "Note",
                    new XAttribute("languageID", languageId),
                    GetString(invoiceData, "note")));
            }

            // DocumentCurrencyCode
            invoice.Add(new XElement(_cbc + "DocumentCurrencyCode", GetString(invoiceData, "currencyCode", _currencyId)));

            // TaxCurrencyCode
            invoice.Add(new XElement(_cbc + "TaxCurrencyCode", GetString(invoiceData, "taxCurrencyCode", _currencyId)));

            // OrderReference
            if (invoiceData.ContainsKey("orderReference") && invoiceData["orderReference"] is Dictionary<string, object> orderRef)
            {
                invoice.Add(GenerateOrderReference(orderRef));
            }

            // BillingReference(s)
            if (invoiceData.ContainsKey("billingReferences") && invoiceData["billingReferences"] is IList<object> billingRefs)
            {
                foreach (var billingRef in billingRefs)
                {
                    if (billingRef is Dictionary<string, object> billingRefDict)
                    {
                        invoice.Add(GenerateBillingReference(billingRefDict));
                    }
                }
            }

            // ContractDocumentReference
            if (invoiceData.ContainsKey("contract") && invoiceData["contract"] is Dictionary<string, object> contract)
            {
                invoice.Add(GenerateContractReference(contract));
            }

            // AdditionalDocumentReference(s)
            if (invoiceData.ContainsKey("additionalDocuments") && invoiceData["additionalDocuments"] is IList<object> additionalDocs)
            {
                foreach (var doc in additionalDocs)
                {
                    if (doc is Dictionary<string, object> docDict)
                    {
                        invoice.Add(GenerateAdditionalDocumentReference(docDict));
                    }
                }
            }

            // Signature
            if (invoiceData.ContainsKey("signature") && invoiceData["signature"] is Dictionary<string, object> signature)
            {
                invoice.Add(GenerateSignature(signature));
            }

            // AccountingSupplierParty
            if (invoiceData.ContainsKey("supplier") && invoiceData["supplier"] is Dictionary<string, object> supplier)
            {
                invoice.Add(GenerateAccountingSupplierParty(supplier));
            }

            // AccountingCustomerParty
            if (invoiceData.ContainsKey("customer") && invoiceData["customer"] is Dictionary<string, object> customer)
            {
                invoice.Add(GenerateAccountingCustomerParty(customer));
            }

            // Delivery
            if (invoiceData.ContainsKey("delivery") && invoiceData["delivery"] is Dictionary<string, object> delivery)
            {
                var deliveryElement = GenerateDelivery(delivery);
                if (deliveryElement != null)
                {
                    invoice.Add(deliveryElement);
                }
            }

            // PaymentMeans
            if (invoiceData.ContainsKey("paymentMeans") && invoiceData["paymentMeans"] is Dictionary<string, object> paymentMeans)
            {
                invoice.Add(GeneratePaymentMeans(paymentMeans));
            }

            // AllowanceCharge(s)
            if (invoiceData.ContainsKey("allowanceCharges") && invoiceData["allowanceCharges"] is IList<object> allowanceCharges)
            {
                foreach (var allowanceCharge in allowanceCharges)
                {
                    if (allowanceCharge is Dictionary<string, object> acDict)
                    {
                        invoice.Add(GenerateAllowanceCharge(acDict));
                    }
                }
            }

            // TaxTotal
            if (invoiceData.ContainsKey("taxTotal") && invoiceData["taxTotal"] is Dictionary<string, object> taxTotal)
            {
                // First TaxTotal with currency
                if (taxTotal.ContainsKey("taxAmount"))
                {
                    var taxAmountElement = new XElement(_cac + "TaxTotal",
                        XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxAmount", GetDecimal(taxTotal["taxAmount"]), _currencyId)
                    );
                    invoice.Add(taxAmountElement);
                }

                // Second TaxTotal with subtotals
                invoice.Add(GenerateTaxTotal(taxTotal));
            }

            // LegalMonetaryTotal
            if (invoiceData.ContainsKey("legalMonetaryTotal") && invoiceData["legalMonetaryTotal"] is Dictionary<string, object> lmt)
            {
                invoice.Add(GenerateLegalMonetaryTotal(lmt));
            }

            // InvoiceLine(s)
            if (invoiceData.ContainsKey("invoiceLines") && invoiceData["invoiceLines"] is IList<object> invoiceLines)
            {
                foreach (var line in invoiceLines)
                {
                    if (line is Dictionary<string, object> lineDict)
                    {
                        invoice.Add(GenerateInvoiceLine(lineDict));
                    }
                }
            }

            return invoice;
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

            if (billingRef.ContainsKey("invoiceDocumentReference") &&
                billingRef["invoiceDocumentReference"] is Dictionary<string, object> invoiceDocRef)
            {
                var invoiceDocRefElement = new XElement(_cac + "InvoiceDocumentReference");

                if (invoiceDocRef.ContainsKey("id"))
                {
                    invoiceDocRefElement.Add(new XElement(_cbc + "ID", GetString(invoiceDocRef, "id")));
                }

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

            if (doc.ContainsKey("attachment") && doc["attachment"] is Dictionary<string, object> attachment)
            {
                var attachmentElement = new XElement(_cac + "Attachment");

                if (attachment.ContainsKey("embeddedDocument"))
                {
                    var embeddedDoc = GetString(attachment, "embeddedDocument");
                    var mimeCode = GetString(attachment, "mimeCode", "text/plain");
                    var encoding = GetString(attachment, "encoding", "UTF-8");

                    attachmentElement.Add(new XElement(_cbc + "EmbeddedDocumentBinaryObject",
                        new XAttribute("mimeCode", mimeCode),
                        new XAttribute("encoding", encoding),
                        embeddedDoc));
                }

                element.Add(attachmentElement);
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
            var element = new XElement(_cac + "AccountingCustomerParty",
                new XElement(_cac + "Party",
                    GenerateParty(customer)
                )
            );

            return element;
        }

        private XElement[] GenerateParty(Dictionary<string, object> party)
        {
            var elements = new List<XElement>();

            // PartyIdentification
            if (party.ContainsKey("partyIdentification") && !string.IsNullOrWhiteSpace(GetString(party, "partyIdentification")))
            {
                var partyIdElement = new XElement(_cac + "PartyIdentification");
                var idElement = new XElement(_cbc + "ID", GetString(party, "partyIdentification"));

                if (party.ContainsKey("partyIdentificationId"))
                {
                    idElement.Add(new XAttribute("schemeID", GetString(party, "partyIdentificationId")));
                }

                partyIdElement.Add(idElement);
                elements.Add(partyIdElement);
            }

            // PostalAddress
            if (party.ContainsKey("address") && party["address"] is Dictionary<string, object> address)
            {
                elements.Add(GenerateAddress(address));
            }

            // PartyTaxScheme
            if (party.ContainsKey("taxId") || party.ContainsKey("taxScheme"))
            {
                var partyTaxScheme = new XElement(_cac + "PartyTaxScheme");

                if (party.ContainsKey("taxId"))
                {
                    partyTaxScheme.Add(new XElement(_cbc + "CompanyID", GetString(party, "taxId")));
                }

                var taxSchemeElement = new XElement(_cac + "TaxScheme");
                if (party.ContainsKey("taxScheme") && party["taxScheme"] is Dictionary<string, object> taxScheme)
                {
                    if (taxScheme.ContainsKey("id"))
                    {
                        taxSchemeElement.Add(new XElement(_cbc + "ID", GetString(taxScheme, "id")));
                    }
                }
                else
                {
                    taxSchemeElement.Add(new XElement(_cbc + "ID", "VAT"));
                }

                partyTaxScheme.Add(taxSchemeElement);
                elements.Add(partyTaxScheme);
            }

            // PartyLegalEntity
            if (party.ContainsKey("registrationName"))
            {
                var partyLegalEntity = new XElement(_cac + "PartyLegalEntity",
                    new XElement(_cbc + "RegistrationName", GetString(party, "registrationName"))
                );

                elements.Add(partyLegalEntity);
            }

            return elements.ToArray();
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

            if (address.ContainsKey("citySubdivisionName"))
            {
                element.Add(new XElement(_cbc + "CitySubdivisionName", GetString(address, "citySubdivisionName")));
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

        private XElement GenerateDelivery(Dictionary<string, object> delivery)
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
            {
                element.Add(new XElement(_cbc + "ChargeIndicator", GetString(ac, "chargeIndicator")));
            }

            if (ac.ContainsKey("allowanceChargeReason"))
            {
                element.Add(new XElement(_cbc + "AllowanceChargeReason", GetString(ac, "allowanceChargeReason")));
            }

            if (ac.ContainsKey("amount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "Amount", GetDecimal(ac["amount"]), _currencyId));
            }

            return element;
        }

        private XElement GenerateTaxTotal(Dictionary<string, object> taxTotal)
        {
            var element = new XElement(_cac + "TaxTotal");

            if (taxTotal.ContainsKey("taxAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxAmount", GetDecimal(taxTotal["taxAmount"]), _currencyId));
            }

            if (taxTotal.ContainsKey("subTotals") && taxTotal["subTotals"] is IList<object> subTotals)
            {
                foreach (var subTotal in subTotals)
                {
                    if (subTotal is Dictionary<string, object> subTotalDict)
                    {
                        element.Add(GenerateTaxSubTotal(subTotalDict));
                    }
                }
            }

            return element;
        }

        private XElement GenerateTaxSubTotal(Dictionary<string, object> subTotal)
        {
            var element = new XElement(_cac + "TaxSubtotal");

            if (subTotal.ContainsKey("taxableAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxableAmount", GetDecimal(subTotal["taxableAmount"]), _currencyId));
            }

            if (subTotal.ContainsKey("taxAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxAmount", GetDecimal(subTotal["taxAmount"]), _currencyId));
            }

            if (subTotal.ContainsKey("taxCategory") && subTotal["taxCategory"] is Dictionary<string, object> taxCategory)
            {
                var taxCategoryElement = new XElement(_cac + "TaxCategory");

                if (taxCategory.ContainsKey("id"))
                {
                    taxCategoryElement.Add(new XElement(_cbc + "ID", GetString(taxCategory, "id")));
                }

                if (taxCategory.ContainsKey("percent"))
                {
                    taxCategoryElement.Add(new XElement(_cbc + "Percent", GetDecimal(taxCategory["percent"]).FormatPercent()));
                }

                if (taxCategory.ContainsKey("taxExemptionReason"))
                {
                    taxCategoryElement.Add(new XElement(_cbc + "TaxExemptionReason", GetString(taxCategory, "taxExemptionReason")));
                }

                if (taxCategory.ContainsKey("taxExemptionReasonCode"))
                {
                    taxCategoryElement.Add(new XElement(_cbc + "TaxExemptionReasonCode", GetString(taxCategory, "taxExemptionReasonCode")));
                }

                if (taxCategory.ContainsKey("taxScheme") && taxCategory["taxScheme"] is Dictionary<string, object> taxScheme)
                {
                    var taxSchemeElement = new XElement(_cac + "TaxScheme");

                    if (taxScheme.ContainsKey("id"))
                    {
                        taxSchemeElement.Add(new XElement(_cbc + "ID", GetString(taxScheme, "id")));
                    }

                    taxCategoryElement.Add(taxSchemeElement);
                }

                element.Add(taxCategoryElement);
            }

            return element;
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
            {
                element.Add(new XElement(_cbc + "ID", GetString(line, "id")));
            }

            if (line.ContainsKey("note"))
            {
                element.Add(new XElement(_cbc + "Note", GetString(line, "note")));
            }

            if (line.ContainsKey("quantity"))
            {
                var unitCode = GetString(line, "unitCode", "PCE");
                element.Add(XmlSerializationExtensions.CreateQuantityElement(_cbc + "InvoicedQuantity", GetDecimal(line["quantity"]), unitCode));
            }

            if (line.ContainsKey("lineExtensionAmount"))
            {
                element.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "LineExtensionAmount", GetDecimal(line["lineExtensionAmount"]), _currencyId));
            }

            // TaxTotal
            if (line.ContainsKey("taxTotal") && line["taxTotal"] is Dictionary<string, object> taxTotal)
            {
                var taxTotalElement = new XElement(_cac + "TaxTotal");

                if (taxTotal.ContainsKey("taxAmount"))
                {
                    taxTotalElement.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "TaxAmount", GetDecimal(taxTotal["taxAmount"]), _currencyId));
                }

                if (taxTotal.ContainsKey("roundingAmount"))
                {
                    taxTotalElement.Add(XmlSerializationExtensions.CreateAmountElement(_cbc + "RoundingAmount", GetDecimal(taxTotal["roundingAmount"]), _currencyId));
                }

                element.Add(taxTotalElement);
            }

            // Item
            if (line.ContainsKey("item") && line["item"] is Dictionary<string, object> item)
            {
                element.Add(GenerateItem(item));
            }

            // Price
            if (line.ContainsKey("price") && line["price"] is Dictionary<string, object> price)
            {
                element.Add(GeneratePrice(price));
            }

            return element;
        }

        private XElement GenerateItem(Dictionary<string, object> item)
        {
            var element = new XElement(_cac + "Item");

            if (item.ContainsKey("name"))
            {
                element.Add(new XElement(_cbc + "Name", GetString(item, "name")));
            }

            if (item.ContainsKey("classifiedTaxCategory") && item["classifiedTaxCategory"] is IList<object> classifiedTaxCategories)
            {
                foreach (var category in classifiedTaxCategories)
                {
                    if (category is Dictionary<string, object> categoryDict)
                    {
                        var taxCategoryElement = new XElement(_cac + "ClassifiedTaxCategory");

                        if (categoryDict.ContainsKey("id"))
                        {
                            taxCategoryElement.Add(new XElement(_cbc + "ID", GetString(categoryDict, "id")));
                        }

                        if (categoryDict.ContainsKey("percent"))
                        {
                            taxCategoryElement.Add(new XElement(_cbc + "Percent", GetDecimal(categoryDict["percent"]).FormatPercent()));
                        }

                        if (categoryDict.ContainsKey("taxScheme") && categoryDict["taxScheme"] is Dictionary<string, object> taxScheme)
                        {
                            var taxSchemeElement = new XElement(_cac + "TaxScheme");

                            if (taxScheme.ContainsKey("id"))
                            {
                                taxSchemeElement.Add(new XElement(_cbc + "ID", GetString(taxScheme, "id")));
                            }

                            taxCategoryElement.Add(taxSchemeElement);
                        }

                        element.Add(taxCategoryElement);
                    }
                }
            }

            return element;
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
                return dict[key].ToString();
            }
            return defaultValue;
        }

        private decimal GetDecimal(object value)
        {
            if (value == null)
            {
                return 0m;
            }

            if (value is decimal d)
            {
                return d;
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

            if (value is string str && DateTime.TryParse(str, out DateTime result))
            {
                return result;
            }

            return DateTime.Now;
        }

        /// <summary>
        /// Formats XML with proper indentation.
        /// </summary>
        /// <param name="element">The XML element to format.</param>
        /// <returns>A formatted XML string.</returns>
        private string FormatXml(XElement element)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using (var stringWriter = new System.IO.StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                element.WriteTo(xmlWriter);
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }
    }
}
