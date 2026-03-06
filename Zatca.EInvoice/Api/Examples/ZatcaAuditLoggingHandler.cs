using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zatca.EInvoice.Api.Examples
{
    /// <summary>
    /// Example DelegatingHandler implementation for comprehensive audit logging of ZATCA API calls.
    /// This handler captures full request/response details for compliance audit trails.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <code>
    /// var auditHandler = new ZatcaAuditLoggingHandler(auditService);
    /// 
    /// var options = new ZatcaApiClientOptions
    /// {
    ///     Environment = ZatcaEnvironment.Production,
    ///     Handlers = new List&lt;DelegatingHandler&gt; { auditHandler }
    /// };
    /// 
    /// var client = new ZatcaApiClient(options);
    /// </code>
    /// </remarks>
    public class ZatcaAuditLoggingHandler : DelegatingHandler
    {
        private readonly IZatcaAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaAuditLoggingHandler"/> class.
        /// </summary>
        /// <param name="auditService">The audit service to log API calls to.</param>
        public ZatcaAuditLoggingHandler(IZatcaAuditService auditService)
        {
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var startTime = DateTimeOffset.UtcNow;
            var requestBody = string.Empty;

            // Capture request body
            if (request.Content != null)
            {
                requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                // Recreate content since ReadAsStringAsync consumes it
                request.Content = new StringContent(
                    requestBody,
                    Encoding.UTF8,
                    request.Content.Headers.ContentType?.MediaType ?? "application/json");
            }

            HttpResponseMessage? response = null;
            Exception? error = null;

            try
            {
                // Execute the request
                response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                error = ex;
                throw;
            }
            finally
            {
                var endTime = DateTimeOffset.UtcNow;
                var responseBody = string.Empty;

                if (response != null)
                {
                    try
                    {
                        responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    }
                    catch
                    {
                        // If reading response fails, log what we have
                    }
                }

                // Log the API call
                await _auditService.LogApiCallAsync(new ZatcaApiCallLog
                {
                    OperationType = DetermineOperationType(request.RequestUri),
                    RequestTimestamp = startTime,
                    ResponseTimestamp = endTime,
                    DurationMs = (endTime - startTime).TotalMilliseconds,
                    HttpMethod = request.Method.ToString(),
                    RequestUrl = request.RequestUri?.ToString() ?? string.Empty,
                    RequestHeaders = SerializeHeaders(request.Headers),
                    RequestPayload = requestBody,
                    HttpStatusCode = response != null ? (int)response.StatusCode : null,
                    ResponseHeaders = response != null ? SerializeHeaders(response.Headers) : null,
                    ResponsePayload = responseBody,
                    IsSuccess = response?.IsSuccessStatusCode ?? false,
                    ErrorDetails = error?.ToString()
                }, cancellationToken);
            }
        }

        /// <summary>
        /// Determines the operation type from the request URI.
        /// </summary>
        private static string DetermineOperationType(Uri? requestUri)
        {
            if (requestUri == null)
                return "Unknown";

            var path = requestUri.AbsolutePath.ToLowerInvariant();

            if (path.Contains("compliance"))
                return "OnboardCompliance";
            if (path.Contains("production/csids"))
                return "UpgradeProduction";
            if (path.Contains("compliance/invoices"))
                return "ValidateCompliance";
            if (path.Contains("invoices/clearance"))
                return "SubmitClearance";
            if (path.Contains("invoices/reporting"))
                return "SubmitReporting";

            return "Unknown";
        }

        /// <summary>
        /// Serializes HTTP headers to a dictionary for logging.
        /// </summary>
        private static Dictionary<string, string> SerializeHeaders(System.Net.Http.Headers.HttpHeaders headers)
        {
            var result = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                result[header.Key] = string.Join(", ", header.Value);
            }
            return result;
        }
    }

    /// <summary>
    /// Interface for ZATCA API audit logging service.
    /// Implement this interface to persist audit logs to your storage system.
    /// </summary>
    public interface IZatcaAuditService
    {
        /// <summary>
        /// Logs a ZATCA API call asynchronously.
        /// </summary>
        /// <param name="log">The API call log details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task LogApiCallAsync(ZatcaApiCallLog log, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a logged ZATCA API call for audit purposes.
    /// </summary>
    public class ZatcaApiCallLog
    {
        /// <summary>
        /// Gets or sets the operation type (e.g., "OnboardCompliance", "SubmitInvoice").
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the request was sent.
        /// </summary>
        public DateTimeOffset RequestTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the response was received.
        /// </summary>
        public DateTimeOffset ResponseTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the duration in milliseconds.
        /// </summary>
        public double DurationMs { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method (GET, POST, PATCH, etc.).
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full request URL.
        /// </summary>
        public string RequestUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the request headers.
        /// </summary>
        public Dictionary<string, string>? RequestHeaders { get; set; }

        /// <summary>
        /// Gets or sets the request payload/body.
        /// </summary>
        public string? RequestPayload { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        public int? HttpStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response headers.
        /// </summary>
        public Dictionary<string, string>? ResponseHeaders { get; set; }

        /// <summary>
        /// Gets or sets the response payload/body.
        /// </summary>
        public string? ResponsePayload { get; set; }

        /// <summary>
        /// Gets or sets whether the request was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets error details if the request failed.
        /// </summary>
        public string? ErrorDetails { get; set; }
    }
}
