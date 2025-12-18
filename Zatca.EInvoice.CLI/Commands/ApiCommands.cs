using System.CommandLine;
using Zatca.EInvoice.Api;
using Zatca.EInvoice.CLI.Output;
using Zatca.EInvoice.CLI.Services;

namespace Zatca.EInvoice.CLI.Commands;

/// <summary>
/// API command handlers.
/// </summary>
public static class ApiCommands
{
    public static Command CreateApiCommand(IApiService apiService, IOutputFormatter formatter, FileWriter fileWriter)
    {
        var apiCommand = new Command("api", "ZATCA API operations");

        apiCommand.AddCommand(CreateComplianceCertCommand(apiService, formatter, fileWriter));
        apiCommand.AddCommand(CreateComplianceCheckCommand(apiService, formatter));
        apiCommand.AddCommand(CreateProductionCertCommand(apiService, formatter, fileWriter));
        apiCommand.AddCommand(CreateClearanceCommand(apiService, formatter, fileWriter));
        apiCommand.AddCommand(CreateReportingCommand(apiService, formatter));

        return apiCommand;
    }

    private static ZatcaEnvironment ParseEnvironment(string env)
    {
        return env.ToLowerInvariant() switch
        {
            "sandbox" => ZatcaEnvironment.Sandbox,
            "simulation" => ZatcaEnvironment.Simulation,
            "production" => ZatcaEnvironment.Production,
            _ => ZatcaEnvironment.Simulation
        };
    }

    private static Command CreateComplianceCertCommand(IApiService apiService, IOutputFormatter formatter, FileWriter fileWriter)
    {
        var command = new Command("compliance-cert", "Request compliance certificate from ZATCA");

        var csrOption = new Option<string>("--csr", "CSR file path") { IsRequired = true };
        var otpOption = new Option<string>("--otp", "One-time password from ZATCA") { IsRequired = true };
        var envOption = new Option<string>("--env", () => "simulation", "Environment: sandbox|simulation|production");
        var outputOption = new Option<string?>(new[] { "-o", "--output" }, "Output directory for certificate and secret");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        command.AddOption(csrOption);
        command.AddOption(otpOption);
        command.AddOption(envOption);
        command.AddOption(outputOption);
        command.AddOption(jsonOption);

        command.SetHandler(async (context) =>
        {
            var csrPath = context.ParseResult.GetValueForOption(csrOption)!;
            var otp = context.ParseResult.GetValueForOption(otpOption)!;
            var env = context.ParseResult.GetValueForOption(envOption)!;
            var outputDir = context.ParseResult.GetValueForOption(outputOption);
            var jsonOutput = context.ParseResult.GetValueForOption(jsonOption);

            if (!File.Exists(csrPath))
            {
                formatter.WriteError($"CSR file not found: {csrPath}");
                context.ExitCode = 1;
                return;
            }

            var csr = await File.ReadAllTextAsync(csrPath);
            var environment = ParseEnvironment(env);

            var result = await apiService.RequestComplianceCertificateAsync(csr, otp, environment);

            string? certFile = null, secretFile = null;
            if (result.Success && !string.IsNullOrEmpty(outputDir))
            {
                fileWriter.EnsureDirectory(outputDir);
                certFile = await fileWriter.WriteCertificateAsync(result.Data!.BinarySecurityToken, Path.Combine(outputDir, "compliance.crt"));
                secretFile = await fileWriter.WriteTextAsync(result.Data!.Secret, Path.Combine(outputDir, "secret.txt"));
                await fileWriter.WriteTextAsync(result.Data!.RequestId ?? "", Path.Combine(outputDir, "request_id.txt"));
            }

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    requestId = result.Data?.RequestId,
                    certificate = result.Data?.BinarySecurityToken,
                    secret = result.Data?.Secret,
                    files = new { certificate = certFile, secret = secretFile },
                    warnings = result.Warnings
                });
            }
            else
            {
                formatter.WriteHeader("Compliance Certificate Request");

                if (result.Success)
                {
                    formatter.WriteSuccess("Compliance certificate received");
                    formatter.WriteKeyValue("Request ID", result.Data?.RequestId);
                    formatter.WriteKeyValue("Certificate Length", $"{result.Data?.BinarySecurityToken?.Length} characters");

                    if (!string.IsNullOrEmpty(certFile))
                    {
                        formatter.WriteKeyValue("Certificate saved to", certFile);
                        formatter.WriteKeyValue("Secret saved to", secretFile);
                    }

                    foreach (var warning in result.Warnings)
                    {
                        formatter.WriteWarning(warning);
                    }
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                    context.ExitCode = 1;
                }
            }
        });

        return command;
    }

    private static Command CreateComplianceCheckCommand(IApiService apiService, IOutputFormatter formatter)
    {
        var command = new Command("compliance-check", "Validate invoice compliance with ZATCA");

        var inputOption = new Option<string>(new[] { "-i", "--input" }, "Signed invoice XML file") { IsRequired = true };
        var hashOption = new Option<string>("--hash", "Invoice hash") { IsRequired = true };
        var uuidOption = new Option<string>("--uuid", "Invoice UUID") { IsRequired = true };
        var certOption = new Option<string>("--cert", "Compliance certificate file") { IsRequired = true };
        var secretOption = new Option<string>("--secret", "Secret key") { IsRequired = true };
        var envOption = new Option<string>("--env", () => "simulation", "Environment: sandbox|simulation|production");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        command.AddOption(inputOption);
        command.AddOption(hashOption);
        command.AddOption(uuidOption);
        command.AddOption(certOption);
        command.AddOption(secretOption);
        command.AddOption(envOption);
        command.AddOption(jsonOption);

        command.SetHandler(async (context) =>
        {
            var inputPath = context.ParseResult.GetValueForOption(inputOption)!;
            var hash = context.ParseResult.GetValueForOption(hashOption)!;
            var uuid = context.ParseResult.GetValueForOption(uuidOption)!;
            var certPath = context.ParseResult.GetValueForOption(certOption)!;
            var secret = context.ParseResult.GetValueForOption(secretOption)!;
            var env = context.ParseResult.GetValueForOption(envOption)!;
            var jsonOutput = context.ParseResult.GetValueForOption(jsonOption);

            if (!File.Exists(inputPath))
            {
                formatter.WriteError($"Invoice file not found: {inputPath}");
                context.ExitCode = 1;
                return;
            }

            var signedXml = await File.ReadAllTextAsync(inputPath);
            var certificate = File.Exists(certPath) ? await File.ReadAllTextAsync(certPath) : certPath;
            var environment = ParseEnvironment(env);

            var result = await apiService.ValidateComplianceAsync(signedXml, hash, uuid, certificate, secret, environment);

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    status = result.Data?.Status,
                    clearanceStatus = result.Data?.ClearanceStatus,
                    reportingStatus = result.Data?.ReportingStatus,
                    warnings = result.Warnings,
                    info = result.InfoMessages
                });
            }
            else
            {
                formatter.WriteHeader("Compliance Check");

                if (result.Success)
                {
                    formatter.WriteSuccess("Compliance check completed");
                    formatter.WriteKeyValue("Status", result.Data?.Status);
                    formatter.WriteKeyValue("Clearance Status", result.Data?.ClearanceStatus);
                    formatter.WriteKeyValue("Reporting Status", result.Data?.ReportingStatus);

                    foreach (var warning in result.Warnings)
                    {
                        formatter.WriteWarning(warning);
                    }
                    foreach (var info in result.InfoMessages)
                    {
                        formatter.WriteInfo(info);
                    }
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                    context.ExitCode = 1;
                }
            }
        });

        return command;
    }

    private static Command CreateProductionCertCommand(IApiService apiService, IOutputFormatter formatter, FileWriter fileWriter)
    {
        var command = new Command("production-cert", "Request production certificate from ZATCA");

        var requestIdOption = new Option<string>("--request-id", "Compliance request ID") { IsRequired = true };
        var certOption = new Option<string>("--cert", "Compliance certificate file") { IsRequired = true };
        var secretOption = new Option<string>("--secret", "Secret key") { IsRequired = true };
        var envOption = new Option<string>("--env", () => "simulation", "Environment: sandbox|simulation|production");
        var outputOption = new Option<string?>(new[] { "-o", "--output" }, "Output directory");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        command.AddOption(requestIdOption);
        command.AddOption(certOption);
        command.AddOption(secretOption);
        command.AddOption(envOption);
        command.AddOption(outputOption);
        command.AddOption(jsonOption);

        command.SetHandler(async (context) =>
        {
            var requestId = context.ParseResult.GetValueForOption(requestIdOption)!;
            var certPath = context.ParseResult.GetValueForOption(certOption)!;
            var secret = context.ParseResult.GetValueForOption(secretOption)!;
            var env = context.ParseResult.GetValueForOption(envOption)!;
            var outputDir = context.ParseResult.GetValueForOption(outputOption);
            var jsonOutput = context.ParseResult.GetValueForOption(jsonOption);

            var certificate = File.Exists(certPath) ? await File.ReadAllTextAsync(certPath) : certPath;
            var environment = ParseEnvironment(env);

            var result = await apiService.RequestProductionCertificateAsync(requestId, certificate, secret, environment);

            string? certFile = null, secretFile = null;
            if (result.Success && !string.IsNullOrEmpty(outputDir))
            {
                fileWriter.EnsureDirectory(outputDir);
                certFile = await fileWriter.WriteCertificateAsync(result.Data!.BinarySecurityToken, Path.Combine(outputDir, "production.crt"));
                secretFile = await fileWriter.WriteTextAsync(result.Data!.Secret, Path.Combine(outputDir, "production_secret.txt"));
            }

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    certificate = result.Data?.BinarySecurityToken,
                    secret = result.Data?.Secret,
                    files = new { certificate = certFile, secret = secretFile },
                    warnings = result.Warnings
                });
            }
            else
            {
                formatter.WriteHeader("Production Certificate Request");

                if (result.Success)
                {
                    formatter.WriteSuccess("Production certificate received");
                    formatter.WriteKeyValue("Certificate Length", $"{result.Data?.BinarySecurityToken?.Length} characters");

                    if (!string.IsNullOrEmpty(certFile))
                    {
                        formatter.WriteKeyValue("Certificate saved to", certFile);
                        formatter.WriteKeyValue("Secret saved to", secretFile);
                    }

                    foreach (var warning in result.Warnings)
                    {
                        formatter.WriteWarning(warning);
                    }
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                    context.ExitCode = 1;
                }
            }
        });

        return command;
    }

    private static Command CreateClearanceCommand(IApiService apiService, IOutputFormatter formatter, FileWriter fileWriter)
    {
        var command = new Command("clearance", "Submit clearance invoice (B2B) to ZATCA");

        var inputOption = new Option<string>(new[] { "-i", "--input" }, "Signed invoice XML file") { IsRequired = true };
        var hashOption = new Option<string>("--hash", "Invoice hash") { IsRequired = true };
        var uuidOption = new Option<string>("--uuid", "Invoice UUID") { IsRequired = true };
        var certOption = new Option<string>("--cert", "Production certificate file") { IsRequired = true };
        var secretOption = new Option<string>("--secret", "Secret key") { IsRequired = true };
        var envOption = new Option<string>("--env", () => "simulation", "Environment: sandbox|simulation|production");
        var outputOption = new Option<string?>(new[] { "-o", "--output" }, "Output file for cleared invoice");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        command.AddOption(inputOption);
        command.AddOption(hashOption);
        command.AddOption(uuidOption);
        command.AddOption(certOption);
        command.AddOption(secretOption);
        command.AddOption(envOption);
        command.AddOption(outputOption);
        command.AddOption(jsonOption);

        command.SetHandler(async (context) =>
        {
            var inputPath = context.ParseResult.GetValueForOption(inputOption)!;
            var hash = context.ParseResult.GetValueForOption(hashOption)!;
            var uuid = context.ParseResult.GetValueForOption(uuidOption)!;
            var certPath = context.ParseResult.GetValueForOption(certOption)!;
            var secret = context.ParseResult.GetValueForOption(secretOption)!;
            var env = context.ParseResult.GetValueForOption(envOption)!;
            var outputPath = context.ParseResult.GetValueForOption(outputOption);
            var jsonOutput = context.ParseResult.GetValueForOption(jsonOption);

            if (!File.Exists(inputPath))
            {
                formatter.WriteError($"Invoice file not found: {inputPath}");
                context.ExitCode = 1;
                return;
            }

            var signedXml = await File.ReadAllTextAsync(inputPath);
            var certificate = File.Exists(certPath) ? await File.ReadAllTextAsync(certPath) : certPath;
            var environment = ParseEnvironment(env);

            var result = await apiService.SubmitClearanceAsync(signedXml, hash, uuid, certificate, secret, environment);

            string? savedPath = null;
            if (result.Success && !string.IsNullOrEmpty(outputPath) && !string.IsNullOrEmpty(result.Data?.ClearedInvoice))
            {
                savedPath = await fileWriter.WriteXmlAsync(result.Data.ClearedInvoice, outputPath);
            }

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    status = result.Data?.Status,
                    clearanceStatus = result.Data?.ClearanceStatus,
                    clearedInvoiceSavedTo = savedPath,
                    warnings = result.Warnings,
                    info = result.InfoMessages
                });
            }
            else
            {
                formatter.WriteHeader("Clearance Invoice Submission");

                if (result.Success)
                {
                    formatter.WriteSuccess("Clearance submission completed");
                    formatter.WriteKeyValue("Status", result.Data?.Status);
                    formatter.WriteKeyValue("Clearance Status", result.Data?.ClearanceStatus);

                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        formatter.WriteKeyValue("Cleared invoice saved to", savedPath);
                    }

                    foreach (var warning in result.Warnings)
                    {
                        formatter.WriteWarning(warning);
                    }
                    foreach (var info in result.InfoMessages)
                    {
                        formatter.WriteInfo(info);
                    }
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                    context.ExitCode = 1;
                }
            }
        });

        return command;
    }

    private static Command CreateReportingCommand(IApiService apiService, IOutputFormatter formatter)
    {
        var command = new Command("reporting", "Submit reporting invoice (B2C) to ZATCA");

        var inputOption = new Option<string>(new[] { "-i", "--input" }, "Signed invoice XML file") { IsRequired = true };
        var hashOption = new Option<string>("--hash", "Invoice hash") { IsRequired = true };
        var uuidOption = new Option<string>("--uuid", "Invoice UUID") { IsRequired = true };
        var certOption = new Option<string>("--cert", "Production certificate file") { IsRequired = true };
        var secretOption = new Option<string>("--secret", "Secret key") { IsRequired = true };
        var envOption = new Option<string>("--env", () => "simulation", "Environment: sandbox|simulation|production");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        command.AddOption(inputOption);
        command.AddOption(hashOption);
        command.AddOption(uuidOption);
        command.AddOption(certOption);
        command.AddOption(secretOption);
        command.AddOption(envOption);
        command.AddOption(jsonOption);

        command.SetHandler(async (context) =>
        {
            var inputPath = context.ParseResult.GetValueForOption(inputOption)!;
            var hash = context.ParseResult.GetValueForOption(hashOption)!;
            var uuid = context.ParseResult.GetValueForOption(uuidOption)!;
            var certPath = context.ParseResult.GetValueForOption(certOption)!;
            var secret = context.ParseResult.GetValueForOption(secretOption)!;
            var env = context.ParseResult.GetValueForOption(envOption)!;
            var jsonOutput = context.ParseResult.GetValueForOption(jsonOption);

            if (!File.Exists(inputPath))
            {
                formatter.WriteError($"Invoice file not found: {inputPath}");
                context.ExitCode = 1;
                return;
            }

            var signedXml = await File.ReadAllTextAsync(inputPath);
            var certificate = File.Exists(certPath) ? await File.ReadAllTextAsync(certPath) : certPath;
            var environment = ParseEnvironment(env);

            var result = await apiService.SubmitReportingAsync(signedXml, hash, uuid, certificate, secret, environment);

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    success = result.Success,
                    error = result.ErrorMessage,
                    status = result.Data?.Status,
                    reportingStatus = result.Data?.ReportingStatus,
                    warnings = result.Warnings,
                    info = result.InfoMessages
                });
            }
            else
            {
                formatter.WriteHeader("Reporting Invoice Submission");

                if (result.Success)
                {
                    formatter.WriteSuccess("Reporting submission completed");
                    formatter.WriteKeyValue("Status", result.Data?.Status);
                    formatter.WriteKeyValue("Reporting Status", result.Data?.ReportingStatus);

                    foreach (var warning in result.Warnings)
                    {
                        formatter.WriteWarning(warning);
                    }
                    foreach (var info in result.InfoMessages)
                    {
                        formatter.WriteInfo(info);
                    }
                }
                else
                {
                    formatter.WriteError(result.ErrorMessage ?? "Unknown error");
                    context.ExitCode = 1;
                }
            }
        });

        return command;
    }
}
