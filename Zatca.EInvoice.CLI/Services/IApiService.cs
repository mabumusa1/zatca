using Zatca.EInvoice.Api;
using Zatca.EInvoice.CLI.Models;

namespace Zatca.EInvoice.CLI.Services;

/// <summary>
/// Service interface for ZATCA API operations.
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Requests a compliance certificate.
    /// </summary>
    Task<CommandResult<ComplianceCertificateResult>> RequestComplianceCertificateAsync(
        string csr,
        string otp,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation);

    /// <summary>
    /// Validates invoice compliance.
    /// </summary>
    Task<CommandResult<InvoiceSubmissionResult>> ValidateComplianceAsync(
        string signedXml,
        string invoiceHash,
        string uuid,
        string certificate,
        string secret,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation);

    /// <summary>
    /// Requests a production certificate.
    /// </summary>
    Task<CommandResult<ProductionCertificateResult>> RequestProductionCertificateAsync(
        string complianceRequestId,
        string certificate,
        string secret,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation);

    /// <summary>
    /// Submits a clearance invoice (B2B).
    /// </summary>
    Task<CommandResult<InvoiceSubmissionResult>> SubmitClearanceAsync(
        string signedXml,
        string invoiceHash,
        string uuid,
        string certificate,
        string secret,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation);

    /// <summary>
    /// Submits a reporting invoice (B2C).
    /// </summary>
    Task<CommandResult<InvoiceSubmissionResult>> SubmitReportingAsync(
        string signedXml,
        string invoiceHash,
        string uuid,
        string certificate,
        string secret,
        ZatcaEnvironment environment = ZatcaEnvironment.Simulation);
}
