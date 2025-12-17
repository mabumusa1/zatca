using System;
using System.Collections.Generic;

namespace Zatca.EInvoice.Exceptions
{
    /// <summary>
    /// Exception thrown for file I/O errors.
    /// </summary>
    public class ZatcaStorageException : ZatcaException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaStorageException"/> class.
        /// </summary>
        public ZatcaStorageException() : this("Storage operation failed.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaStorageException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ZatcaStorageException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaStorageException"/> class with a specified error message and context.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="context">Additional context information about the error.</param>
        public ZatcaStorageException(string message, Dictionary<string, object> context) : base(message, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaStorageException"/> class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ZatcaStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaStorageException"/> class with a specified error message, context, and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="context">Additional context information about the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ZatcaStorageException(string message, Dictionary<string, object> context, Exception innerException)
            : base(message, context, innerException)
        {
        }
    }
}
