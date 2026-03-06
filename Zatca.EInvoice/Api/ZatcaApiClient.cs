using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Zatca.EInvoice.Exceptions;

namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// ZATCA API client for e-invoicing operations.
    /// </summary>
    public class ZatcaApiClient : IZatcaApiClient, IDisposable
    {
        private const string ApplicationJson = "application/json";
        private const string AcceptVersionHeader = "Accept-Version";
        private const string AcceptVersionValue = "V2";
        private const string ContentTypeHeader = "Content-Type";
        private const string ValidationResultsKey = "validationResults";
        private const string MessageKey = "message";

        private readonly HttpClient _httpClient;
        private readonly bool _disposeHttpClient;
        private bool _allowWarnings;
        private bool _disposed;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// Gets the current environment.
        /// </summary>
        public ZatcaEnvironment Environment { get; }

        /// <summary>
        /// Event fired before each API request is sent.
        /// </summary>
        public event EventHandler<ZatcaApiRequestEventArgs>? BeforeRequest;

        /// <summary>
        /// Event fired after each API response is received (or request fails).
        /// </summary>
        public event EventHandler<ZatcaApiResponseEventArgs>? AfterResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaApiClient"/> class.
        /// </summary>
        /// <param name="environment">The ZATCA environment.</param>
        /// <param name="httpClient">Optional HttpClient instance. If not provided, a new one will be created.</param>
        public ZatcaApiClient(ZatcaEnvironment environment, HttpClient? httpClient = null)
        {
            Environment = environment;

            if (httpClient != null)
            {
                _httpClient = httpClient;
                _disposeHttpClient = false;
            }
            else
            {
                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri(ZatcaApiEndpoints.GetBaseUrl(environment)),
                    Timeout = TimeSpan.FromSeconds(30)
                };
                _disposeHttpClient = true;
            }

            // Set default headers
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJson));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaApiClient"/> class with options.
        /// </summary>
        /// <param name="options">Configuration options for the API client.</param>
        public ZatcaApiClient(ZatcaApiClientOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Environment = options.Environment;
            _allowWarnings = options.AllowWarnings;

            if (options.HttpClient != null)
            {
                // Use the provided HttpClient directly
                _httpClient = options.HttpClient;
                _disposeHttpClient = false;
            }
            else
            {
                // Create HttpClient with optional handler pipeline
                if (options.Handlers != null && options.Handlers.Count > 0)
                {
                    // Build handler pipeline: innermost handler first
                    HttpMessageHandler handler = new HttpClientHandler();
                    
                    // Chain handlers in reverse order so the first handler in the list is outermost
                    for (int i = options.Handlers.Count - 1; i >= 0; i--)
                    {
                        var delegatingHandler = options.Handlers[i];
                        delegatingHandler.InnerHandler = handler;
                        handler = delegatingHandler;
                    }

                    _httpClient = new HttpClient(handler, disposeHandler: true)
                    {
                        BaseAddress = new Uri(ZatcaApiEndpoints.GetBaseUrl(options.Environment)),
                        Timeout = options.Timeout
                    };
                }
                else
                {
                    // No handlers, create simple HttpClient
                    _httpClient = new HttpClient
                    {
                        BaseAddress = new Uri(ZatcaApiEndpoints.GetBaseUrl(options.Environment)),
                        Timeout = options.Timeout
                    };
                }

                _disposeHttpClient = true;
            }

            // Set default headers
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJson));
        }

        /// <summary>
        /// Sets whether to allow warnings in responses.
        /// </summary>
        /// <param name="allow">True to allow warnings, false otherwise.</param>
        public void SetWarningHandling(bool allow)
        {
            _allowWarnings = allow;
        }

        /// <inheritdoc/>
        public async Task<ComplianceCertificateResult> RequestComplianceCertificateAsync(
            string csr,
            string otp,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(csr))
                throw new ArgumentNullException(nameof(csr));
            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentNullException(nameof(otp));

            // Remove BOM if present and encode the entire PEM file to base64
            csr = csr.TrimStart('\uFEFF');
            var csrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(csr));

            var operationType = "OnboardCompliance";
            var startTime = DateTimeOffset.UtcNow;
            HttpResponseMessage? response = null;
            string? requestBody = null;
            string? responseBody = null;
            Exception? error = null;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, ZatcaApiEndpoints.ComplianceCertificate);
                request.Headers.TryAddWithoutValidation("OTP", otp);
                request.Headers.TryAddWithoutValidation(AcceptVersionHeader, AcceptVersionValue);

                var json = JsonSerializer.Serialize(new { csr = csrBase64 }, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, ApplicationJson);
                requestBody = json;

                // Fire BeforeRequest event
                var requestEventArgs = new ZatcaApiRequestEventArgs
                {
                    OperationType = operationType,
                    HttpMethod = HttpMethod.Post.ToString(),
                    RequestUrl = BuildFullUrl(ZatcaApiEndpoints.ComplianceCertificate),
                    RequestHeaders = ExtractHeaders(request.Headers, request.Content?.Headers),
                    RequestBody = requestBody,
                    RequestTimestamp = startTime,
                    Environment = Environment
                };
                OnBeforeRequest(requestEventArgs);

                response = await _httpClient.SendAsync(request, cancellationToken);
                responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                var endTime = DateTimeOffset.UtcNow;
                var isSuccess = IsSuccessStatusCode(response.StatusCode);

                // Fire AfterResponse event
                var responseEventArgs = new ZatcaApiResponseEventArgs
                {
                    OperationType = operationType,
                    HttpMethod = HttpMethod.Post.ToString(),
                    RequestUrl = BuildFullUrl(ZatcaApiEndpoints.ComplianceCertificate),
                    RequestHeaders = requestEventArgs.RequestHeaders,
                    RequestBody = requestBody,
                    RequestTimestamp = startTime,
                    StatusCode = (int)response.StatusCode,
                    ResponseHeaders = ExtractHeaders(response.Headers, response.Content?.Headers),
                    ResponseBody = responseBody,
                    ResponseTimestamp = endTime,
                    Duration = endTime - startTime,
                    IsSuccess = isSuccess,
                    Environment = Environment
                };
                OnAfterResponse(responseEventArgs);

                if (!isSuccess)
                {
                    throw new ZatcaApiException(
                        $"API request failed with status code {(int)response.StatusCode}",
                        (int)response.StatusCode,
                        responseBody);
                }

                var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, _jsonOptions)
                    ?? throw new ZatcaApiException("Failed to parse API response", 0, responseBody);
                return ParseComplianceCertificateResult(responseDict);
            }
            catch (Exception ex)
            {
                error = ex;
                var endTime = DateTimeOffset.UtcNow;

                // Fire AfterResponse event with error
                var responseEventArgs = new ZatcaApiResponseEventArgs
                {
                    OperationType = operationType,
                    HttpMethod = HttpMethod.Post.ToString(),
                    RequestUrl = BuildFullUrl(ZatcaApiEndpoints.ComplianceCertificate),
                    RequestHeaders = new Dictionary<string, string> 
                    {
                        { "OTP", otp },
                        { AcceptVersionHeader, AcceptVersionValue }
                    },
                    RequestBody = requestBody,
                    RequestTimestamp = startTime,
                    StatusCode = response != null ? (int)response.StatusCode : null,
                    ResponseHeaders = response != null ? ExtractHeaders(response.Headers, response.Content?.Headers) : new Dictionary<string, string>(),
                    ResponseBody = responseBody,
                    ResponseTimestamp = endTime,
                    Duration = endTime - startTime,
                    IsSuccess = false,
                    Error = error,
                    Environment = Environment
                };
                OnAfterResponse(responseEventArgs);

                // Re-wrap exceptions if needed
                if (ex is ZatcaApiException)
                {
                    throw;
                }
                else if (ex is HttpRequestException httpEx)
                {
                    throw new ZatcaApiException("HTTP request failed", new Dictionary<string, object>
                    {
                        { "endpoint", ZatcaApiEndpoints.ComplianceCertificate },
                        { MessageKey, httpEx.Message }
                    }, 0, httpEx);
                }
                else if (ex is JsonException jsonEx)
                {
                    throw new ZatcaApiException("Failed to parse API response", new Dictionary<string, object>
                    {
                        { "endpoint", ZatcaApiEndpoints.ComplianceCertificate },
                        { MessageKey, jsonEx.Message }
                    }, 0, jsonEx);
                }

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<InvoiceSubmissionResult> ValidateInvoiceComplianceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            string certificate,
            string secret,
            CancellationToken cancellationToken = default)
        {
            ValidateInvoiceParameters(signedXml, invoiceHash, uuid, certificate, secret);

            var payload = new
            {
                invoiceHash = invoiceHash,
                uuid = uuid,
                invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml))
            };

            var headers = CreateAuthHeaders(certificate, secret);
            headers[AcceptVersionHeader] = AcceptVersionValue;
            headers["Accept-Language"] = "en";
            headers[ContentTypeHeader] = ApplicationJson;

            var response = await SendRequestAsync<Dictionary<string, object>>(
                HttpMethod.Post,
                ZatcaApiEndpoints.ComplianceInvoices,
                payload,
                headers,
                cancellationToken);

            return ParseInvoiceSubmissionResult(response);
        }

        /// <inheritdoc/>
        public async Task<ProductionCertificateResult> RequestProductionCertificateAsync(
            string complianceRequestId,
            string certificate,
            string secret,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(complianceRequestId))
                throw new ArgumentNullException(nameof(complianceRequestId));
            if (string.IsNullOrWhiteSpace(certificate))
                throw new ArgumentNullException(nameof(certificate));
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentNullException(nameof(secret));

            var payload = new
            {
                compliance_request_id = complianceRequestId
            };

            var headers = CreateAuthHeaders(certificate, secret);
            headers[AcceptVersionHeader] = AcceptVersionValue;
            headers[ContentTypeHeader] = ApplicationJson;

            var response = await SendRequestAsync<Dictionary<string, object>>(
                HttpMethod.Post,
                ZatcaApiEndpoints.ProductionCertificate,
                payload,
                headers,
                cancellationToken);

            return ParseProductionCertificateResult(response);
        }

        /// <inheritdoc/>
        public async Task<InvoiceSubmissionResult> SubmitClearanceInvoiceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            string certificate,
            string secret,
            CancellationToken cancellationToken = default)
        {
            ValidateInvoiceParameters(signedXml, invoiceHash, uuid, certificate, secret);

            var payload = new
            {
                invoiceHash = invoiceHash,
                uuid = uuid,
                invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml))
            };

            var headers = CreateAuthHeaders(certificate, secret);
            headers[AcceptVersionHeader] = AcceptVersionValue;
            headers["Clearance-Status"] = "1";
            headers["Accept-Language"] = "en";

            var response = await SendRequestAsync<Dictionary<string, object>>(
                HttpMethod.Post,
                ZatcaApiEndpoints.ClearanceInvoice,
                payload,
                headers,
                cancellationToken);

            return ParseInvoiceSubmissionResult(response);
        }

        /// <inheritdoc/>
        public async Task<InvoiceSubmissionResult> SubmitReportingInvoiceAsync(
            string signedXml,
            string invoiceHash,
            string uuid,
            string certificate,
            string secret,
            CancellationToken cancellationToken = default)
        {
            ValidateInvoiceParameters(signedXml, invoiceHash, uuid, certificate, secret);

            var payload = new
            {
                invoiceHash = invoiceHash,
                uuid = uuid,
                invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml))
            };

            var headers = CreateAuthHeaders(certificate, secret);
            headers[AcceptVersionHeader] = AcceptVersionValue;
            headers["Accept-Language"] = "en";

            var response = await SendRequestAsync<Dictionary<string, object>>(
                HttpMethod.Post,
                ZatcaApiEndpoints.ReportingInvoice,
                payload,
                headers,
                cancellationToken);

            return ParseInvoiceSubmissionResult(response);
        }

        /// <inheritdoc/>
        public async Task<ProductionCertificateResult> RenewProductionCertificateAsync(
            string otp,
            string csr,
            string certificate,
            string secret,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentNullException(nameof(otp));
            if (string.IsNullOrWhiteSpace(csr))
                throw new ArgumentNullException(nameof(csr));
            if (string.IsNullOrWhiteSpace(certificate))
                throw new ArgumentNullException(nameof(certificate));
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentNullException(nameof(secret));

            // Remove BOM if present and encode the entire PEM file to base64
            csr = csr.TrimStart('\uFEFF');
            var csrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(csr));

            var payload = new
            {
                csr = csrBase64
            };

            var headers = CreateAuthHeaders(certificate, secret);
            headers["OTP"] = otp;
            headers[AcceptVersionHeader] = AcceptVersionValue;
            headers[ContentTypeHeader] = ApplicationJson;

            var response = await SendRequestAsync<Dictionary<string, object>>(
                new HttpMethod("PATCH"),
                ZatcaApiEndpoints.ProductionCertificateRenewal,
                payload,
                headers,
                cancellationToken);

            return ParseProductionCertificateResult(response);
        }

        private async Task<T> SendRequestAsync<T>(
            HttpMethod method,
            string endpoint,
            object payload,
            Dictionary<string, string> headers,
            CancellationToken cancellationToken)
        {
            var operationType = DetermineOperationType(endpoint);
            var startTime = DateTimeOffset.UtcNow;
            HttpResponseMessage? response = null;
            string? requestBody = null;
            string? responseBody = null;
            Exception? error = null;

            try
            {
                var request = new HttpRequestMessage(method, endpoint);

                // Add custom headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (header.Key.Equals(ContentTypeHeader, StringComparison.OrdinalIgnoreCase))
                        {
                            request.Content = new StringContent(
                                JsonSerializer.Serialize(payload, _jsonOptions),
                                Encoding.UTF8,
                                header.Value);
                        }
                        else
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                }

                // Add content if not already set
                if (request.Content == null && payload != null)
                {
                    request.Content = new StringContent(
                        JsonSerializer.Serialize(payload, _jsonOptions),
                        Encoding.UTF8,
                        ApplicationJson);
                }

                // Capture request body for audit logging
                if (request.Content != null)
                {
                    requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                    // Recreate content since ReadAsStringAsync consumes it
                    if (headers != null && headers.TryGetValue(ContentTypeHeader, out var contentType))
                    {
                        request.Content = new StringContent(requestBody, Encoding.UTF8, contentType);
                    }
                    else
                    {
                        request.Content = new StringContent(requestBody, Encoding.UTF8, ApplicationJson);
                    }
                }

                // Fire BeforeRequest event
                var requestEventArgs = new ZatcaApiRequestEventArgs
                {
                    OperationType = operationType,
                    HttpMethod = method.ToString(),
                    RequestUrl = BuildFullUrl(endpoint),
                    RequestHeaders = ExtractHeaders(request.Headers, request.Content?.Headers),
                    RequestBody = requestBody,
                    RequestTimestamp = startTime,
                    Environment = Environment
                };
                OnBeforeRequest(requestEventArgs);

                // Send the request
                response = await _httpClient.SendAsync(request, cancellationToken);
                responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                var endTime = DateTimeOffset.UtcNow;
                var isSuccess = IsSuccessStatusCode(response.StatusCode);

                // Fire AfterResponse event
                var responseEventArgs = new ZatcaApiResponseEventArgs
                {
                    OperationType = operationType,
                    HttpMethod = method.ToString(),
                    RequestUrl = BuildFullUrl(endpoint),
                    RequestHeaders = requestEventArgs.RequestHeaders,
                    RequestBody = requestBody,
                    RequestTimestamp = startTime,
                    StatusCode = (int)response.StatusCode,
                    ResponseHeaders = ExtractHeaders(response.Headers, response.Content?.Headers),
                    ResponseBody = responseBody,
                    ResponseTimestamp = endTime,
                    Duration = endTime - startTime,
                    IsSuccess = isSuccess,
                    Environment = Environment
                };
                OnAfterResponse(responseEventArgs);

                if (!isSuccess)
                {
                    throw new ZatcaApiException(
                        $"API request failed with status code {(int)response.StatusCode}",
                        (int)response.StatusCode,
                        responseBody);
                }

                return JsonSerializer.Deserialize<T>(responseBody, _jsonOptions)
                    ?? throw new ZatcaApiException("Failed to deserialize API response", 0, responseBody);
            }
            catch (Exception ex)
            {
                error = ex;
                var endTime = DateTimeOffset.UtcNow;

                // Fire AfterResponse event with error
                var responseEventArgs = new ZatcaApiResponseEventArgs
                {
                    OperationType = operationType,
                    HttpMethod = method.ToString(),
                    RequestUrl = BuildFullUrl(endpoint),
                    RequestHeaders = headers != null ? new Dictionary<string, string>(headers) : new Dictionary<string, string>(),
                    RequestBody = requestBody,
                    RequestTimestamp = startTime,
                    StatusCode = response != null ? (int)response.StatusCode : null,
                    ResponseHeaders = response != null ? ExtractHeaders(response.Headers, response.Content?.Headers) : new Dictionary<string, string>(),
                    ResponseBody = responseBody,
                    ResponseTimestamp = endTime,
                    Duration = endTime - startTime,
                    IsSuccess = false,
                    Error = error,
                    Environment = Environment
                };
                OnAfterResponse(responseEventArgs);

                // Re-wrap exceptions if needed
                if (ex is ZatcaApiException)
                {
                    throw;
                }
                else if (ex is HttpRequestException httpEx)
                {
                    throw new ZatcaApiException("HTTP request failed", new Dictionary<string, object>
                    {
                        { "endpoint", endpoint },
                        { MessageKey, httpEx.Message }
                    }, 0, httpEx);
                }
                else if (ex is JsonException jsonEx)
                {
                    throw new ZatcaApiException("Failed to parse API response", new Dictionary<string, object>
                    {
                        { "endpoint", endpoint },
                        { MessageKey, jsonEx.Message }
                    }, 0, jsonEx);
                }

                throw;
            }
        }

        private bool IsSuccessStatusCode(System.Net.HttpStatusCode statusCode)
        {
            var code = (int)statusCode;

            if (code == 200)
                return true;

            if (code == 202 && _allowWarnings)
                return true;

            return false;
        }

        private static Dictionary<string, string> CreateAuthHeaders(string certificate, string secret)
        {
            // ZATCA Basic Auth format (matching php-zatca-xml implementation):
            // 1. Get raw certificate content (base64 string from PEM, without headers)
            // 2. Base64 encode it again: base64(rawCert)
            // 3. Concatenate with secret: base64(rawCert):secret
            // 4. Base64 encode the whole thing: base64(base64(rawCert):secret)
            // Authorization = Basic base64(base64(rawCert):secret)

            var certContent = certificate.Trim();
            string rawCertificate;

            if (certContent.Contains("-----BEGIN CERTIFICATE-----"))
            {
                // Certificate is in PEM format - extract just the base64 content (without headers)
                rawCertificate = certContent
                    .Replace("-----BEGIN CERTIFICATE-----", "")
                    .Replace("-----END CERTIFICATE-----", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Trim();
            }
            else
            {
                // Already in base64 format (raw certificate directly)
                rawCertificate = certContent;
            }

            // Double base64 encoding as per php-zatca-xml:
            // base64(base64(rawCert):secret)
            var encodedCert = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCertificate));
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{encodedCert}:{secret}"));

            return new Dictionary<string, string>
            {
                { "Authorization", $"Basic {credentials}" }
            };
        }

        private static void ValidateInvoiceParameters(string signedXml, string invoiceHash, string uuid, string certificate, string secret)
        {
            if (string.IsNullOrWhiteSpace(signedXml))
                throw new ArgumentNullException(nameof(signedXml));
            if (string.IsNullOrWhiteSpace(invoiceHash))
                throw new ArgumentNullException(nameof(invoiceHash));
            if (string.IsNullOrWhiteSpace(uuid))
                throw new ArgumentNullException(nameof(uuid));
            if (string.IsNullOrWhiteSpace(certificate))
                throw new ArgumentNullException(nameof(certificate));
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentNullException(nameof(secret));
        }

        private static ComplianceCertificateResult ParseComplianceCertificateResult(Dictionary<string, object> response)
        {
            var binarySecurityToken = GetStringValue(response, "binarySecurityToken");
            var secret = GetStringValue(response, "secret");
            var requestId = GetStringValue(response, "requestID");
            var dispositionMessage = GetStringValue(response, "dispositionMessage");

            var errors = ParseValidationMessages(response, ValidationResultsKey, "ERROR");
            var warnings = ParseValidationMessages(response, ValidationResultsKey, "WARNING");

            // Decode the base64 certificate
            var certificate = !string.IsNullOrEmpty(binarySecurityToken)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(binarySecurityToken))
                : string.Empty;

            return new ComplianceCertificateResult(
                certificate,
                secret ?? string.Empty,
                requestId ?? string.Empty,
                dispositionMessage,
                errors,
                warnings);
        }

        private static ProductionCertificateResult ParseProductionCertificateResult(Dictionary<string, object> response)
        {
            var binarySecurityToken = GetStringValue(response, "binarySecurityToken");
            var secret = GetStringValue(response, "secret");
            var requestId = GetStringValue(response, "requestID");
            var dispositionMessage = GetStringValue(response, "dispositionMessage");

            var errors = ParseValidationMessages(response, ValidationResultsKey, "ERROR");
            var warnings = ParseValidationMessages(response, ValidationResultsKey, "WARNING");

            // Decode the base64 certificate
            var certificate = !string.IsNullOrEmpty(binarySecurityToken)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(binarySecurityToken))
                : string.Empty;

            return new ProductionCertificateResult(
                certificate,
                secret ?? string.Empty,
                requestId ?? string.Empty,
                dispositionMessage,
                errors,
                warnings);
        }

        private static InvoiceSubmissionResult ParseInvoiceSubmissionResult(Dictionary<string, object> response)
        {
            var status = GetStringValue(response, "status");
            var clearanceStatus = GetStringValue(response, "clearanceStatus");
            var reportingStatus = GetStringValue(response, "reportingStatus");
            var clearedInvoice = GetStringValue(response, "clearedInvoice");

            var errors = ParseValidationMessagesDetailed(response, ValidationResultsKey, "ERROR");
            var warnings = ParseValidationMessagesDetailed(response, ValidationResultsKey, "WARNING");
            var infoMessages = ParseValidationMessagesDetailed(response, ValidationResultsKey, "INFO");

            // Decode cleared invoice if present
            if (!string.IsNullOrEmpty(clearedInvoice))
            {
                try
                {
                    clearedInvoice = Encoding.UTF8.GetString(Convert.FromBase64String(clearedInvoice));
                }
                catch
                {
                    // If decoding fails, keep the original value
                }
            }

            return new InvoiceSubmissionResult(
                status ?? string.Empty,
                clearanceStatus,
                reportingStatus,
                clearedInvoice,
                errors,
                warnings,
                infoMessages);
        }

        private static List<string> ParseValidationMessages(Dictionary<string, object> response, string key, string type)
        {
            var messagesElement = GetValidationMessagesElement(response, key, type);
            if (messagesElement == null || messagesElement.Value.ValueKind != JsonValueKind.Array)
                return new List<string>();

            return ExtractMessageStrings(messagesElement.Value);
        }

        private static JsonElement? GetValidationMessagesElement(Dictionary<string, object> response, string key, string type)
        {
            if (!response.TryGetValue(key, out var validationResults))
                return null;

            if (validationResults is not JsonElement jsonElement || jsonElement.ValueKind != JsonValueKind.Object)
                return null;

            var propertyName = type.ToLowerInvariant() + "Messages";
            if (!jsonElement.TryGetProperty(propertyName, out var messagesElement))
                return null;

            return messagesElement;
        }

        private static List<string> ExtractMessageStrings(JsonElement messagesElement)
        {
            var messages = new List<string>();
            foreach (var message in messagesElement.EnumerateArray())
            {
                if (message.TryGetProperty(MessageKey, out var msgText))
                {
                    var text = msgText.GetString();
                    if (text != null)
                        messages.Add(text);
                }
            }
            return messages;
        }

        private static List<ValidationMessage> ParseValidationMessagesDetailed(Dictionary<string, object> response, string key, string type)
        {
            var messagesElement = GetValidationMessagesElement(response, key, type);
            if (messagesElement == null || messagesElement.Value.ValueKind != JsonValueKind.Array)
                return new List<ValidationMessage>();

            return ExtractValidationMessages(messagesElement.Value, type);
        }

        private static List<ValidationMessage> ExtractValidationMessages(JsonElement messagesElement, string type)
        {
            var messages = new List<ValidationMessage>();
            foreach (var message in messagesElement.EnumerateArray())
            {
                // Skip non-object elements
                if (message.ValueKind != JsonValueKind.Object)
                    continue;

                messages.Add(new ValidationMessage
                {
                    Type = GetJsonStringValue(message, "type") ?? type,
                    Code = GetJsonStringValue(message, "code") ?? string.Empty,
                    Category = GetJsonStringValue(message, "category") ?? string.Empty,
                    Message = GetJsonStringValue(message, MessageKey) ?? string.Empty,
                    Status = GetJsonStringValue(message, "status") ?? string.Empty
                });
            }
            return messages;
        }

        private static string? GetStringValue(Dictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is JsonElement jsonElement)
                {
                    // Handle different JSON value types
                    return jsonElement.ValueKind switch
                    {
                        JsonValueKind.String => jsonElement.GetString(),
                        JsonValueKind.Number => jsonElement.ToString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Null => null,
                        _ => jsonElement.ToString()
                    };
                }
                return value?.ToString();
            }
            return null;
        }

        private static string? GetJsonStringValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
            }
            return null;
        }

        /// <summary>
        /// Raises the BeforeRequest event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnBeforeRequest(ZatcaApiRequestEventArgs e)
        {
            try
            {
                BeforeRequest?.Invoke(this, e);
            }
            catch
            {
                // Suppress event handler exceptions to prevent breaking API operations
            }
        }

        /// <summary>
        /// Raises the AfterResponse event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnAfterResponse(ZatcaApiResponseEventArgs e)
        {
            try
            {
                AfterResponse?.Invoke(this, e);
            }
            catch
            {
                // Suppress event handler exceptions to prevent breaking API operations
            }
        }

        /// <summary>
        /// Determines the operation type from the endpoint.
        /// </summary>
        private static string DetermineOperationType(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
                return "Unknown";

            if (endpoint.Contains("compliance"))
                return "OnboardCompliance";
            if (endpoint.Contains("production/csids"))
                return "UpgradeProduction";
            if (endpoint.Contains("compliance/invoices"))
                return "ValidateCompliance";
            if (endpoint.Contains("invoices/clearance"))
                return "SubmitClearance";
            if (endpoint.Contains("invoices/reporting"))
                return "SubmitReporting";
            
            return "Unknown";
        }

        /// <summary>
        /// Builds the full URL for the endpoint.
        /// </summary>
        private string BuildFullUrl(string endpoint)
        {
            if (Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
                return endpoint;

            var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? ZatcaApiEndpoints.GetBaseUrl(Environment);
            return $"{baseUrl}/{endpoint.TrimStart('/')}";
        }

        /// <summary>
        /// Extracts headers from HttpHeaders collections.
        /// </summary>
        private static Dictionary<string, string> ExtractHeaders(HttpHeaders? requestHeaders, HttpContentHeaders? contentHeaders)
        {
            var headers = new Dictionary<string, string>();

            if (requestHeaders != null)
            {
                foreach (var header in requestHeaders)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }
            }

            if (contentHeaders != null)
            {
                foreach (var header in contentHeaders)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }
            }

            return headers;
        }

        /// <summary>
        /// Disposes the HTTP client if it was created internally.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && _disposeHttpClient)
            {
                _httpClient?.Dispose();
            }

            _disposed = true;
        }
    }
}
