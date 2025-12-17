using System.Threading;
using System.Threading.Tasks;

namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// Interface for ZATCA API client operations.
    /// </summary>
    public interface IZatcaApiClient
    {
        /// <summary>
        /// Gets the current environment.
        /// </summary>
        ZatcaEnvironment Environment { get; }

        /// <summary>
        /// Requests a compliance certificate using CSR and OTP.
        /// </summary>
        /// <param name="csr">The Certificate Signing Request content.</param>
        /// <param name="otp">The One-Time Password from ZATCA portal.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The compliance certificate result.</returns>
        Task<ComplianceCertificateResult> RequestComplianceCertificateAsync(
            string csr,
            string otp,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates an invoice for compliance.
        /// </summary>
        /// <param name="signedXml">The signed invoice XML.</param>
        /// <param name="invoiceHash">The invoice hash.</param>
        /// <param name="uuid">The invoice UUID.</param>
        /// <param name="certificate">The compliance certificate.</param>
        /// <param name="secret">The secret key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The validation result.</returns>
        Task<InvoiceSubmissionResult> ValidateInvoiceComplianceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            string certificate,
            string secret,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests a production certificate using compliance request ID.
        /// </summary>
        /// <param name="complianceRequestId">The compliance request ID.</param>
        /// <param name="certificate">The compliance certificate.</param>
        /// <param name="secret">The secret key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The production certificate result.</returns>
        Task<ProductionCertificateResult> RequestProductionCertificateAsync(
            string complianceRequestId,
            string certificate,
            string secret,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Submits a clearance invoice (standard/tax invoice).
        /// </summary>
        /// <param name="signedXml">The signed invoice XML.</param>
        /// <param name="invoiceHash">The invoice hash.</param>
        /// <param name="uuid">The invoice UUID.</param>
        /// <param name="certificate">The production certificate.</param>
        /// <param name="secret">The secret key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The submission result.</returns>
        Task<InvoiceSubmissionResult> SubmitClearanceInvoiceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            string certificate,
            string secret,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Submits a reporting invoice (simplified invoice).
        /// </summary>
        /// <param name="signedXml">The signed invoice XML.</param>
        /// <param name="invoiceHash">The invoice hash.</param>
        /// <param name="uuid">The invoice UUID.</param>
        /// <param name="certificate">The production certificate.</param>
        /// <param name="secret">The secret key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The submission result.</returns>
        Task<InvoiceSubmissionResult> SubmitReportingInvoiceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            string certificate,
            string secret,
            CancellationToken cancellationToken = default);
    }
}
