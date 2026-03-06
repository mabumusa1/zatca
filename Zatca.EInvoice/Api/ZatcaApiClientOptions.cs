using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// Configuration options for <see cref="ZatcaApiClient"/>.
    /// </summary>
    public class ZatcaApiClientOptions
    {
        /// <summary>
        /// Gets or sets the ZATCA environment.
        /// </summary>
        public ZatcaEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request timeout. Default is 30 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets whether to allow warnings in responses.
        /// </summary>
        public bool AllowWarnings { get; set; }

        /// <summary>
        /// Gets or sets custom DelegatingHandlers to inject into the HTTP pipeline.
        /// These handlers can be used for cross-cutting concerns like logging, retry, authentication, etc.
        /// </summary>
        public List<DelegatingHandler>? Handlers { get; set; }

        /// <summary>
        /// Gets or sets an optional pre-configured HttpClient instance.
        /// If provided, the Handlers and Timeout properties will be ignored.
        /// </summary>
        public HttpClient? HttpClient { get; set; }
    }
}
