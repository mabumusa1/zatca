using System;
using System.Collections.Generic;

namespace Zatca.EInvoice.Exceptions
{
    /// <summary>
    /// Base exception class for ZATCA-related errors.
    /// </summary>
    public class ZatcaException : Exception
    {
        /// <summary>
        /// Gets the context dictionary containing additional error information.
        /// </summary>
        public Dictionary<string, object> Context { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaException"/> class.
        /// </summary>
        public ZatcaException() : this("An error occurred")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ZatcaException(string message) : this(message, new Dictionary<string, object>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaException"/> class with a specified error message and context.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="context">Additional context information about the error.</param>
        public ZatcaException(string message, Dictionary<string, object> context) : base(message)
        {
            Context = context ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaException"/> class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ZatcaException(string message, Exception innerException) : this(message, new Dictionary<string, object>(), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZatcaException"/> class with a specified error message, context, and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="context">Additional context information about the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ZatcaException(string message, Dictionary<string, object> context, Exception? innerException) : base(message, innerException)
        {
            Context = context ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Adds context information to the exception.
        /// </summary>
        /// <param name="context">Additional context to merge with existing context.</param>
        /// <returns>The current exception instance.</returns>
        public ZatcaException WithContext(Dictionary<string, object> context)
        {
            if (context != null)
            {
                foreach (var kvp in context)
                {
                    Context[kvp.Key] = kvp.Value;
                }
            }
            return this;
        }
    }
}
