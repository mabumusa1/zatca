using System.Security.Cryptography.X509Certificates;
using Zatca.EInvoice.CLI.Models;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Signing;

namespace Zatca.EInvoice.CLI.Services;

/// <summary>
/// Service interface for invoice operations.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Creates an Invoice object from JSON file.
    /// </summary>
    CommandResult<Invoice> CreateFromJson(string jsonFilePath);

    /// <summary>
    /// Creates an Invoice object from JSON string.
    /// </summary>
    CommandResult<Invoice> CreateFromJsonString(string jsonContent);

    /// <summary>
    /// Validates invoice data from JSON file.
    /// </summary>
    CommandResult<List<string>> ValidateFromJson(string jsonFilePath);

    /// <summary>
    /// Generates UBL XML from JSON file.
    /// </summary>
    CommandResult<string> GenerateXml(string jsonFilePath, string currency = "SAR");

    /// <summary>
    /// Generates UBL XML from dictionary.
    /// </summary>
    CommandResult<string> GenerateXmlFromData(Dictionary<string, object> data, string currency = "SAR");

    /// <summary>
    /// Signs an invoice XML with a certificate.
    /// </summary>
    CommandResult<SignedInvoiceResult> SignInvoice(string xmlContent, X509Certificate2 certificate);

    /// <summary>
    /// Computes the hash of an invoice XML.
    /// </summary>
    CommandResult<string> ComputeHash(string xmlContent);

    /// <summary>
    /// Loads invoice data from JSON file as dictionary.
    /// </summary>
    CommandResult<Dictionary<string, object>> LoadJsonData(string jsonFilePath);
}
