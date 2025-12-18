using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Zatca.EInvoice.CLI.Models;
using Zatca.EInvoice.Mappers;
using Zatca.EInvoice.Models;
using Zatca.EInvoice.Signing;
using Zatca.EInvoice.Validation;
using Zatca.EInvoice.Xml;

namespace Zatca.EInvoice.CLI.Services;

/// <summary>
/// Service for invoice operations.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly InvoiceMapper _mapper;
    private readonly InvoiceValidator _validator;

    public InvoiceService()
    {
        _mapper = new InvoiceMapper();
        _validator = new InvoiceValidator();
    }

    /// <inheritdoc/>
    public CommandResult<Invoice> CreateFromJson(string jsonFilePath)
    {
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                return CommandResult<Invoice>.Fail($"File not found: {jsonFilePath}");
            }

            var jsonContent = File.ReadAllText(jsonFilePath);
            return CreateFromJsonString(jsonContent);
        }
        catch (Exception ex)
        {
            return CommandResult<Invoice>.Fail($"Failed to read file: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public CommandResult<Invoice> CreateFromJsonString(string jsonContent)
    {
        try
        {
            var invoice = _mapper.MapToInvoice(jsonContent);
            return CommandResult<Invoice>.Ok(invoice);
        }
        catch (Exception ex)
        {
            return CommandResult<Invoice>.Fail($"Failed to create invoice: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public CommandResult<List<string>> ValidateFromJson(string jsonFilePath)
    {
        try
        {
            var loadResult = LoadJsonData(jsonFilePath);
            if (!loadResult.Success)
            {
                return CommandResult<List<string>>.Fail(loadResult.ErrorMessage!);
            }

            var errors = new List<string>();
            var result = _validator.Validate(loadResult.Data!);

            if (!result.IsValid)
            {
                errors.AddRange(result.Errors);
            }

            // Also validate amounts if invoice data is complete enough
            var amountValidator = new InvoiceAmountValidator();
            var amountResult = amountValidator.ValidateMonetaryTotals(loadResult.Data!);
            if (!amountResult.IsValid)
            {
                errors.AddRange(amountResult.Errors);
            }

            var commandResult = CommandResult<List<string>>.Ok(errors);
            if (errors.Count > 0)
            {
                commandResult.ErrorMessage = $"Found {errors.Count} validation error(s)";
            }
            return commandResult;
        }
        catch (Exception ex)
        {
            return CommandResult<List<string>>.Fail($"Validation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public CommandResult<string> GenerateXml(string jsonFilePath, string currency = "SAR")
    {
        try
        {
            var loadResult = LoadJsonData(jsonFilePath);
            if (!loadResult.Success)
            {
                return CommandResult<string>.Fail(loadResult.ErrorMessage!);
            }

            return GenerateXmlFromData(loadResult.Data!, currency);
        }
        catch (Exception ex)
        {
            return CommandResult<string>.Fail($"Failed to generate XML: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public CommandResult<string> GenerateXmlFromData(Dictionary<string, object> data, string currency = "SAR")
    {
        try
        {
            var generator = new InvoiceGenerator(currency);
            var xml = generator.Generate(data);
            return CommandResult<string>.Ok(xml);
        }
        catch (Exception ex)
        {
            return CommandResult<string>.Fail($"Failed to generate XML: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public CommandResult<SignedInvoiceResult> SignInvoice(string xmlContent, X509Certificate2 certificate)
    {
        try
        {
            var result = InvoiceSigner.Sign(xmlContent, certificate);
            return CommandResult<SignedInvoiceResult>.Ok(result);
        }
        catch (Exception ex)
        {
            return CommandResult<SignedInvoiceResult>.Fail($"Failed to sign invoice: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public CommandResult<string> ComputeHash(string xmlContent)
    {
        try
        {
            var hash = InvoiceSigner.GetHash(xmlContent);
            return CommandResult<string>.Ok(hash);
        }
        catch (Exception ex)
        {
            return CommandResult<string>.Fail($"Failed to compute hash: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public CommandResult<Dictionary<string, object>> LoadJsonData(string jsonFilePath)
    {
        try
        {
            if (!File.Exists(jsonFilePath))
            {
                return CommandResult<Dictionary<string, object>>.Fail($"File not found: {jsonFilePath}");
            }

            var jsonContent = File.ReadAllText(jsonFilePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null)
            {
                return CommandResult<Dictionary<string, object>>.Fail("Failed to parse JSON data");
            }

            return CommandResult<Dictionary<string, object>>.Ok(data);
        }
        catch (JsonException ex)
        {
            return CommandResult<Dictionary<string, object>>.Fail($"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return CommandResult<Dictionary<string, object>>.Fail($"Failed to load file: {ex.Message}");
        }
    }
}
