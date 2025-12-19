using System.Xml.Linq;
using Zatca.EInvoice.Xml;

namespace Zatca.EInvoice.Tests.Xml;

public class InvoiceGeneratorTests
{
    private readonly InvoiceGenerator _generator;

    public InvoiceGeneratorTests()
    {
        _generator = new InvoiceGenerator("SAR");
    }

    #region Constructor Tests

    [Fact]
    public void InvoiceGenerator_DefaultCurrency_UsesSAR()
    {
        var generator = new InvoiceGenerator();
        var data = CreateMinimalInvoiceData();
        var xml = generator.Generate(data);
        xml.Should().Contain("SAR");
    }

    [Fact]
    public void InvoiceGenerator_CustomCurrency_UsesSpecifiedCurrency()
    {
        var generator = new InvoiceGenerator("USD");
        var data = CreateMinimalInvoiceData();
        data["legalMonetaryTotal"] = new Dictionary<string, object>
        {
            { "lineExtensionAmount", 100m }
        };
        var xml = generator.Generate(data);
        xml.Should().Contain("USD");
    }

    [Fact]
    public void InvoiceGenerator_NullCurrency_DefaultsToSAR()
    {
        var generator = new InvoiceGenerator(null!);
        var data = CreateMinimalInvoiceData();
        var xml = generator.Generate(data);
        xml.Should().Contain("SAR");
    }

    #endregion

    #region Generate Method Tests

    [Fact]
    public void Generate_MinimalInvoice_CreatesValidXml()
    {
        var data = CreateMinimalInvoiceData();
        var xml = _generator.Generate(data);

        xml.Should().StartWith("<?xml version=\"1.0\"");
        xml.Should().Contain("<Invoice");
        xml.Should().Contain("xmlns:cac=");
        xml.Should().Contain("xmlns:cbc=");
    }

    [Fact]
    public void Generate_WithId_IncludesIdElement()
    {
        var data = CreateMinimalInvoiceData();
        data["id"] = "INV-001";

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:ID>INV-001</cbc:ID>");
    }

    [Fact]
    public void Generate_WithUuid_IncludesUuidElement()
    {
        var data = CreateMinimalInvoiceData();
        data["uuid"] = "12345678-1234-1234-1234-123456789012";

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:UUID>12345678-1234-1234-1234-123456789012</cbc:UUID>");
    }

    [Fact]
    public void Generate_WithIssueDate_IncludesIssueDateElement()
    {
        var data = CreateMinimalInvoiceData();
        data["issueDate"] = new DateTime(2024, 1, 15);

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:IssueDate>2024-01-15</cbc:IssueDate>");
    }

    [Fact]
    public void Generate_WithIssueTime_IncludesIssueTimeElement()
    {
        var data = CreateMinimalInvoiceData();
        data["issueTime"] = new DateTime(2024, 1, 15, 14, 30, 45);

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:IssueTime>14:30:45</cbc:IssueTime>");
    }

    [Fact]
    public void Generate_WithStringDate_ParsesAndFormatsCorrectly()
    {
        var data = CreateMinimalInvoiceData();
        data["issueDate"] = "2024-01-15";

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:IssueDate>2024-01-15</cbc:IssueDate>");
    }

    #endregion

    #region InvoiceTypeCode Tests

    [Fact]
    public void Generate_WithInvoiceType_IncludesInvoiceTypeCode()
    {
        var data = CreateMinimalInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "type", "388" },
            { "invoice", "0100000" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:InvoiceTypeCode");
        xml.Should().Contain("name=\"0100000\"");
        xml.Should().Contain(">388</cbc:InvoiceTypeCode>");
    }

    [Fact]
    public void Generate_WithCreditType_ConvertsToCode381()
    {
        var data = CreateMinimalInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "type", "credit" },
            { "invoice", "0100000" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain(">381</cbc:InvoiceTypeCode>");
    }

    [Fact]
    public void Generate_WithDebitType_ConvertsToCode383()
    {
        var data = CreateMinimalInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "type", "debit" },
            { "invoice", "0100000" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain(">383</cbc:InvoiceTypeCode>");
    }

    [Fact]
    public void Generate_WithTypeCode_UsesExplicitTypeCode()
    {
        var data = CreateMinimalInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "typeCode", "381" },
            { "type", "credit" },
            { "invoice", "0100000" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain(">381</cbc:InvoiceTypeCode>");
    }

    [Fact]
    public void Generate_WithInvoiceTypeName_UsesExplicitName()
    {
        var data = CreateMinimalInvoiceData();
        data["invoiceType"] = new Dictionary<string, object>
        {
            { "type", "388" },
            { "name", "0200000" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("name=\"0200000\"");
    }

    #endregion

    #region Note Tests

    [Fact]
    public void Generate_WithNote_IncludesNoteElement()
    {
        var data = CreateMinimalInvoiceData();
        data["note"] = "This is a test note";

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:Note");
        xml.Should().Contain("This is a test note");
    }

    [Fact]
    public void Generate_WithNoteAndLanguageId_IncludesLanguageAttribute()
    {
        var data = CreateMinimalInvoiceData();
        data["note"] = "Test note";
        data["languageID"] = "ar";

        var xml = _generator.Generate(data);

        xml.Should().Contain("languageID=\"ar\"");
    }

    [Fact]
    public void Generate_WithEmptyNote_DoesNotIncludeNoteElement()
    {
        var data = CreateMinimalInvoiceData();
        data["note"] = "";

        var xml = _generator.Generate(data);

        xml.Should().NotContain("<cbc:Note");
    }

    #endregion

    #region Currency Tests

    [Fact]
    public void Generate_WithCurrencyCode_IncludesCurrencyElements()
    {
        var data = CreateMinimalInvoiceData();
        data["currencyCode"] = "SAR";
        data["taxCurrencyCode"] = "SAR";

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:DocumentCurrencyCode>SAR</cbc:DocumentCurrencyCode>");
        xml.Should().Contain("<cbc:TaxCurrencyCode>SAR</cbc:TaxCurrencyCode>");
    }

    #endregion

    #region OrderReference Tests

    [Fact]
    public void Generate_WithOrderReference_IncludesOrderReferenceElement()
    {
        var data = CreateMinimalInvoiceData();
        data["orderReference"] = new Dictionary<string, object>
        {
            { "id", "PO-001" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:OrderReference>");
        xml.Should().Contain("<cbc:ID>PO-001</cbc:ID>");
    }

    #endregion

    #region BillingReference Tests

    [Fact]
    public void Generate_WithBillingReferences_IncludesBillingReferenceElements()
    {
        var data = CreateMinimalInvoiceData();
        data["billingReferences"] = new List<object>
        {
            new Dictionary<string, object>
            {
                { "invoiceDocumentReference", new Dictionary<string, object>
                    {
                        { "id", "INV-000" }
                    }
                }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:BillingReference>");
        xml.Should().Contain("<cac:InvoiceDocumentReference>");
        xml.Should().Contain("<cbc:ID>INV-000</cbc:ID>");
    }

    [Fact]
    public void Generate_WithBillingReferenceDirectId_IncludesId()
    {
        var data = CreateMinimalInvoiceData();
        data["billingReferences"] = new List<object>
        {
            new Dictionary<string, object>
            {
                { "id", "INV-000" }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:BillingReference>");
        xml.Should().Contain("<cbc:ID>INV-000</cbc:ID>");
    }

    #endregion

    #region Contract Tests

    [Fact]
    public void Generate_WithContract_IncludesContractElement()
    {
        var data = CreateMinimalInvoiceData();
        data["contract"] = new Dictionary<string, object>
        {
            { "id", "CONTRACT-001" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:ContractDocumentReference>");
        xml.Should().Contain("<cbc:ID>CONTRACT-001</cbc:ID>");
    }

    #endregion

    #region AdditionalDocumentReference Tests

    [Fact]
    public void Generate_WithAdditionalDocuments_IncludesDocumentReferences()
    {
        var data = CreateMinimalInvoiceData();
        data["additionalDocuments"] = new List<object>
        {
            new Dictionary<string, object>
            {
                { "id", "ICV" },
                { "uuid", "1" }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:AdditionalDocumentReference>");
        xml.Should().Contain("<cbc:ID>ICV</cbc:ID>");
        xml.Should().Contain("<cbc:UUID>1</cbc:UUID>");
    }

    [Fact]
    public void Generate_WithAdditionalDocumentAttachment_IncludesAttachment()
    {
        var data = CreateMinimalInvoiceData();
        data["additionalDocuments"] = new List<object>
        {
            new Dictionary<string, object>
            {
                { "id", "PIH" },
                { "attachment", new Dictionary<string, object>
                    {
                        { "embeddedDocument", "base64content" },
                        { "mimeCode", "text/plain" }
                    }
                }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:Attachment>");
        xml.Should().Contain("<cbc:EmbeddedDocumentBinaryObject");
        xml.Should().Contain("mimeCode=\"text/plain\"");
        xml.Should().Contain("base64content");
    }

    #endregion

    #region Signature Tests

    [Fact]
    public void Generate_WithSignature_IncludesSignatureElement()
    {
        var data = CreateMinimalInvoiceData();
        data["signature"] = new Dictionary<string, object>
        {
            { "id", "urn:oasis:names:specification:ubl:signature:Invoice" },
            { "signatureMethod", "urn:oasis:names:specification:ubl:dsig:enveloped:xades" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:Signature>");
        xml.Should().Contain("<cbc:SignatureMethod>");
    }

    #endregion

    #region Supplier Tests

    [Fact]
    public void Generate_WithSupplier_IncludesSupplierElement()
    {
        var data = CreateMinimalInvoiceData();
        data["supplier"] = new Dictionary<string, object>
        {
            { "registrationName", "Test Company" },
            { "taxId", "300000000000003" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:AccountingSupplierParty>");
        xml.Should().Contain("<cbc:RegistrationName>Test Company</cbc:RegistrationName>");
        xml.Should().Contain("<cbc:CompanyID>300000000000003</cbc:CompanyID>");
    }

    [Fact]
    public void Generate_WithSupplierAddress_IncludesAddressElements()
    {
        var data = CreateMinimalInvoiceData();
        data["supplier"] = new Dictionary<string, object>
        {
            { "registrationName", "Test Company" },
            { "address", new Dictionary<string, object>
                {
                    { "street", "Test Street" },
                    { "additionalStreet", "Additional Street" },
                    { "buildingNumber", "123" },
                    { "plotIdentification", "456" },
                    { "citySubdivisionName", "District" },
                    { "city", "Riyadh" },
                    { "postalZone", "12345" },
                    { "countrySubentity", "Region" },
                    { "country", "SA" }
                }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:StreetName>Test Street</cbc:StreetName>");
        xml.Should().Contain("<cbc:AdditionalStreetName>Additional Street</cbc:AdditionalStreetName>");
        xml.Should().Contain("<cbc:BuildingNumber>123</cbc:BuildingNumber>");
        xml.Should().Contain("<cbc:PlotIdentification>456</cbc:PlotIdentification>");
        xml.Should().Contain("<cbc:CitySubdivisionName>District</cbc:CitySubdivisionName>");
        xml.Should().Contain("<cbc:CityName>Riyadh</cbc:CityName>");
        xml.Should().Contain("<cbc:PostalZone>12345</cbc:PostalZone>");
        xml.Should().Contain("<cbc:CountrySubentity>Region</cbc:CountrySubentity>");
        xml.Should().Contain("<cbc:IdentificationCode>SA</cbc:IdentificationCode>");
    }

    [Fact]
    public void Generate_WithSupplierPartyIdentification_IncludesPartyId()
    {
        var data = CreateMinimalInvoiceData();
        data["supplier"] = new Dictionary<string, object>
        {
            { "registrationName", "Test Company" },
            { "partyIdentification", "1234567890" },
            { "partyIdentificationId", "CRN" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:PartyIdentification>");
        xml.Should().Contain("schemeID=\"CRN\"");
        xml.Should().Contain(">1234567890</cbc:ID>");
    }

    [Fact]
    public void Generate_WithSupplierTaxScheme_IncludesTaxScheme()
    {
        var data = CreateMinimalInvoiceData();
        data["supplier"] = new Dictionary<string, object>
        {
            { "registrationName", "Test Company" },
            { "taxId", "300000000000003" },
            { "taxScheme", new Dictionary<string, object>
                {
                    { "id", "VAT" }
                }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:TaxScheme>");
        xml.Should().Contain(">VAT</cbc:ID>");
    }

    [Fact]
    public void Generate_WithAddressSubdivision_IncludesCitySubdivisionName()
    {
        var data = CreateMinimalInvoiceData();
        data["supplier"] = new Dictionary<string, object>
        {
            { "registrationName", "Test Company" },
            { "address", new Dictionary<string, object>
                {
                    { "subdivision", "Test Subdivision" },
                    { "city", "Riyadh" },
                    { "country", "SA" }
                }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:CitySubdivisionName>Test Subdivision</cbc:CitySubdivisionName>");
    }

    #endregion

    #region Customer Tests

    [Fact]
    public void Generate_WithCustomer_IncludesCustomerElement()
    {
        var data = CreateMinimalInvoiceData();
        data["customer"] = new Dictionary<string, object>
        {
            { "registrationName", "Customer Company" },
            { "taxId", "300000000000004" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:AccountingCustomerParty>");
        xml.Should().Contain("<cbc:RegistrationName>Customer Company</cbc:RegistrationName>");
    }

    [Fact]
    public void Generate_WithEmptyCustomer_IncludesEmptyCustomerElement()
    {
        var data = CreateMinimalInvoiceData();
        data["customer"] = new Dictionary<string, object>();

        var xml = _generator.Generate(data);

        // Empty customer generates a self-closing element
        xml.Should().Contain("<cac:AccountingCustomerParty />");
    }

    #endregion

    #region Delivery Tests

    [Fact]
    public void Generate_WithDelivery_IncludesDeliveryElement()
    {
        var data = CreateMinimalInvoiceData();
        data["delivery"] = new Dictionary<string, object>
        {
            { "actualDeliveryDate", new DateTime(2024, 1, 15) }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:Delivery>");
        xml.Should().Contain("<cbc:ActualDeliveryDate>2024-01-15</cbc:ActualDeliveryDate>");
    }

    [Fact]
    public void Generate_WithLatestDeliveryDate_IncludesLatestDeliveryDate()
    {
        var data = CreateMinimalInvoiceData();
        data["delivery"] = new Dictionary<string, object>
        {
            { "latestDeliveryDate", new DateTime(2024, 1, 20) }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:LatestDeliveryDate>2024-01-20</cbc:LatestDeliveryDate>");
    }

    [Fact]
    public void Generate_WithEmptyDelivery_DoesNotIncludeDeliveryElement()
    {
        var data = CreateMinimalInvoiceData();
        data["delivery"] = new Dictionary<string, object>();

        var xml = _generator.Generate(data);

        xml.Should().NotContain("<cac:Delivery>");
    }

    #endregion

    #region PaymentMeans Tests

    [Fact]
    public void Generate_WithPaymentMeans_IncludesPaymentMeansElement()
    {
        var data = CreateMinimalInvoiceData();
        data["paymentMeans"] = new Dictionary<string, object>
        {
            { "code", "10" },
            { "instructionNote", "Payment instructions" }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:PaymentMeans>");
        xml.Should().Contain("<cbc:PaymentMeansCode>10</cbc:PaymentMeansCode>");
        xml.Should().Contain("<cbc:InstructionNote>Payment instructions</cbc:InstructionNote>");
    }

    #endregion

    #region AllowanceCharge Tests

    [Fact]
    public void Generate_WithAllowanceCharges_IncludesAllowanceChargeElements()
    {
        var data = CreateMinimalInvoiceData();
        data["allowanceCharges"] = new List<object>
        {
            new Dictionary<string, object>
            {
                { "chargeIndicator", "false" },
                { "allowanceChargeReason", "Discount" },
                { "amount", 10m },
                { "taxCategories", new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            { "id", "S" },
                            { "percent", 15m },
                            { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                        }
                    }
                }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:AllowanceCharge>");
        xml.Should().Contain("<cbc:ChargeIndicator>false</cbc:ChargeIndicator>");
        xml.Should().Contain("<cbc:AllowanceChargeReason>Discount</cbc:AllowanceChargeReason>");
    }

    #endregion

    #region TaxTotal Tests

    [Fact]
    public void Generate_WithTaxTotal_IncludesTaxTotalElements()
    {
        var data = CreateMinimalInvoiceData();
        data["taxTotal"] = new Dictionary<string, object>
        {
            { "taxAmount", 15m },
            { "subTotals", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "taxableAmount", 100m },
                        { "taxAmount", 15m },
                        { "taxCategory", new Dictionary<string, object>
                            {
                                { "id", "S" },
                                { "percent", 15m },
                                { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                            }
                        }
                    }
                }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:TaxTotal>");
        xml.Should().Contain("<cbc:TaxAmount");
        xml.Should().Contain("<cac:TaxSubtotal>");
        xml.Should().Contain("<cbc:TaxableAmount");
    }

    [Fact]
    public void Generate_WithTaxExemption_IncludesExemptionReasonElements()
    {
        var data = CreateMinimalInvoiceData();
        data["taxTotal"] = new Dictionary<string, object>
        {
            { "taxAmount", 0m },
            { "subTotals", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "taxableAmount", 100m },
                        { "taxAmount", 0m },
                        { "taxCategory", new Dictionary<string, object>
                            {
                                { "id", "E" },
                                { "percent", 0m },
                                { "taxExemptionReasonCode", "VATEX-SA-32" },
                                { "taxExemptionReason", "Export of goods" },
                                { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                            }
                        }
                    }
                }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:TaxExemptionReasonCode>VATEX-SA-32</cbc:TaxExemptionReasonCode>");
        xml.Should().Contain("<cbc:TaxExemptionReason>Export of goods</cbc:TaxExemptionReason>");
    }

    #endregion

    #region LegalMonetaryTotal Tests

    [Fact]
    public void Generate_WithLegalMonetaryTotal_IncludesMonetaryTotalElements()
    {
        var data = CreateMinimalInvoiceData();
        data["legalMonetaryTotal"] = new Dictionary<string, object>
        {
            { "lineExtensionAmount", 100m },
            { "taxExclusiveAmount", 100m },
            { "taxInclusiveAmount", 115m },
            { "allowanceTotalAmount", 0m },
            { "prepaidAmount", 0m },
            { "payableAmount", 115m }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:LegalMonetaryTotal>");
        xml.Should().Contain("<cbc:LineExtensionAmount");
        xml.Should().Contain("<cbc:TaxExclusiveAmount");
        xml.Should().Contain("<cbc:TaxInclusiveAmount");
        xml.Should().Contain("<cbc:AllowanceTotalAmount");
        xml.Should().Contain("<cbc:PrepaidAmount");
        xml.Should().Contain("<cbc:PayableAmount");
    }

    #endregion

    #region InvoiceLine Tests

    [Fact]
    public void Generate_WithInvoiceLines_IncludesInvoiceLineElements()
    {
        var data = CreateMinimalInvoiceData();
        data["invoiceLines"] = new List<object>
        {
            new Dictionary<string, object>
            {
                { "id", "1" },
                { "note", "Line note" },
                { "quantity", 2m },
                { "unitCode", "PCE" },
                { "lineExtensionAmount", 200m },
                { "item", new Dictionary<string, object>
                    {
                        { "name", "Test Item" },
                        { "classifiedTaxCategory", new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    { "id", "S" },
                                    { "percent", 15m },
                                    { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                                }
                            }
                        }
                    }
                },
                { "price", new Dictionary<string, object>
                    {
                        { "amount", 100m },
                        { "baseQuantity", 1m },
                        { "baseQuantityUnitCode", "PCE" }
                    }
                },
                { "taxTotal", new Dictionary<string, object>
                    {
                        { "taxAmount", 30m },
                        { "roundingAmount", 230m }
                    }
                }
            }
        };

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cac:InvoiceLine>");
        xml.Should().Contain("<cbc:ID>1</cbc:ID>");
        xml.Should().Contain("<cbc:Note>Line note</cbc:Note>");
        xml.Should().Contain("<cbc:InvoicedQuantity");
        xml.Should().Contain("unitCode=\"PCE\"");
        xml.Should().Contain("<cbc:LineExtensionAmount");
        xml.Should().Contain("<cac:Item>");
        xml.Should().Contain("<cbc:Name>Test Item</cbc:Name>");
        xml.Should().Contain("<cac:ClassifiedTaxCategory>");
        xml.Should().Contain("<cac:Price>");
        xml.Should().Contain("<cbc:PriceAmount");
        xml.Should().Contain("<cbc:BaseQuantity");
    }

    #endregion

    #region UBL Extensions Tests

    [Fact]
    public void Generate_WithUblExtensions_IncludesUblExtensionsElement()
    {
        var data = CreateMinimalInvoiceData();
        data["ublExtensions"] = new Dictionary<string, object>();

        var xml = _generator.Generate(data);

        xml.Should().Contain("<ext:UBLExtensions");
    }

    #endregion

    #region GenerateInvoiceElement Tests

    [Fact]
    public void GenerateInvoiceElement_ReturnsXElement()
    {
        var data = CreateMinimalInvoiceData();
        var element = _generator.GenerateInvoiceElement(data);

        element.Should().NotBeNull();
        element.Name.LocalName.Should().Be("Invoice");
    }

    #endregion

    #region ProfileID Tests

    [Fact]
    public void Generate_DefaultProfileId_UsesReporting()
    {
        var data = CreateMinimalInvoiceData();
        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:ProfileID>reporting:1.0</cbc:ProfileID>");
    }

    [Fact]
    public void Generate_WithCustomProfileId_UsesCustomValue()
    {
        var data = CreateMinimalInvoiceData();
        data["profileID"] = "clearance:1.0";

        var xml = _generator.Generate(data);

        xml.Should().Contain("<cbc:ProfileID>clearance:1.0</cbc:ProfileID>");
    }

    #endregion

    #region Helper Methods

    private static Dictionary<string, object> CreateMinimalInvoiceData()
    {
        return new Dictionary<string, object>
        {
            { "id", "INV-001" },
            { "uuid", "12345678-1234-1234-1234-123456789012" },
            { "issueDate", new DateTime(2024, 1, 15) },
            { "currencyCode", "SAR" },
            { "taxCurrencyCode", "SAR" }
        };
    }

    #endregion
}
