using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Zatca.EInvoice.Api;
using Zatca.EInvoice.Exceptions;

namespace Zatca.EInvoice.Tests.Api
{
    /// <summary>
    /// Tests for ZatcaApiClient class with mocked HttpClient.
    /// </summary>
    public class ZatcaApiClientTests : IDisposable
    {
        private readonly List<ZatcaApiClient> _clients = new List<ZatcaApiClient>();

        /// <summary>
        /// Test that invalid environment enum value works (all enum values are valid).
        /// </summary>
        [Fact]
        public void TestValidEnvironmentDoesNotThrow()
        {
            // Act & Assert - All enum values should be valid
            var client1 = new ZatcaApiClient(ZatcaEnvironment.Sandbox);
            var client2 = new ZatcaApiClient(ZatcaEnvironment.Simulation);
            var client3 = new ZatcaApiClient(ZatcaEnvironment.Production);

            _clients.Add(client1);
            _clients.Add(client2);
            _clients.Add(client3);

            Assert.Equal(ZatcaEnvironment.Sandbox, client1.Environment);
            Assert.Equal(ZatcaEnvironment.Simulation, client2.Environment);
            Assert.Equal(ZatcaEnvironment.Production, client3.Environment);
        }

        /// <summary>
        /// Test that RequestComplianceCertificate returns successful response.
        /// </summary>
        [Fact]
        public async Task TestRequestComplianceCertificateSuccess()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new
            {
                binarySecurityToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("TEST_CERTIFICATE")),
                secret = "test_secret_123",
                requestID = "req_12345",
                dispositionMessage = "Certificate issued successfully",
                validationResults = new
                {
                    errorMessages = Array.Empty<object>(),
                    warningMessages = Array.Empty<object>()
                }
            });

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act
            var result = await client.RequestComplianceCertificateAsync(
                "test_csr_content",
                "123456");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST_CERTIFICATE", result.BinarySecurityToken);
            Assert.Equal("test_secret_123", result.Secret);
            Assert.Equal("req_12345", result.RequestId);
            Assert.Equal("Certificate issued successfully", result.DispositionMessage);
            Assert.Empty(result.Errors);
            Assert.Empty(result.Warnings);
        }

        /// <summary>
        /// Test that ValidateInvoiceCompliance returns successful response.
        /// </summary>
        [Fact]
        public async Task TestValidateInvoiceComplianceSuccess()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new
            {
                status = "VALID",
                clearanceStatus = "",
                reportingStatus = "REPORTED",
                clearedInvoice = "",
                validationResults = new
                {
                    errorMessages = Array.Empty<object>(),
                    warningMessages = Array.Empty<object>(),
                    infoMessages = Array.Empty<object>()
                }
            });

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act
            var result = await client.ValidateInvoiceComplianceAsync(
                "<Invoice>test</Invoice>",
                "test_hash",
                "12345678-1234-1234-1234-123456789012",
                "test_certificate",
                "test_secret");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("VALID", result.Status);
            Assert.Equal("REPORTED", result.ReportingStatus);
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
            Assert.Empty(result.Warnings);
        }

        /// <summary>
        /// Test that RequestProductionCertificate returns successful response.
        /// </summary>
        [Fact]
        public async Task TestRequestProductionCertificateSuccess()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new
            {
                binarySecurityToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("PROD_CERTIFICATE")),
                secret = "prod_secret_456",
                requestID = "prod_req_67890",
                dispositionMessage = "Production certificate issued",
                validationResults = new
                {
                    errorMessages = Array.Empty<object>(),
                    warningMessages = Array.Empty<object>()
                }
            });

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Simulation, httpClient);
            _clients.Add(client);

            // Act
            var result = await client.RequestProductionCertificateAsync(
                "compliance_req_12345",
                "test_certificate",
                "test_secret");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PROD_CERTIFICATE", result.BinarySecurityToken);
            Assert.Equal("prod_secret_456", result.Secret);
            Assert.Equal("prod_req_67890", result.RequestId);
            Assert.Equal("Production certificate issued", result.DispositionMessage);
            Assert.Empty(result.Errors);
            Assert.Empty(result.Warnings);
        }

        /// <summary>
        /// Test that SubmitClearanceInvoice returns successful response.
        /// </summary>
        [Fact]
        public async Task TestSubmitClearanceInvoiceSuccess()
        {
            // Arrange
            var clearedInvoiceContent = "<Invoice>cleared</Invoice>";
            var responseContent = JsonSerializer.Serialize(new
            {
                status = "CLEARED",
                clearanceStatus = "CLEARED",
                reportingStatus = "",
                clearedInvoice = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(clearedInvoiceContent)),
                validationResults = new
                {
                    errorMessages = Array.Empty<object>(),
                    warningMessages = Array.Empty<object>(),
                    infoMessages = Array.Empty<object>()
                }
            });

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Production, httpClient);
            _clients.Add(client);

            // Act
            var result = await client.SubmitClearanceInvoiceAsync(
                "<Invoice>test</Invoice>",
                "test_hash",
                "12345678-1234-1234-1234-123456789012",
                "test_certificate",
                "test_secret");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CLEARED", result.Status);
            Assert.Equal("CLEARED", result.ClearanceStatus);
            Assert.NotNull(result.ClearedInvoice);
            Assert.Contains("cleared", result.ClearedInvoice);
            Assert.True(result.IsClearance);
            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// Test that API request failure throws ZatcaApiException.
        /// </summary>
        [Fact]
        public async Task TestSendRequestFailure()
        {
            // Arrange
            var errorResponse = JsonSerializer.Serialize(new
            {
                error = "Invalid request",
                message = "The provided data is invalid"
            });

            var httpClient = CreateMockHttpClient(HttpStatusCode.BadRequest, errorResponse);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act & Assert
            await Assert.ThrowsAsync<ZatcaApiException>(async () =>
                await client.RequestComplianceCertificateAsync("invalid_csr", "invalid_otp"));
        }

        /// <summary>
        /// Test that 500 Internal Server Error throws ZatcaApiException.
        /// </summary>
        [Fact]
        public async Task TestInternalServerErrorThrowsException()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(
                HttpStatusCode.InternalServerError,
                "Internal Server Error");
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ZatcaApiException>(async () =>
                await client.RequestComplianceCertificateAsync("test_csr", "123456"));

            Assert.Contains("500", exception.Message);
        }

        /// <summary>
        /// Test that null parameters throw ArgumentNullException.
        /// </summary>
        [Fact]
        public async Task TestNullParametersThrowException()
        {
            // Arrange
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox);
            _clients.Add(client);

            // Act & Assert - RequestComplianceCertificateAsync
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await client.RequestComplianceCertificateAsync(null!, "123456"));

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await client.RequestComplianceCertificateAsync("csr", null!));

            // Act & Assert - ValidateInvoiceComplianceAsync
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await client.ValidateInvoiceComplianceAsync(
                    null!, "hash", "uuid", "cert", "secret"));

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await client.ValidateInvoiceComplianceAsync(
                    "xml", null!, "uuid", "cert", "secret"));

            // Act & Assert - RequestProductionCertificateAsync
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await client.RequestProductionCertificateAsync(null!, "cert", "secret"));

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await client.RequestProductionCertificateAsync("req_id", null!, "secret"));

            // Act & Assert - SubmitClearanceInvoiceAsync
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await client.SubmitClearanceInvoiceAsync(
                    null!, "hash", "uuid", "cert", "secret"));
        }

        /// <summary>
        /// Test that SetWarningHandling affects response handling.
        /// </summary>
        [Fact]
        public async Task TestWarningHandling()
        {
            // Arrange - Response with warnings (status 202)
            var responseContent = JsonSerializer.Serialize(new
            {
                status = "VALID",
                reportingStatus = "REPORTED",
                validationResults = new
                {
                    errorMessages = Array.Empty<object>(),
                    warningMessages = new[]
                    {
                        new
                        {
                            type = "WARNING",
                            code = "W001",
                            category = "validation",
                            message = "Minor validation warning",
                            status = "WARNING"
                        }
                    },
                    infoMessages = Array.Empty<object>()
                }
            });

            // Create client that allows warnings
            var httpClient = CreateMockHttpClient(HttpStatusCode.Accepted, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            client.SetWarningHandling(true);
            _clients.Add(client);

            // Act
            var result = await client.ValidateInvoiceComplianceAsync(
                "<Invoice>test</Invoice>",
                "test_hash",
                "12345678-1234-1234-1234-123456789012",
                "test_certificate",
                "test_secret");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasWarnings);
        }

        /// <summary>
        /// Test that response with validation errors is parsed correctly.
        /// </summary>
        [Fact]
        public async Task TestResponseWithValidationErrors()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new
            {
                status = "INVALID",
                reportingStatus = "FAILED",
                validationResults = new
                {
                    errorMessages = new[]
                    {
                        new
                        {
                            type = "ERROR",
                            code = "E001",
                            category = "validation",
                            message = "Invalid invoice format",
                            status = "ERROR"
                        }
                    },
                    warningMessages = Array.Empty<object>(),
                    infoMessages = Array.Empty<object>()
                }
            });

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act
            var result = await client.ValidateInvoiceComplianceAsync(
                "<Invoice>test</Invoice>",
                "test_hash",
                "12345678-1234-1234-1234-123456789012",
                "test_certificate",
                "test_secret");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal("E001", result.Errors[0].Code);
            Assert.Equal("Invalid invoice format", result.Errors[0].Message);
        }

        /// <summary>
        /// Creates a mock HttpClient that returns predefined responses.
        /// </summary>
        private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string responseContent)
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent)
                });
            return new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/")
            };
        }

        /// <summary>
        /// Test SubmitReportingInvoice returns successful response.
        /// </summary>
        [Fact]
        public async Task TestSubmitReportingInvoiceSuccess()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new
            {
                status = "REPORTED",
                clearanceStatus = "",
                reportingStatus = "REPORTED",
                clearedInvoice = "",
                validationResults = new
                {
                    errorMessages = Array.Empty<object>(),
                    warningMessages = Array.Empty<object>(),
                    infoMessages = Array.Empty<object>()
                }
            });

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Production, httpClient);
            _clients.Add(client);

            // Act
            var result = await client.SubmitReportingInvoiceAsync(
                "<Invoice>test</Invoice>",
                "test_hash",
                "12345678-1234-1234-1234-123456789012",
                "test_certificate",
                "test_secret");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("REPORTED", result.Status);
            Assert.Equal("REPORTED", result.ReportingStatus);
            Assert.True(result.IsReporting);
            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// Test timeout handling with cancellation token.
        /// </summary>
        [Fact]
        public async Task TestCancellationTokenHandling()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(async (HttpRequestMessage request, CancellationToken ct) =>
                {
                    await Task.Delay(5000, ct); // Simulate long operation
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/")
            };
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(100); // Cancel after 100ms

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await client.RequestComplianceCertificateAsync("csr", "otp", cts.Token));
        }

        /// <summary>
        /// Test network error handling.
        /// </summary>
        [Fact]
        public async Task TestNetworkErrorHandling()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/")
            };
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act & Assert
            await Assert.ThrowsAsync<ZatcaApiException>(async () =>
                await client.RequestComplianceCertificateAsync("csr", "otp"));
        }

        /// <summary>
        /// Test malformed JSON response handling.
        /// </summary>
        [Fact]
        public async Task TestMalformedJsonResponseHandling()
        {
            // Arrange
            var malformedJson = "{invalid json content";
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, malformedJson);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act & Assert
            await Assert.ThrowsAsync<ZatcaApiException>(async () =>
                await client.RequestComplianceCertificateAsync("csr", "otp"));
        }

        /// <summary>
        /// Test response with multiple validation messages.
        /// </summary>
        [Fact]
        public async Task TestResponseWithMultipleValidationMessages()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new
            {
                status = "INVALID",
                reportingStatus = "FAILED",
                validationResults = new
                {
                    errorMessages = new[]
                    {
                        new
                        {
                            type = "ERROR",
                            code = "E001",
                            category = "validation",
                            message = "Error 1",
                            status = "ERROR"
                        },
                        new
                        {
                            type = "ERROR",
                            code = "E002",
                            category = "validation",
                            message = "Error 2",
                            status = "ERROR"
                        }
                    },
                    warningMessages = new[]
                    {
                        new
                        {
                            type = "WARNING",
                            code = "W001",
                            category = "validation",
                            message = "Warning 1",
                            status = "WARNING"
                        }
                    },
                    infoMessages = new[]
                    {
                        new
                        {
                            type = "INFO",
                            code = "I001",
                            category = "information",
                            message = "Info 1",
                            status = "INFO"
                        }
                    }
                }
            });

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act
            var result = await client.ValidateInvoiceComplianceAsync(
                "<Invoice>test</Invoice>",
                "test_hash",
                "12345678-1234-1234-1234-123456789012",
                "test_certificate",
                "test_secret");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(2, result.Errors.Count);
            Assert.Single(result.Warnings);
            Assert.Single(result.InfoMessages);
            Assert.True(result.HasWarnings);
        }

        /// <summary>
        /// Test authentication header formatting.
        /// </summary>
        [Fact]
        public async Task TestAuthenticationHeaderFormatting()
        {
            // Arrange
            HttpRequestMessage capturedRequest = null;
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken ct) =>
                {
                    capturedRequest = request;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(new
                        {
                            status = "VALID",
                            validationResults = new
                            {
                                errorMessages = Array.Empty<object>(),
                                warningMessages = Array.Empty<object>(),
                                infoMessages = Array.Empty<object>()
                            }
                        }))
                    };
                });

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/")
            };
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act
            await client.ValidateInvoiceComplianceAsync(
                "<Invoice>test</Invoice>",
                "test_hash",
                "12345678-1234-1234-1234-123456789012",
                "test_cert",
                "test_secret");

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest.Headers.Contains("Authorization"));
            var authHeader = capturedRequest.Headers.Authorization;
            Assert.Equal("Basic", authHeader?.Scheme);
        }

        /// <summary>
        /// Test different HTTP status codes.
        /// </summary>
        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task TestVariousHttpErrorStatusCodes(HttpStatusCode statusCode)
        {
            // Arrange
            var errorResponse = JsonSerializer.Serialize(new { error = "Error occurred" });
            var httpClient = CreateMockHttpClient(statusCode, errorResponse);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ZatcaApiException>(async () =>
                await client.RequestComplianceCertificateAsync("csr", "otp"));

            Assert.Contains(((int)statusCode).ToString(), exception.Message);
        }

        /// <summary>
        /// Test concurrent API requests.
        /// </summary>
        [Fact]
        public async Task TestConcurrentApiRequests()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new
            {
                binarySecurityToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("CERT")),
                secret = "secret",
                requestID = "req_id",
                dispositionMessage = "Success",
                validationResults = new
                {
                    errorMessages = Array.Empty<object>(),
                    warningMessages = Array.Empty<object>()
                }
            });

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            var tasks = new List<Task<ComplianceCertificateResult>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var task = client.RequestComplianceCertificateAsync($"csr_{i}", $"otp_{i}");
                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, results.Length);
            foreach (var result in results)
            {
                Assert.NotNull(result);
                Assert.Equal("CERT", result.BinarySecurityToken);
            }
        }

        /// <summary>
        /// Test empty response handling.
        /// </summary>
        [Fact]
        public async Task TestEmptyResponseHandling()
        {
            // Arrange
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, "{}");
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act
            var result = await client.RequestComplianceCertificateAsync("csr", "otp");

            // Assert
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(result.BinarySecurityToken));
            Assert.True(string.IsNullOrEmpty(result.Secret));
        }

        /// <summary>
        /// Test base64 encoding in requests.
        /// </summary>
        [Fact]
        public async Task TestBase64EncodingInRequests()
        {
            // Arrange
            string capturedContent = null;
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(async (HttpRequestMessage request, CancellationToken ct) =>
                {
                    capturedContent = await request.Content.ReadAsStringAsync();
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(new
                        {
                            binarySecurityToken = "dGVzdA==",
                            secret = "secret",
                            requestID = "req",
                            dispositionMessage = "OK",
                            validationResults = new { }
                        }))
                    };
                });

            var httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/")
            };
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            _clients.Add(client);

            // Act
            await client.RequestComplianceCertificateAsync("test_csr_content", "123456");

            // Assert
            Assert.NotNull(capturedContent);
            var requestObj = JsonSerializer.Deserialize<Dictionary<string, object>>(capturedContent);
            Assert.True(requestObj.ContainsKey("csr"));

            // Verify CSR is base64 encoded
            var csrElement = (JsonElement)requestObj["csr"];
            var csrValue = csrElement.GetString();
            Assert.NotNull(csrValue);

            // Should be valid base64
            var decodedCsr = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(csrValue));
            Assert.Equal("test_csr_content", decodedCsr);
        }

        /// <summary>
        /// Test client disposal doesn't affect external HttpClient.
        /// </summary>
        [Fact]
        public void TestClientDisposalWithExternalHttpClient()
        {
            // Arrange
            var externalHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/")
            };
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, externalHttpClient);

            // Act
            client.Dispose();

            // Assert - External client should still be usable (not disposed)
            Assert.NotNull(externalHttpClient.BaseAddress);
        }

        /// <summary>
        /// Test warning handling with 202 status without allowing warnings.
        /// </summary>
        [Fact]
        public async Task TestWarningRejectionWhenNotAllowed()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new { status = "VALID" });
            var httpClient = CreateMockHttpClient(HttpStatusCode.Accepted, responseContent);
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, httpClient);
            client.SetWarningHandling(false); // Don't allow warnings
            _clients.Add(client);

            // Act & Assert
            await Assert.ThrowsAsync<ZatcaApiException>(async () =>
                await client.ValidateInvoiceComplianceAsync(
                    "<Invoice>test</Invoice>",
                    "hash",
                    "uuid",
                    "cert",
                    "secret"));
        }

        /// <summary>
        /// Cleanup test clients.
        /// </summary>
        public void Dispose()
        {
            foreach (var client in _clients)
            {
                client.Dispose();
            }
            _clients.Clear();
        }
    }
}
