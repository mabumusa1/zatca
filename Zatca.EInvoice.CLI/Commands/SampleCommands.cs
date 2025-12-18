using System.CommandLine;
using System.Text.Json;
using Zatca.EInvoice.CLI.Output;

namespace Zatca.EInvoice.CLI.Commands;

/// <summary>
/// Sample data command handlers.
/// </summary>
public static class SampleCommands
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static Command CreateSampleCommand(IOutputFormatter formatter, FileWriter fileWriter)
    {
        var sampleCommand = new Command("sample", "Generate sample data files");

        sampleCommand.AddCommand(CreateInvoiceCommand(formatter, fileWriter));
        sampleCommand.AddCommand(CreateCertConfigCommand(formatter, fileWriter));

        return sampleCommand;
    }

    private static Command CreateInvoiceCommand(IOutputFormatter formatter, FileWriter fileWriter)
    {
        var invoiceCommand = new Command("invoice", "Generate sample invoice JSON");

        var typeOption = new Option<string>("--type", () => "standard",
            "Invoice type: standard|simplified|debit|credit|export|prepayment");
        var fullOption = new Option<bool>("--full", () => false, "Include all optional fields");
        var outputOption = new Option<string?>(new[] { "-o", "--output" }, "Output file path");

        invoiceCommand.AddOption(typeOption);
        invoiceCommand.AddOption(fullOption);
        invoiceCommand.AddOption(outputOption);

        invoiceCommand.SetHandler(async (type, full, output) =>
        {
            var invoiceData = GenerateInvoiceData(type, full);
            var json = JsonSerializer.Serialize(invoiceData, JsonOptions);

            if (!string.IsNullOrEmpty(output))
            {
                await fileWriter.WriteTextAsync(json, output);
                formatter.WriteSuccess($"Sample invoice saved to: {output}");
            }
            else
            {
                formatter.WriteLine(json);
            }
        }, typeOption, fullOption, outputOption);

        return invoiceCommand;
    }

    private static Command CreateCertConfigCommand(IOutputFormatter formatter, FileWriter fileWriter)
    {
        var certConfigCommand = new Command("cert-config", "Generate sample certificate configuration");

        var outputOption = new Option<string?>(new[] { "-o", "--output" }, "Output file path");

        certConfigCommand.AddOption(outputOption);

        certConfigCommand.SetHandler(async (output) =>
        {
            var config = new
            {
                organizationIdentifier = "399999999900003",
                solutionName = "MySolution",
                model = "EGS1",
                serialNumber = "ed22f1d8-e6a2-1118-9b58-d9a8f11e445f",
                commonName = "MySolution-EGS1-399999999900003",
                countryName = "SA",
                organizationName = "My Company LLC",
                organizationalUnitName = "IT Department",
                address = "King Fahd Road, Riyadh, Saudi Arabia",
                invoiceType = 1100,
                businessCategory = "IT Services",
                isProduction = false
            };

            var json = JsonSerializer.Serialize(config, JsonOptions);

            if (!string.IsNullOrEmpty(output))
            {
                await fileWriter.WriteTextAsync(json, output);
                formatter.WriteSuccess($"Sample config saved to: {output}");
            }
            else
            {
                formatter.WriteLine(json);
            }
        }, outputOption);

        return certConfigCommand;
    }

    private static object GenerateInvoiceData(string type, bool full)
    {
        // Determine invoice type code and name per ZATCA specifications
        // InvoiceTypeCode: 388 = tax invoice, 381 = credit note, 383 = debit note
        // Name attribute: 7-character code where:
        //   Position 1-2: Invoice category (01 = standard B2B, 02 = simplified B2C)
        //   Position 3: Third-party transactions (0 = no)
        //   Position 4: Nominal invoice (0 = no)
        //   Position 5: Export (0 = no)
        //   Position 6: Summary (0 = no)
        //   Position 7: Self-billed (0 = no)
        var isSimplified = type.ToLowerInvariant() == "simplified";
        var isExport = type.ToLowerInvariant() == "export";

        var invoiceTypeCode = type.ToLowerInvariant() switch
        {
            "credit" => "381",
            "debit" => "383",
            _ => "388"
        };

        // Build the 7-character name code
        var categoryCode = isSimplified ? "02" : "01";
        var exportFlag = isExport ? "1" : "0";
        var invoiceTypeName = $"{categoryCode}000{exportFlag}0";

        var invoiceType = new Dictionary<string, object>
        {
            { "typeCode", invoiceTypeCode },      // 388, 381, or 383
            { "name", invoiceTypeName }           // 0200000 for simplified, 0100000 for standard
        };

        var baseInvoice = new Dictionary<string, object>
        {
            { "uuid", Guid.NewGuid().ToString() },
            { "id", $"INV-{DateTime.Now:yyyyMMdd}-001" },
            { "issueDate", DateTime.Now.ToString("yyyy-MM-dd") },
            { "issueTime", DateTime.Now.ToString("HH:mm:ss") },
            { "currencyCode", "SAR" },
            { "taxCurrencyCode", "SAR" },
            { "invoiceType", invoiceType },
            { "additionalDocuments", new List<object>
                {
                    new Dictionary<string, object> { { "id", "ICV" }, { "uuid", "10" } },
                    new Dictionary<string, object>
                    {
                        { "id", "PIH" },
                        { "attachment", new Dictionary<string, object>
                            {
                                { "embeddedDocument", "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==" },
                                { "mimeCode", "text/plain" }
                            }
                        }
                    }
                }
            },
            // Add signature reference (required by ZATCA)
            { "signature", new Dictionary<string, object>
                {
                    { "id", "urn:oasis:names:specification:ubl:signature:Invoice" },
                    { "signatureMethod", "urn:oasis:names:specification:ubl:dsig:enveloped:xades" }
                }
            },
            { "supplier", new Dictionary<string, object>
                {
                    { "registrationName", "شركة توريد التكنولوجيا بأقصى سرعة المحدودة | Maximum Speed Tech Supply LTD" },
                    { "taxId", "399999999900003" },
                    { "partyIdentification", "1010010000" },
                    { "partyIdentificationId", "CRN" },
                    { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } },
                    { "address", new Dictionary<string, object>
                        {
                            { "street", "الامير سلطان | Prince Sultan" },
                            { "buildingNumber", "2322" },
                            { "citySubdivisionName", "المربع | Al-Murabba" },
                            { "city", "الرياض | Riyadh" },
                            { "postalZone", "23333" },
                            { "country", "SA" }
                        }
                    }
                }
            },
            { "paymentMeans", new Dictionary<string, object> { { "code", "10" } } },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", 15.0 },
                    { "subTotals", new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                { "taxableAmount", 100.0 },
                                { "taxAmount", 15.0 },
                                { "taxCategory", new Dictionary<string, object>
                                    {
                                        { "id", "S" },  // S = Standard rate
                                        { "percent", 15.0 },
                                        { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 100.0 },
                    { "taxExclusiveAmount", 100.0 },
                    { "taxInclusiveAmount", 115.0 },
                    { "prepaidAmount", 0.0 },
                    { "payableAmount", 115.0 },
                    { "allowanceTotalAmount", 0.0 }
                }
            },
            { "invoiceLines", new List<object>
                {
                    new Dictionary<string, object>
                    {
                        { "id", "1" },
                        { "unitCode", "PCE" },
                        { "quantity", 1.0 },
                        { "lineExtensionAmount", 100.0 },
                        { "item", new Dictionary<string, object>
                            {
                                { "name", "Test Product" },
                                { "classifiedTaxCategory", new List<object>
                                    {
                                        new Dictionary<string, object>
                                        {
                                            { "id", "S" },  // S = Standard rate
                                            { "percent", 15.0 },
                                            { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                                        }
                                    }
                                }
                            }
                        },
                        { "price", new Dictionary<string, object>
                            {
                                { "amount", 100.0 },
                                { "baseQuantity", 1.0 }
                            }
                        },
                        { "taxTotal", new Dictionary<string, object>
                            {
                                { "taxAmount", 15.0 },
                                { "roundingAmount", 115.0 }
                            }
                        }
                    }
                }
            }
        };

        // Add customer - simplified invoices have empty customer, standard have full
        if (type.ToLowerInvariant() != "simplified")
        {
            baseInvoice["customer"] = new Dictionary<string, object>
            {
                { "registrationName", "Test Customer LLC" },
                { "taxId", "311111111100003" },
                { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } },
                { "address", new Dictionary<string, object>
                    {
                        { "street", "Customer Street" },
                        { "buildingNumber", "5678" },
                        { "citySubdivisionName", "Al-Murooj" },
                        { "city", "Jeddah" },
                        { "postalZone", "23456" },
                        { "country", "SA" }
                    }
                }
            };
        }
        else
        {
            // Simplified invoices still need AccountingCustomerParty element but it's empty
            baseInvoice["customer"] = new Dictionary<string, object>();
        }

        // Add billing reference for debit/credit notes
        if (type == "debit" || type == "credit")
        {
            baseInvoice["billingReferences"] = new List<object>
            {
                new Dictionary<string, object> { { "id", "INV-REF-001" } }
            };
        }

        // Add optional fields
        if (full)
        {
            baseInvoice["note"] = "Sample invoice for testing";
            baseInvoice["languageID"] = "en";
            baseInvoice["delivery"] = new Dictionary<string, object>
            {
                { "actualDeliveryDate", DateTime.Now.ToString("yyyy-MM-dd") }
            };
            baseInvoice["allowanceCharges"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    { "chargeIndicator", "false" },
                    { "allowanceChargeReason", "discount" },
                    { "amount", 0.0 },
                    { "taxCategories", new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                { "id", "S" },  // Standard rate (15%)
                                { "percent", 15.0 },
                                { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                            }
                        }
                    }
                }
            };
        }

        return baseInvoice;
    }
}
