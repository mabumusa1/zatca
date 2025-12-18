using System.CommandLine;
using Zatca.EInvoice.CLI.Models;
using Zatca.EInvoice.CLI.Output;
using Zatca.EInvoice.CLI.Services;

namespace Zatca.EInvoice.CLI.Commands;

/// <summary>
/// Certificate command handlers.
/// </summary>
public static class CertCommands
{
    public static Command CreateCertCommand(ICertificateService certService, IOutputFormatter formatter, FileWriter fileWriter)
    {
        var certCommand = new Command("cert", "Certificate operations (CSR generation)");

        certCommand.AddCommand(CreateGenerateCommand(certService, formatter, fileWriter));

        return certCommand;
    }

    private static Command CreateGenerateCommand(ICertificateService certService, IOutputFormatter formatter, FileWriter fileWriter)
    {
        var generateCommand = new Command("generate", "Generate CSR and private key");

        var orgIdOption = new Option<string>("--org-id", "Organization identifier (15 digits, starts/ends with 3)") { IsRequired = true };
        var solutionOption = new Option<string>("--solution", "Solution name") { IsRequired = true };
        var modelOption = new Option<string>("--model", "Device model") { IsRequired = true };
        var serialOption = new Option<string>("--serial", "Device serial number") { IsRequired = true };
        var nameOption = new Option<string>("--name", "Common name") { IsRequired = true };
        var countryOption = new Option<string>("--country", () => "SA", "Country code (2 characters)");
        var orgNameOption = new Option<string>("--org-name", "Organization name") { IsRequired = true };
        var orgUnitOption = new Option<string>("--org-unit", "Organizational unit name") { IsRequired = true };
        var addressOption = new Option<string>("--address", "Business address") { IsRequired = true };
        var invoiceTypeOption = new Option<int>("--invoice-type", () => 1100, "Invoice type code (default: 1100 = standard + simplified)");
        var categoryOption = new Option<string>("--category", "Business category") { IsRequired = true };
        var productionOption = new Option<bool>("--production", () => false, "Generate for production environment");
        var outputDirOption = new Option<string?>("--output-dir", "Output directory for files");
        var csrFileOption = new Option<string>("--csr-file", () => "certificate.csr", "CSR output filename");
        var keyFileOption = new Option<string>("--key-file", () => "private.pem", "Private key filename");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        generateCommand.AddOption(orgIdOption);
        generateCommand.AddOption(solutionOption);
        generateCommand.AddOption(modelOption);
        generateCommand.AddOption(serialOption);
        generateCommand.AddOption(nameOption);
        generateCommand.AddOption(countryOption);
        generateCommand.AddOption(orgNameOption);
        generateCommand.AddOption(orgUnitOption);
        generateCommand.AddOption(addressOption);
        generateCommand.AddOption(invoiceTypeOption);
        generateCommand.AddOption(categoryOption);
        generateCommand.AddOption(productionOption);
        generateCommand.AddOption(outputDirOption);
        generateCommand.AddOption(csrFileOption);
        generateCommand.AddOption(keyFileOption);
        generateCommand.AddOption(jsonOption);

        generateCommand.SetHandler(async (context) =>
        {
            var config = new CertificateConfig
            {
                OrganizationIdentifier = context.ParseResult.GetValueForOption(orgIdOption)!,
                SolutionName = context.ParseResult.GetValueForOption(solutionOption)!,
                Model = context.ParseResult.GetValueForOption(modelOption)!,
                SerialNumber = context.ParseResult.GetValueForOption(serialOption)!,
                CommonName = context.ParseResult.GetValueForOption(nameOption)!,
                CountryName = context.ParseResult.GetValueForOption(countryOption)!,
                OrganizationName = context.ParseResult.GetValueForOption(orgNameOption)!,
                OrganizationalUnitName = context.ParseResult.GetValueForOption(orgUnitOption)!,
                Address = context.ParseResult.GetValueForOption(addressOption)!,
                InvoiceType = context.ParseResult.GetValueForOption(invoiceTypeOption),
                BusinessCategory = context.ParseResult.GetValueForOption(categoryOption)!,
                IsProduction = context.ParseResult.GetValueForOption(productionOption)
            };

            var outputDir = context.ParseResult.GetValueForOption(outputDirOption);
            var csrFile = context.ParseResult.GetValueForOption(csrFileOption)!;
            var keyFile = context.ParseResult.GetValueForOption(keyFileOption)!;
            var jsonOutput = context.ParseResult.GetValueForOption(jsonOption);

            var result = await certService.GenerateAndSaveAsync(config, outputDir, csrFile, keyFile);

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    csrFile = result.Data?.CsrFilePath,
                    keyFile = result.Data?.PrivateKeyFilePath,
                    csrLength = result.Data?.Csr?.Length,
                    keyLength = result.Data?.PrivateKey?.Length
                });
            }
            else
            {
                if (result.Success)
                {
                    formatter.WriteHeader("Certificate Generation");
                    formatter.WriteSuccess("CSR and private key generated successfully");
                    formatter.WriteKeyValue("CSR File", result.Data?.CsrFilePath);
                    formatter.WriteKeyValue("Private Key File", result.Data?.PrivateKeyFilePath);
                    formatter.WriteKeyValue("CSR Length", $"{result.Data?.Csr?.Length} characters");
                    formatter.WriteKeyValue("Environment", config.IsProduction ? "Production" : "Test/Simulation");
                    formatter.WriteLine();
                    formatter.WriteInfo("CSR Preview (first 100 chars):");
                    formatter.WriteLine(result.Data?.Csr?.Substring(0, Math.Min(100, result.Data.Csr.Length)) + "...");
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                    context.ExitCode = 1;
                }
            }
        });

        return generateCommand;
    }
}
