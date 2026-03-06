using System;
using System.Collections.Generic;

namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// Event arguments for ZATCA API response events.
    /// </summary>
    public class ZatcaApiResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the operation type identifier (e.g., "OnboardCompliance", "SubmitInvoice", "RenewCertificate").
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

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
        public Dictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the request body/payload.
        /// </summary>
        public string? RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the request timestamp.
        /// </summary>
        public DateTimeOffset RequestTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the response headers.
        /// </summary>
        public Dictionary<string, string> ResponseHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the response body/payload.
        /// </summary>
        public string? ResponseBody { get; set; }

        /// <summary>
        /// Gets or sets the response timestamp.
        /// </summary>
        public DateTimeOffset ResponseTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the duration of the request.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets whether the request was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the error/exception if the request failed.
        /// </summary>
        public Exception? Error { get; set; }

        /// <summary>
        /// Gets or sets the ZATCA environment (Sandbox, Simulation, Production).
        /// </summary>
        public ZatcaEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets custom context data that consumers can attach.
        /// </summary>
        public object? Context { get; set; }
    }
}
