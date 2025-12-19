using System.Collections.Generic;

namespace Zatca.EInvoice.Api
{
    /// <summary>
    /// Result of an invoice submission to ZATCA API.
    /// </summary>
    public class InvoiceSubmissionResult
    {
        /// <summary>
        /// Gets the overall status of the submission.
        /// </summary>
        public string Status { get; } = string.Empty;

        /// <summary>
        /// Gets the clearance status (for clearance invoices).
        /// </summary>
        public string? ClearanceStatus { get; }

        /// <summary>
        /// Gets the reporting status (for reporting invoices).
        /// </summary>
        public string? ReportingStatus { get; }

        /// <summary>
        /// Gets the cleared invoice XML (for clearance invoices).
        /// </summary>
        public string? ClearedInvoice { get; }

        /// <summary>
        /// Gets the list of validation errors.
        /// </summary>
        public List<ValidationMessage> Errors { get; }

        /// <summary>
        /// Gets the list of validation warnings.
        /// </summary>
        public List<ValidationMessage> Warnings { get; }

        /// <summary>
        /// Gets the list of informational messages.
        /// </summary>
        public List<ValidationMessage> InfoMessages { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceSubmissionResult"/> class.
        /// </summary>
        public InvoiceSubmissionResult()
        {
            Errors = new List<ValidationMessage>();
            Warnings = new List<ValidationMessage>();
            InfoMessages = new List<ValidationMessage>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceSubmissionResult"/> class.
        /// </summary>
        /// <param name="status">The overall status.</param>
        /// <param name="clearanceStatus">The clearance status.</param>
        /// <param name="reportingStatus">The reporting status.</param>
        /// <param name="clearedInvoice">The cleared invoice XML.</param>
        /// <param name="errors">The list of errors.</param>
        /// <param name="warnings">The list of warnings.</param>
        /// <param name="infoMessages">The list of informational messages.</param>
        public InvoiceSubmissionResult(
            string status,
            string? clearanceStatus,
            string? reportingStatus,
            string? clearedInvoice,
            List<ValidationMessage>? errors,
            List<ValidationMessage>? warnings,
            List<ValidationMessage>? infoMessages)
        {
            Status = status ?? string.Empty;
            ClearanceStatus = clearanceStatus;
            ReportingStatus = reportingStatus;
            ClearedInvoice = clearedInvoice;
            Errors = errors ?? new List<ValidationMessage>();
            Warnings = warnings ?? new List<ValidationMessage>();
            InfoMessages = infoMessages ?? new List<ValidationMessage>();
        }

        /// <summary>
        /// Gets a value indicating whether the submission was successful.
        /// </summary>
        public bool IsSuccess => Errors.Count == 0;

        /// <summary>
        /// Gets a value indicating whether there are warnings.
        /// </summary>
        public bool HasWarnings => Warnings.Count > 0;

        /// <summary>
        /// Gets a value indicating whether this is a clearance invoice.
        /// </summary>
        public bool IsClearance => !string.IsNullOrEmpty(ClearanceStatus);

        /// <summary>
        /// Gets a value indicating whether this is a reporting invoice.
        /// </summary>
        public bool IsReporting => !string.IsNullOrEmpty(ReportingStatus);
    }

    /// <summary>
    /// Represents a validation message from ZATCA API.
    /// </summary>
    public class ValidationMessage
    {
        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message category.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
        /// </summary>
        public ValidationMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="code">The message code.</param>
        /// <param name="category">The message category.</param>
        /// <param name="message">The message text.</param>
        /// <param name="status">The message status.</param>
        public ValidationMessage(string type, string code, string category, string message, string status)
        {
            Type = type;
            Code = code;
            Category = category;
            Message = message;
            Status = status;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{Type}] {Code}: {Message}";
        }
    }
}
