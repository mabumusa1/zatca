using System;
using System.Collections.Generic;

namespace Zatca.EInvoice.Exceptions
{
    /// <summary>
    /// Exception thrown for certificate generation errors.
    /// </summary>
    public class CertificateBuilderException : ZatcaException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateBuilderException"/> class.
        /// </summary>
        public CertificateBuilderException() : this("Certificate builder operation failed.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateBuilderException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CertificateBuilderException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateBuilderException"/> class with a specified error message and context.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="context">Additional context information about the error.</param>
        public CertificateBuilderException(string message, Dictionary<string, object> context) : base(message, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateBuilderException"/> class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CertificateBuilderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateBuilderException"/> class with a specified error message, context, and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="context">Additional context information about the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public CertificateBuilderException(string message, Dictionary<string, object> context, Exception innerException)
            : base(message, context, innerException)
        {
        }
    }
}
