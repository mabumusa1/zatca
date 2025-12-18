using System.CommandLine;
using System.Security.Cryptography.X509Certificates;
using Zatca.EInvoice.CLI.Output;
using Zatca.EInvoice.CLI.Services;

namespace Zatca.EInvoice.CLI.Commands;

/// <summary>
/// Invoice command handlers.
/// </summary>
public static class InvoiceCommands
{
    public static Command CreateInvoiceCommand(IInvoiceService invoiceService, IOutputFormatter formatter, FileWriter fileWriter)
    {
        var invoiceCommand = new Command("invoice", "Invoice operations (create, validate, xml, sign, hash)");

        invoiceCommand.AddCommand(CreateCreateCommand(invoiceService, formatter));
        invoiceCommand.AddCommand(CreateValidateCommand(invoiceService, formatter));
        invoiceCommand.AddCommand(CreateXmlCommand(invoiceService, formatter, fileWriter));
        invoiceCommand.AddCommand(CreateSignCommand(invoiceService, formatter, fileWriter));
        invoiceCommand.AddCommand(CreateHashCommand(invoiceService, formatter));

        return invoiceCommand;
    }

    private static Command CreateCreateCommand(IInvoiceService invoiceService, IOutputFormatter formatter)
    {
        var createCommand = new Command("create", "Create invoice from JSON file");

        var inputOption = new Option<string>(new[] { "-i", "--input" }, "JSON invoice file path") { IsRequired = true };
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        createCommand.AddOption(inputOption);
        createCommand.AddOption(jsonOption);

        createCommand.SetHandler((input, jsonOutput) =>
        {
            var result = invoiceService.CreateFromJson(input);

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    invoice = result.Success ? new
                    {
                        id = result.Data?.Id,
                        uuid = result.Data?.UUID,
                        issueDate = result.Data?.IssueDate?.ToString(),
                        issueTime = result.Data?.IssueTime?.ToString(),
                        type = result.Data?.InvoiceType?.Invoice,
                        subType = result.Data?.InvoiceType?.InvoiceSubType,
                        currency = result.Data?.InvoiceCurrencyCode,
                        supplier = result.Data?.AccountingSupplierParty?.LegalEntity?.RegistrationName,
                        customer = result.Data?.AccountingCustomerParty?.LegalEntity?.RegistrationName,
                        taxAmount = result.Data?.TaxTotal?.TaxAmount,
                        totalAmount = result.Data?.LegalMonetaryTotal?.TaxInclusiveAmount,
                        lineCount = result.Data?.InvoiceLines?.Count
                    } : null
                });
            }
            else
            {
                if (result.Success)
                {
                    var invoice = result.Data!;
                    formatter.WriteHeader("Invoice Created");
                    formatter.WriteSuccess("Invoice loaded successfully");
                    formatter.WriteKeyValue("ID", invoice.Id);
                    formatter.WriteKeyValue("UUID", invoice.UUID);
                    formatter.WriteKeyValue("Issue Date", invoice.IssueDate?.ToString("yyyy-MM-dd"));
                    formatter.WriteKeyValue("Issue Time", invoice.IssueTime?.ToString("HH:mm:ss"));
                    formatter.WriteKeyValue("Type", $"{invoice.InvoiceType?.Invoice} - {invoice.InvoiceType?.InvoiceSubType}");
                    formatter.WriteKeyValue("Currency", invoice.InvoiceCurrencyCode);
                    formatter.WriteKeyValue("Supplier", invoice.AccountingSupplierParty?.LegalEntity?.RegistrationName);
                    formatter.WriteKeyValue("Customer", invoice.AccountingCustomerParty?.LegalEntity?.RegistrationName);
                    formatter.WriteKeyValue("Tax Amount", $"{invoice.TaxTotal?.TaxAmount:N2}");
                    formatter.WriteKeyValue("Total Amount", $"{invoice.LegalMonetaryTotal?.TaxInclusiveAmount:N2}");
                    formatter.WriteKeyValue("Line Count", invoice.InvoiceLines?.Count.ToString());
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                }
            }
        }, inputOption, jsonOption);

        return createCommand;
    }

    private static Command CreateValidateCommand(IInvoiceService invoiceService, IOutputFormatter formatter)
    {
        var validateCommand = new Command("validate", "Validate invoice data from JSON file");

        var inputOption = new Option<string>(new[] { "-i", "--input" }, "JSON invoice file path") { IsRequired = true };
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        validateCommand.AddOption(inputOption);
        validateCommand.AddOption(jsonOption);

        validateCommand.SetHandler((input, jsonOutput) =>
        {
            var result = invoiceService.ValidateFromJson(input);

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    success = result.Success,
                    valid = result.Success && (result.Data?.Count ?? 0) == 0,
                    errorCount = result.Data?.Count ?? 0,
                    errors = result.Data
                });
            }
            else
            {
                formatter.WriteHeader("Invoice Validation");

                if (!result.Success)
                {
                    formatter.WriteError(result.ErrorMessage ?? "Validation failed");
                    return;
                }

                if (result.Data?.Count == 0)
                {
                    formatter.WriteSuccess("Invoice is valid - no errors found");
                }
                else
                {
                    formatter.WriteError($"Found {result.Data?.Count} validation error(s):");
                    foreach (var error in result.Data!)
                    {
                        formatter.WriteWarning($"  - {error}");
                    }
                }
            }
        }, inputOption, jsonOption);

        return validateCommand;
    }

    private static Command CreateXmlCommand(IInvoiceService invoiceService, IOutputFormatter formatter, FileWriter fileWriter)
    {
        var xmlCommand = new Command("xml", "Generate UBL XML from JSON file");

        var inputOption = new Option<string>(new[] { "-i", "--input" }, "JSON invoice file path") { IsRequired = true };
        var outputOption = new Option<string?>(new[] { "-o", "--output" }, "XML output file path");
        var currencyOption = new Option<string>("--currency", () => "SAR", "Currency code");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        xmlCommand.AddOption(inputOption);
        xmlCommand.AddOption(outputOption);
        xmlCommand.AddOption(currencyOption);
        xmlCommand.AddOption(jsonOption);

        xmlCommand.SetHandler(async (input, output, currency, jsonOutput) =>
        {
            var result = invoiceService.GenerateXml(input, currency);

            if (jsonOutput)
            {
                string? savedPath = null;
                if (!string.IsNullOrEmpty(output) && result.Success)
                {
                    savedPath = await fileWriter.WriteXmlAsync(result.Data!, output);
                }

                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    xmlLength = result.Data?.Length,
                    savedTo = savedPath
                });
            }
            else
            {
                formatter.WriteHeader("XML Generation");

                if (result.Success)
                {
                    formatter.WriteSuccess($"XML generated successfully ({result.Data?.Length} characters)");

                    if (!string.IsNullOrEmpty(output))
                    {
                        var savedPath = await fileWriter.WriteXmlAsync(result.Data!, output);
                        formatter.WriteKeyValue("Saved to", savedPath);
                    }
                    else
                    {
                        formatter.WriteLine();
                        formatter.WriteInfo("XML Preview (first 500 chars):");
                        formatter.WriteLine(new string('-', 60));
                        formatter.WriteLine(result.Data?.Substring(0, Math.Min(500, result.Data.Length)));
                        if (result.Data?.Length > 500)
                            formatter.WriteLine("...");
                        formatter.WriteLine(new string('-', 60));
                    }
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                }
            }
        }, inputOption, outputOption, currencyOption, jsonOption);

        return xmlCommand;
    }

    private static Command CreateSignCommand(IInvoiceService invoiceService, IOutputFormatter formatter, FileWriter fileWriter)
    {
        var signCommand = new Command("sign", "Sign invoice XML with certificate");

        var inputOption = new Option<string>(new[] { "-i", "--input" }, "Invoice XML file path") { IsRequired = true };
        var certOption = new Option<string>(new[] { "-c", "--cert" }, "Certificate file path (PFX)") { IsRequired = true };
        var passwordOption = new Option<string?>(new[] { "-p", "--password" }, "Certificate password");
        var outputXmlOption = new Option<string?>("--output-xml", "Signed XML output path");
        var outputQrOption = new Option<string?>("--output-qr", "QR code output path");
        var outputHashOption = new Option<string?>("--output-hash", "Hash output path");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        signCommand.AddOption(inputOption);
        signCommand.AddOption(certOption);
        signCommand.AddOption(passwordOption);
        signCommand.AddOption(outputXmlOption);
        signCommand.AddOption(outputQrOption);
        signCommand.AddOption(outputHashOption);
        signCommand.AddOption(jsonOption);

        signCommand.SetHandler(async (context) =>
        {
            var input = context.ParseResult.GetValueForOption(inputOption)!;
            var certPath = context.ParseResult.GetValueForOption(certOption)!;
            var password = context.ParseResult.GetValueForOption(passwordOption);
            var outputXml = context.ParseResult.GetValueForOption(outputXmlOption);
            var outputQr = context.ParseResult.GetValueForOption(outputQrOption);
            var outputHash = context.ParseResult.GetValueForOption(outputHashOption);
            var jsonOutput = context.ParseResult.GetValueForOption(jsonOption);

            // Load XML content
            if (!File.Exists(input))
            {
                formatter.WriteError($"XML file not found: {input}");
                context.ExitCode = 1;
                return;
            }

            var xmlContent = await File.ReadAllTextAsync(input);

            // Load certificate
            X509Certificate2? certificate = null;
            try
            {
                certificate = new X509Certificate2(certPath, password, X509KeyStorageFlags.Exportable);
            }
            catch (Exception ex)
            {
                formatter.WriteError($"Failed to load certificate: {ex.Message}");
                context.ExitCode = 1;
                return;
            }

            var result = invoiceService.SignInvoice(xmlContent, certificate);

            if (jsonOutput)
            {
                string? savedXml = null, savedQr = null, savedHash = null;

                if (result.Success)
                {
                    if (!string.IsNullOrEmpty(outputXml))
                        savedXml = await fileWriter.WriteXmlAsync(result.Data!.SignedXml, outputXml);
                    if (!string.IsNullOrEmpty(outputQr))
                        savedQr = await fileWriter.WriteTextAsync(result.Data!.QrCode, outputQr);
                    if (!string.IsNullOrEmpty(outputHash))
                        savedHash = await fileWriter.WriteTextAsync(result.Data!.Hash, outputHash);
                }

                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    hash = result.Data?.Hash,
                    qrCode = result.Data?.QrCode,
                    signedXmlLength = result.Data?.SignedXml?.Length,
                    files = new { xml = savedXml, qr = savedQr, hash = savedHash }
                });
            }
            else
            {
                formatter.WriteHeader("Invoice Signing");

                if (result.Success)
                {
                    formatter.WriteSuccess("Invoice signed successfully");
                    formatter.WriteKeyValue("Hash", result.Data?.Hash);
                    formatter.WriteKeyValue("QR Code Length", $"{result.Data?.QrCode?.Length} characters");
                    formatter.WriteKeyValue("Signed XML Length", $"{result.Data?.SignedXml?.Length} characters");

                    if (!string.IsNullOrEmpty(outputXml))
                    {
                        var savedPath = await fileWriter.WriteXmlAsync(result.Data!.SignedXml, outputXml);
                        formatter.WriteKeyValue("Signed XML saved to", savedPath);
                    }
                    if (!string.IsNullOrEmpty(outputQr))
                    {
                        var savedPath = await fileWriter.WriteTextAsync(result.Data!.QrCode, outputQr);
                        formatter.WriteKeyValue("QR Code saved to", savedPath);
                    }
                    if (!string.IsNullOrEmpty(outputHash))
                    {
                        var savedPath = await fileWriter.WriteTextAsync(result.Data!.Hash, outputHash);
                        formatter.WriteKeyValue("Hash saved to", savedPath);
                    }
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                    context.ExitCode = 1;
                }
            }

            certificate?.Dispose();
        });

        return signCommand;
    }

    private static Command CreateHashCommand(IInvoiceService invoiceService, IOutputFormatter formatter)
    {
        var hashCommand = new Command("hash", "Compute invoice XML hash");

        var inputOption = new Option<string>(new[] { "-i", "--input" }, "Invoice XML file path") { IsRequired = true };
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        hashCommand.AddOption(inputOption);
        hashCommand.AddOption(jsonOption);

        hashCommand.SetHandler(async (input, jsonOutput) =>
        {
            if (!File.Exists(input))
            {
                formatter.WriteError($"File not found: {input}");
                return;
            }

            var xmlContent = await File.ReadAllTextAsync(input);
            var result = invoiceService.ComputeHash(xmlContent);

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    hash = result.Data
                });
            }
            else
            {
                formatter.WriteHeader("Invoice Hash");

                if (result.Success)
                {
                    formatter.WriteSuccess("Hash computed successfully");
                    formatter.WriteKeyValue("Hash", result.Data);
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                }
            }
        }, inputOption, jsonOption);

        return hashCommand;
    }
}
