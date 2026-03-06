using System;
using System.Threading.Tasks;

namespace Zatca.EInvoice.Api.Examples
{
    /// <summary>
    /// Example implementation showing how to use ZATCA API client events for audit logging.
    /// This approach uses event hooks instead of DelegatingHandlers.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <code>
    /// var client = new ZatcaApiClient(ZatcaEnvironment.Production);
    /// var eventLogger = new ZatcaEventBasedAuditLogger(auditService);
    /// eventLogger.AttachToClient(client);
    /// 
    /// // Now all API calls will be logged
    /// var result = await client.RequestComplianceCertificateAsync(csr, otp);
    /// </code>
    /// </remarks>
    public class ZatcaEventBasedAuditLogger
    {
        private readonly IZatcaAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaEventBasedAuditLogger"/> class.
        /// </summary>
        /// <param name="auditService">The audit service to log API calls to.</param>
        public ZatcaEventBasedAuditLogger(IZatcaAuditService auditService)
        {
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <summary>
        /// Attaches this logger to a ZATCA API client to log all its API calls.
        /// </summary>
        /// <param name="client">The API client to attach to.</param>
        public void AttachToClient(ZatcaApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            client.BeforeRequest += OnBeforeRequest;
            client.AfterResponse += OnAfterResponse;
        }

        /// <summary>
        /// Detaches this logger from a ZATCA API client.
        /// </summary>
        /// <param name="client">The API client to detach from.</param>
        public void DetachFromClient(ZatcaApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            client.BeforeRequest -= OnBeforeRequest;
            client.AfterResponse -= OnAfterResponse;
        }

        private void OnBeforeRequest(object? sender, ZatcaApiRequestEventArgs e)
        {
            // Optional: Log request initiation
            Console.WriteLine($"[ZATCA API] {e.OperationType} request starting at {e.RequestTimestamp:O}");
        }

        private void OnAfterResponse(object? sender, ZatcaApiResponseEventArgs e)
        {
            // Log the complete API call
            Task.Run(async () =>
            {
                try
                {
                    await _auditService.LogApiCallAsync(new ZatcaApiCallLog
                    {
                        OperationType = e.OperationType,
                        RequestTimestamp = e.RequestTimestamp,
                        ResponseTimestamp = e.ResponseTimestamp,
                        DurationMs = e.Duration.TotalMilliseconds,
                        HttpMethod = e.HttpMethod,
                        RequestUrl = e.RequestUrl,
                        RequestHeaders = e.RequestHeaders,
                        RequestPayload = e.RequestBody,
                        HttpStatusCode = e.StatusCode,
                        ResponseHeaders = e.ResponseHeaders,
                        ResponsePayload = e.ResponseBody,
                        IsSuccess = e.IsSuccess,
                        ErrorDetails = e.Error?.ToString()
                    });

                    Console.WriteLine(
                        $"[ZATCA API] {e.OperationType} completed: " +
                        $"Status={e.StatusCode}, Duration={e.Duration.TotalMilliseconds}ms, Success={e.IsSuccess}");
                }
                catch (Exception ex)
                {
                    // Don't let audit logging failures affect the main operation
                    Console.Error.WriteLine($"[ZATCA API] Audit logging failed: {ex.Message}");
                }
            }).Wait(); // Consider using async events in production
        }
    }

    /// <summary>
    /// Example audit service implementation that logs to a database.
    /// This is a simplified example - implement according to your requirements.
    /// </summary>
    public class DatabaseAuditService : IZatcaAuditService
    {
        // private readonly IDbConnection _dbConnection;

        public async Task LogApiCallAsync(ZatcaApiCallLog log, System.Threading.CancellationToken cancellationToken = default)
        {
            // Example implementation:
            // 1. Sanitize sensitive data if needed (e.g., redact Authorization headers)
            var sanitizedLog = SanitizeSensitiveData(log);

            // 2. Persist to database
            /*
            await _dbConnection.ExecuteAsync(@"
                INSERT INTO ZatcaApiAuditLog (
                    OperationType, RequestTimestamp, ResponseTimestamp, DurationMs,
                    HttpMethod, RequestUrl, RequestHeaders, RequestPayload,
                    HttpStatusCode, ResponseHeaders, ResponsePayload,
                    IsSuccess, ErrorDetails
                ) VALUES (
                    @OperationType, @RequestTimestamp, @ResponseTimestamp, @DurationMs,
                    @HttpMethod, @RequestUrl, @RequestHeaders, @RequestPayload,
                    @HttpStatusCode, @ResponseHeaders, @ResponsePayload,
                    @IsSuccess, @ErrorDetails
                )", sanitizedLog);
            */

            // For demonstration purposes, just log to console
            Console.WriteLine($"[AUDIT] Logged {sanitizedLog.OperationType} call at {sanitizedLog.RequestTimestamp:O}");

            await Task.CompletedTask;
        }

        private static ZatcaApiCallLog SanitizeSensitiveData(ZatcaApiCallLog log)
        {
            var sanitized = new ZatcaApiCallLog
            {
                OperationType = log.OperationType,
                RequestTimestamp = log.RequestTimestamp,
                ResponseTimestamp = log.ResponseTimestamp,
                DurationMs = log.DurationMs,
                HttpMethod = log.HttpMethod,
                RequestUrl = log.RequestUrl,
                RequestHeaders = log.RequestHeaders != null
                    ? new System.Collections.Generic.Dictionary<string, string>(log.RequestHeaders)
                    : null,
                RequestPayload = log.RequestPayload,
                HttpStatusCode = log.HttpStatusCode,
                ResponseHeaders = log.ResponseHeaders,
                ResponsePayload = log.ResponsePayload,
                IsSuccess = log.IsSuccess,
                ErrorDetails = log.ErrorDetails
            };

            // Redact sensitive headers
            if (sanitized.RequestHeaders != null)
            {
                if (sanitized.RequestHeaders.ContainsKey("Authorization"))
                {
                    sanitized.RequestHeaders["Authorization"] = "***REDACTED***";
                }
                if (sanitized.RequestHeaders.ContainsKey("OTP"))
                {
                    sanitized.RequestHeaders["OTP"] = "***REDACTED***";
                }
            }

            return sanitized;
        }
    }
}
