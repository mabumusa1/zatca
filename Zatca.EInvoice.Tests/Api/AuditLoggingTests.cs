using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Zatca.EInvoice.Api;
using Zatca.EInvoice.Api.Examples;

namespace Zatca.EInvoice.Tests.Api
{
    /// <summary>
    /// Tests for ZATCA API audit logging functionality.
    /// </summary>
    public class AuditLoggingTests : IDisposable
    {
        private readonly TestHttpMessageHandler _httpHandler;
        private readonly HttpClient _httpClient;
        private readonly TestAuditService _auditService;

        public AuditLoggingTests()
        {
            _httpHandler = new TestHttpMessageHandler();
            _httpClient = new HttpClient(_httpHandler)
            {
                BaseAddress = new Uri("https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal")
            };
            _auditService = new TestAuditService();
        }

        [Fact]
        public async Task EventHooks_BeforeRequest_ShouldFire()
        {
            // Arrange
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, _httpClient);
            ZatcaApiRequestEventArgs? capturedRequest = null;

            client.BeforeRequest += (sender, e) =>
            {
                capturedRequest = e;
            };

            _httpHandler.SetMockResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""binarySecurityToken"":""dGVzdA=="",""secret"":""secret123"",""requestID"":""req-001""}")
            });

            // Act
            try
            {
                await client.RequestComplianceCertificateAsync("csr-content", "123456");
            }
            catch
            {
                // Ignore API errors, we're testing events
            }

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("OnboardCompliance", capturedRequest.OperationType);
            Assert.Equal("POST", capturedRequest.HttpMethod);
            Assert.Equal(ZatcaEnvironment.Sandbox, capturedRequest.Environment);
            Assert.True(capturedRequest.RequestTimestamp != default);
        }

        [Fact]
        public async Task EventHooks_AfterResponse_ShouldFireWithSuccessDetails()
        {
            // Arrange
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, _httpClient);
            ZatcaApiResponseEventArgs? capturedResponse = null;

            client.AfterResponse += (sender, e) =>
            {
                capturedResponse = e;
            };

            _httpHandler.SetMockResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""binarySecurityToken"":""dGVzdA=="",""secret"":""secret123"",""requestID"":""req-001""}")
            });

            // Act
            try
            {
                await client.RequestComplianceCertificateAsync("csr-content", "123456");
            }
            catch
            {
                // Ignore API errors
            }

            // Assert
            Assert.NotNull(capturedResponse);
            Assert.Equal(200, capturedResponse.StatusCode);
            Assert.True(capturedResponse.IsSuccess);
            Assert.NotNull(capturedResponse.ResponseBody);
            Assert.True(capturedResponse.Duration != default);
            Assert.True(capturedResponse.Duration.TotalMilliseconds >= 0);
            Assert.Null(capturedResponse.Error);
        }

        [Fact]
        public async Task EventHooks_AfterResponse_ShouldFireWithFailureDetails()
        {
            // Arrange
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, _httpClient);
            ZatcaApiResponseEventArgs? capturedResponse = null;

            client.AfterResponse += (sender, e) =>
            {
                capturedResponse = e;
            };

            _httpHandler.SetMockResponse(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(@"{""error"":""Invalid OTP""}")
            });

            // Act
            try
            {
                await client.RequestComplianceCertificateAsync("csr-content", "wrong-otp");
            }
            catch
            {
                // Expected to throw
            }

            // Assert
            Assert.NotNull(capturedResponse);
            Assert.Equal(401, capturedResponse.StatusCode);
            Assert.False(capturedResponse.IsSuccess);
            Assert.NotNull(capturedResponse.Error);
        }

        [Fact]
        public async Task DelegatingHandler_ShouldCaptureRequestAndResponse()
        {
            // Arrange
            var auditHandler = new ZatcaAuditLoggingHandler(_auditService);
            auditHandler.InnerHandler = _httpHandler;

            var httpClient = new HttpClient(auditHandler)
            {
                BaseAddress = new Uri("https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal")
            };

            var options = new ZatcaApiClientOptions
            {
                Environment = ZatcaEnvironment.Sandbox,
                HttpClient = httpClient
            };

            var client = new ZatcaApiClient(options);

            _httpHandler.SetMockResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""binarySecurityToken"":""dGVzdA=="",""secret"":""secret123"",""requestID"":""req-001""}")
            });

            // Act
            try
            {
                await client.RequestComplianceCertificateAsync("csr-content", "123456");
            }
            catch
            {
                // Ignore API errors
            }

            // Assert
            Assert.Single(_auditService.Logs);
            var log = _auditService.Logs.First();
            Assert.Equal("OnboardCompliance", log.OperationType);
            Assert.Equal("POST", log.HttpMethod);
            Assert.NotNull(log.RequestPayload);
            Assert.Equal(200, log.HttpStatusCode);
            Assert.True(log.IsSuccess);
            Assert.NotNull(log.ResponsePayload);
        }

        [Fact]
        public async Task ZatcaApiClientOptions_WithHandlers_ShouldBuildPipelineCorrectly()
        {
            // Arrange
            var firstHandler = new TestDelegatingHandler("first");
            var secondHandler = new TestDelegatingHandler("second");

            var options = new ZatcaApiClientOptions
            {
                Environment = ZatcaEnvironment.Sandbox,
                Handlers = new List<System.Net.Http.DelegatingHandler>
                {
                    firstHandler,
                    secondHandler
                }
            };

            var client = new ZatcaApiClient(options);

            // Act & Assert
            // Just verify client creation doesn't throw
            Assert.NotNull(client);
            Assert.Equal(ZatcaEnvironment.Sandbox, client.Environment);
        }

        [Fact]
        public async Task EventBasedAuditLogger_ShouldLogAllOperations()
        {
            // Arrange
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, _httpClient);
            var eventLogger = new ZatcaEventBasedAuditLogger(_auditService);
            eventLogger.AttachToClient(client);

            _httpHandler.SetMockResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""binarySecurityToken"":""dGVzdA=="",""secret"":""secret123"",""requestID"":""req-001""}")
            });

            // Act
            try
            {
                await client.RequestComplianceCertificateAsync("csr-content", "123456");
            }
            catch
            {
                // Ignore API errors
            }

            // Give async logging time to complete
            await Task.Delay(100);

            // Assert
            Assert.NotEmpty(_auditService.Logs);
            var log = _auditService.Logs.First();
            Assert.Equal("OnboardCompliance", log.OperationType);
        }

        [Fact]
        public async Task EventBasedAuditLogger_Detach_ShouldStopLogging()
        {
            // Arrange
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, _httpClient);
            var eventLogger = new ZatcaEventBasedAuditLogger(_auditService);
            
            eventLogger.AttachToClient(client);
            eventLogger.DetachFromClient(client);

            _httpHandler.SetMockResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""binarySecurityToken"":""dGVzdA=="",""secret"":""secret123"",""requestID"":""req-001""}")
            });

            // Act
            try
            {
                await client.RequestComplianceCertificateAsync("csr-content", "123456");
            }
            catch
            {
                // Ignore API errors
            }

            // Assert
            Assert.Empty(_auditService.Logs);
        }

        [Fact]
        public async Task AuditLogging_EventHandlerException_ShouldNotBreakOperation()
        {
            // Arrange
            var client = new ZatcaApiClient(ZatcaEnvironment.Sandbox, _httpClient);

            // Attach event handler that throws
            client.AfterResponse += (sender, e) =>
            {
                throw new InvalidOperationException("Event handler failed!");
            };

            _httpHandler.SetMockResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{""binarySecurityToken"":""dGVzdA=="",""secret"":""secret123"",""requestID"":""req-001""}")
            });

            // Act & Assert - Should not throw despite event handler throwing
            var result = await client.RequestComplianceCertificateAsync("csr-content", "123456");
            Assert.NotNull(result);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        // Test helpers
        private class TestHttpMessageHandler : HttpMessageHandler
        {
            private HttpResponseMessage? _mockResponse;

            public void SetMockResponse(HttpResponseMessage response)
            {
                _mockResponse = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(_mockResponse ?? new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
        }

        private class TestAuditService : IZatcaAuditService
        {
            public List<ZatcaApiCallLog> Logs { get; } = new List<ZatcaApiCallLog>();

            public Task LogApiCallAsync(ZatcaApiCallLog log, CancellationToken cancellationToken = default)
            {
                Logs.Add(log);
                return Task.CompletedTask;
            }
        }

        private class TestDelegatingHandler : DelegatingHandler
        {
            public string Name { get; }

            public TestDelegatingHandler(string name)
            {
                Name = name;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
