using Zatca.EInvoice.Api;
using Zatca.EInvoice.CLI.Models;
using Zatca.EInvoice.Exceptions;

namespace Zatca.EInvoice.CLI.Services;

/// <summary>
/// Service for ZATCA API operations.
/// </summary>
public class ApiService : IApiService
{
    /// <inheritdoc/>
    public async Task<CommandResult<ComplianceCertificateResult>> RequestComplianceCertificateAsync(
        string csr,
        string otp,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation)
    {
        try
        {
            using var client = new ZatcaApiClient(environment);
            client.SetWarningHandling(true);

            var result = await client.RequestComplianceCertificateAsync(csr, otp);

            var commandResult = CommandResult<ComplianceCertificateResult>.Ok(result);

            if (result.Errors?.Count > 0)
            {
                foreach (var error in result.Errors)
                {
                    commandResult.WithWarning($"Error: {error}");
                }
            }

            if (result.Warnings?.Count > 0)
            {
                foreach (var warning in result.Warnings)
                {
                    commandResult.WithWarning(warning);
                }
            }

            return commandResult;
        }
        catch (ZatcaApiException apiEx)
        {
            var errorMsg = $"API request failed: {apiEx.Message}";
            if (!string.IsNullOrEmpty(apiEx.Response))
            {
                errorMsg += $"\nResponse: {apiEx.Response}";
            }
            return CommandResult<ComplianceCertificateResult>.Fail(errorMsg);
        }
        catch (Exception ex)
        {
            return CommandResult<ComplianceCertificateResult>.Fail($"API request failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<CommandResult<InvoiceSubmissionResult>> ValidateComplianceAsync(
        string signedXml,
        string invoiceHash,
        string uuid,
        string certificate,
        string secret,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation)
    {
        try
        {
            using var client = new ZatcaApiClient(environment);
            client.SetWarningHandling(true);

            var result = await client.ValidateInvoiceComplianceAsync(
                signedXml, invoiceHash, uuid, certificate, secret);

            return CreateInvoiceSubmissionResult(result);
        }
        catch (Exception ex)
        {
            return CommandResult<InvoiceSubmissionResult>.Fail($"Compliance validation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<CommandResult<ProductionCertificateResult>> RequestProductionCertificateAsync(
        string complianceRequestId,
        string certificate,
        string secret,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation)
    {
        try
        {
            using var client = new ZatcaApiClient(environment);
            client.SetWarningHandling(true);

            var result = await client.RequestProductionCertificateAsync(
                complianceRequestId, certificate, secret);

            var commandResult = CommandResult<ProductionCertificateResult>.Ok(result);

            if (result.Errors?.Count > 0)
            {
                foreach (var error in result.Errors)
                {
                    commandResult.WithWarning($"Error: {error}");
                }
            }

            if (result.Warnings?.Count > 0)
            {
                foreach (var warning in result.Warnings)
                {
                    commandResult.WithWarning(warning);
                }
            }

            return commandResult;
        }
        catch (Exception ex)
        {
            return CommandResult<ProductionCertificateResult>.Fail($"Production certificate request failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<CommandResult<InvoiceSubmissionResult>> SubmitClearanceAsync(
        string signedXml,
        string invoiceHash,
        string uuid,
        string certificate,
        string secret,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation)
    {
        try
        {
            using var client = new ZatcaApiClient(environment);
            client.SetWarningHandling(true);

            var result = await client.SubmitClearanceInvoiceAsync(
                signedXml, invoiceHash, uuid, certificate, secret);

            return CreateInvoiceSubmissionResult(result);
        }
        catch (Exception ex)
        {
            return CommandResult<InvoiceSubmissionResult>.Fail($"Clearance submission failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<CommandResult<InvoiceSubmissionResult>> SubmitReportingAsync(
        string signedXml,
        string invoiceHash,
        string uuid,
        string certificate,
        string secret,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation)
    {
        try
        {
            using var client = new ZatcaApiClient(environment);
            client.SetWarningHandling(true);

            var result = await client.SubmitReportingInvoiceAsync(
                signedXml, invoiceHash, uuid, certificate, secret);

            return CreateInvoiceSubmissionResult(result);
        }
        catch (Exception ex)
        {
            return CommandResult<InvoiceSubmissionResult>.Fail($"Reporting submission failed: {ex.Message}");
        }
    }

    private static CommandResult<InvoiceSubmissionResult> CreateInvoiceSubmissionResult(InvoiceSubmissionResult result)
    {
        var commandResult = CommandResult<InvoiceSubmissionResult>.Ok(result);

        if (result.Errors?.Count > 0)
        {
            foreach (var error in result.Errors)
            {
                commandResult.WithWarning($"Error [{error.Code}]: {error.Message}");
            }
        }

        if (result.Warnings?.Count > 0)
        {
            foreach (var warning in result.Warnings)
            {
                commandResult.WithWarning($"Warning [{warning.Code}]: {warning.Message}");
            }
        }

        if (result.InfoMessages?.Count > 0)
        {
            foreach (var info in result.InfoMessages)
            {
                commandResult.WithInfo($"Info [{info.Code}]: {info.Message}");
            }
        }

        return commandResult;
    }
}
