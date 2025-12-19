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
        private readonly HttpClient _httpClient;
        private readonly bool _disposeHttpClient;
        private bool _allowWarnings;
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
        /// Initializes a new instance of the <see cref="ZatcaApiClient"/> class.
        /// </summary>
        /// <param name="environment">The ZATCA environment.</param>
        /// <param name="httpClient">Optional HttpClient instance. If not provided, a new one will be created.</param>
        public ZatcaApiClient(ZatcaEnvironment environment, HttpClient httpClient = null)
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
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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

            var request = new HttpRequestMessage(HttpMethod.Post, ZatcaApiEndpoints.ComplianceCertificate);
            request.Headers.TryAddWithoutValidation("OTP", otp);
            request.Headers.TryAddWithoutValidation("Accept-Version", "V2");

            var json = JsonSerializer.Serialize(new { csr = csrBase64 }, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync();

            if (!IsSuccessStatusCode(response.StatusCode))
            {
                throw new ZatcaApiException(
                    $"API request failed with status code {(int)response.StatusCode}",
                    (int)response.StatusCode,
                    content);
            }

            var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);
            return ParseComplianceCertificateResult(responseDict);
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
            headers["Accept-Version"] = "V2";
            headers["Accept-Language"] = "en";
            headers["Content-Type"] = "application/json";

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
            headers["Accept-Version"] = "V2";
            headers["Content-Type"] = "application/json";

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
            headers["Accept-Version"] = "V2";
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
            headers["Accept-Version"] = "V2";
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
            headers["Accept-Version"] = "V2";
            headers["Content-Type"] = "application/json";

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
            try
            {
                var request = new HttpRequestMessage(method, endpoint);

                // Add custom headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
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
                        "application/json");
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var content = await response.Content.ReadAsStringAsync();

                if (!IsSuccessStatusCode(response.StatusCode))
                {
                    throw new ZatcaApiException(
                        $"API request failed with status code {(int)response.StatusCode}",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                throw new ZatcaApiException("HTTP request failed", new Dictionary<string, object>
                {
                    { "endpoint", endpoint },
                    { "message", ex.Message }
                }, 0, ex);
            }
            catch (JsonException ex)
            {
                throw new ZatcaApiException("Failed to parse API response", new Dictionary<string, object>
                {
                    { "endpoint", endpoint },
                    { "message", ex.Message }
                }, 0, ex);
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

        private Dictionary<string, string> CreateAuthHeaders(string certificate, string secret)
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

        private void ValidateInvoiceParameters(string signedXml, string invoiceHash, string uuid, string certificate, string secret)
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

        private ComplianceCertificateResult ParseComplianceCertificateResult(Dictionary<string, object> response)
        {
            var binarySecurityToken = GetStringValue(response, "binarySecurityToken");
            var secret = GetStringValue(response, "secret");
            var requestId = GetStringValue(response, "requestID");
            var dispositionMessage = GetStringValue(response, "dispositionMessage");

            var errors = ParseValidationMessages(response, "validationResults", "ERROR");
            var warnings = ParseValidationMessages(response, "validationResults", "WARNING");

            // Decode the base64 certificate
            var certificate = !string.IsNullOrEmpty(binarySecurityToken)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(binarySecurityToken))
                : string.Empty;

            return new ComplianceCertificateResult(
                certificate,
                secret,
                requestId,
                dispositionMessage,
                errors,
                warnings);
        }

        private ProductionCertificateResult ParseProductionCertificateResult(Dictionary<string, object> response)
        {
            var binarySecurityToken = GetStringValue(response, "binarySecurityToken");
            var secret = GetStringValue(response, "secret");
            var requestId = GetStringValue(response, "requestID");
            var dispositionMessage = GetStringValue(response, "dispositionMessage");

            var errors = ParseValidationMessages(response, "validationResults", "ERROR");
            var warnings = ParseValidationMessages(response, "validationResults", "WARNING");

            // Decode the base64 certificate
            var certificate = !string.IsNullOrEmpty(binarySecurityToken)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(binarySecurityToken))
                : string.Empty;

            return new ProductionCertificateResult(
                certificate,
                secret,
                requestId,
                dispositionMessage,
                errors,
                warnings);
        }

        private InvoiceSubmissionResult ParseInvoiceSubmissionResult(Dictionary<string, object> response)
        {
            var status = GetStringValue(response, "status");
            var clearanceStatus = GetStringValue(response, "clearanceStatus");
            var reportingStatus = GetStringValue(response, "reportingStatus");
            var clearedInvoice = GetStringValue(response, "clearedInvoice");

            var errors = ParseValidationMessagesDetailed(response, "validationResults", "ERROR");
            var warnings = ParseValidationMessagesDetailed(response, "validationResults", "WARNING");
            var infoMessages = ParseValidationMessagesDetailed(response, "validationResults", "INFO");

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
                status,
                clearanceStatus,
                reportingStatus,
                clearedInvoice,
                errors,
                warnings,
                infoMessages);
        }

        private List<string> ParseValidationMessages(Dictionary<string, object> response, string key, string type)
        {
            var messages = new List<string>();

            if (response.TryGetValue(key, out var validationResults))
            {
                if (validationResults is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                {
                    if (jsonElement.TryGetProperty(type.ToLowerInvariant() + "Messages", out var messagesElement))
                    {
                        if (messagesElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var message in messagesElement.EnumerateArray())
                            {
                                if (message.TryGetProperty("message", out var msgText))
                                {
                                    messages.Add(msgText.GetString());
                                }
                            }
                        }
                    }
                }
            }

            return messages;
        }

        private List<ValidationMessage> ParseValidationMessagesDetailed(Dictionary<string, object> response, string key, string type)
        {
            var messages = new List<ValidationMessage>();

            if (response.TryGetValue(key, out var validationResults))
            {
                if (validationResults is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                {
                    var propertyName = type.ToLowerInvariant() + "Messages";
                    if (jsonElement.TryGetProperty(propertyName, out var messagesElement))
                    {
                        if (messagesElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var message in messagesElement.EnumerateArray())
                            {
                                var validationMessage = new ValidationMessage
                                {
                                    Type = GetJsonStringValue(message, "type") ?? type,
                                    Code = GetJsonStringValue(message, "code"),
                                    Category = GetJsonStringValue(message, "category"),
                                    Message = GetJsonStringValue(message, "message"),
                                    Status = GetJsonStringValue(message, "status")
                                };
                                messages.Add(validationMessage);
                            }
                        }
                    }
                }
            }

            return messages;
        }

        private string GetStringValue(Dictionary<string, object> dict, string key)
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

        private string GetJsonStringValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
            }
            return null;
        }

        private string ExtractBase64FromPem(string pem)
        {
            const string beginMarker = "-----BEGIN CERTIFICATE REQUEST-----";
            const string endMarker = "-----END CERTIFICATE REQUEST-----";

            var beginIndex = pem.IndexOf(beginMarker);
            if (beginIndex == -1)
                throw new ArgumentException("Invalid PEM format: missing begin marker");

            beginIndex += beginMarker.Length;
            var endIndex = pem.IndexOf(endMarker, beginIndex);
            if (endIndex == -1)
                throw new ArgumentException("Invalid PEM format: missing end marker");

            var base64Content = pem[beginIndex..endIndex].Replace("\n", "").Replace("\r", "").Trim();
            return base64Content;
        }

        /// <summary>
        /// Disposes the HTTP client if it was created internally.
        /// </summary>
        public void Dispose()
        {
            if (_disposeHttpClient)
            {
                _httpClient?.Dispose();
            }
        }
    }
}
