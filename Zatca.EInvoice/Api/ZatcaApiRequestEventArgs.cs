using System;
using System.Collections.Generic;

namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// Event arguments for ZATCA API request events.
    /// </summary>
    public class ZatcaApiRequestEventArgs : EventArgs
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
        /// Gets or sets the ZATCA environment (Sandbox, Simulation, Production).
        /// </summary>
        public ZatcaEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets custom context data that consumers can attach.
        /// </summary>
        public object? Context { get; set; }
    }
}
