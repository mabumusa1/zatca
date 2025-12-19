using System.Collections.Generic;

namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// Holds the compliance certificate response data from ZATCA API.
    /// </summary>
    public sealed class ComplianceCertificateResult
    {
        /// <summary>
        /// Gets the request ID.
        /// </summary>
        public string RequestId { get; }

        /// <summary>
        /// Gets the disposition message.
        /// </summary>
        public string DispositionMessage { get; }

        /// <summary>
        /// Gets the binary security token (certificate).
        /// </summary>
        public string BinarySecurityToken { get; }

        /// <summary>
        /// Gets the secret key for authentication.
        /// </summary>
        public string Secret { get; }

        /// <summary>
        /// Gets the list of errors.
        /// </summary>
        public List<string> Errors { get; }

        /// <summary>
        /// Gets the list of warnings.
        /// </summary>
        public List<string> Warnings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplianceCertificateResult"/> class.
        /// </summary>
        /// <param name="binarySecurityToken">The certificate.</param>
        /// <param name="secret">The secret key.</param>
        /// <param name="requestId">The request ID.</param>
        public ComplianceCertificateResult(string binarySecurityToken, string secret, string requestId)
            : this(binarySecurityToken, secret, requestId, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplianceCertificateResult"/> class.
        /// </summary>
        /// <param name="binarySecurityToken">The certificate.</param>
        /// <param name="secret">The secret key.</param>
        /// <param name="requestId">The request ID.</param>
        /// <param name="dispositionMessage">The disposition message.</param>
        /// <param name="errors">The list of errors.</param>
        /// <param name="warnings">The list of warnings.</param>
        public ComplianceCertificateResult(
            string binarySecurityToken,
            string secret,
            string requestId,
            string? dispositionMessage,
            List<string>? errors,
            List<string>? warnings)
        {
            BinarySecurityToken = binarySecurityToken;
            Secret = secret;
            RequestId = requestId;
            DispositionMessage = dispositionMessage ?? string.Empty;
            Errors = errors ?? new List<string>();
            Warnings = warnings ?? new List<string>();
        }

        /// <summary>
        /// Gets a value indicating whether the request was successful.
        /// </summary>
        public bool IsSuccess => Errors.Count == 0;

        /// <summary>
        /// Gets a value indicating whether there are warnings.
        /// </summary>
        public bool HasWarnings => Warnings.Count > 0;
    }
}
