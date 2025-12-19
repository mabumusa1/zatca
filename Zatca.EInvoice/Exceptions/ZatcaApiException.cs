using System;
using System.Collections.Generic;

namespace Zatca.EInvoice.Exceptions
{
    /// <summary>
    /// Exception thrown for ZATCA API communication errors.
    /// </summary>
    public class ZatcaApiException : ZatcaException
    {
        /// <summary>
        /// Gets the HTTP status code associated with the error.
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// Gets the API response content.
        /// </summary>
        public string Response { get; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaApiException"/> class.
        /// </summary>
        public ZatcaApiException() : this("ZATCA API request failed.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaApiException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ZatcaApiException(string message) : this(message, new Dictionary<string, object>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaApiException"/> class with a specified error message and context.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="context">Additional context information about the error.</param>
        public ZatcaApiException(string message, Dictionary<string, object> context) : base(message ?? "ZATCA API request failed.", context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaApiException"/> class with a specified error message, context, status code, and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="context">Additional context information about the error.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ZatcaApiException(string message, Dictionary<string, object> context, int statusCode, Exception? innerException = null)
            : base(message ?? "ZATCA API request failed.", context, innerException)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaApiException"/> class with status code and response.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="response">The API response content.</param>
        public ZatcaApiException(string message, int statusCode, string response) : base(message ?? "ZATCA API request failed.")
        {
            StatusCode = statusCode;
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaApiException"/> class with status code, response, and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="response">The API response content.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ZatcaApiException(string message, int statusCode, string response, Exception innerException)
            : base(message ?? "ZATCA API request failed.", innerException)
        {
            StatusCode = statusCode;
            Response = response;
        }
    }
}
