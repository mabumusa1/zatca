using Zatca.EInvoice.Certificates;
using Zatca.EInvoice.CLI.Models;
using Zatca.EInvoice.CLI.Output;

namespace Zatca.EInvoice.CLI.Services;

/// <summary>
/// Service for certificate operations.
/// </summary>
public class CertificateService : ICertificateService
{
    private readonly FileWriter _fileWriter;

    public CertificateService(FileWriter fileWriter)
    {
        _fileWriter = fileWriter;
    }

    /// <inheritdoc/>
    public CommandResult<CertificateGenerationResult> GenerateCsr(CertificateConfig config)
    {
        try
        {
            var builder = new CertificateBuilder()
                .SetOrganizationIdentifier(config.OrganizationIdentifier)
                .SetSerialNumber(config.SolutionName, config.Model, config.SerialNumber)
                .SetCommonName(config.CommonName)
                .SetCountryName(config.CountryName)
                .SetOrganizationName(config.OrganizationName)
                .SetOrganizationalUnitName(config.OrganizationalUnitName)
                .SetAddress(config.Address)
                .SetInvoiceType(config.InvoiceType)
                .SetBusinessCategory(config.BusinessCategory)
                .SetProduction(config.IsProduction);

            builder.Generate();

            var result = new CertificateGenerationResult
            {
                Csr = builder.GetCsr(),
                PrivateKey = builder.GetPrivateKey()
            };

            return CommandResult<CertificateGenerationResult>.Ok(result);
        }
        catch (Exception ex)
        {
            return CommandResult<CertificateGenerationResult>.Fail($"Failed to generate CSR: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<CommandResult<CertificateGenerationResult>> GenerateAndSaveAsync(
        CertificateConfig config,
        string? outputDir = null,
        string? csrFileName = null,
        string? keyFileName = null)
    {
        var generateResult = GenerateCsr(config);
        if (!generateResult.Success)
        {
            return generateResult;
        }

        var result = generateResult.Data!;
        var dir = outputDir ?? Directory.GetCurrentDirectory();
        _fileWriter.EnsureDirectory(dir);

        var csrFile = csrFileName ?? "certificate.csr";
        var keyFile = keyFileName ?? "private.pem";

        var csrPath = Path.Combine(dir, csrFile);
        var keyPath = Path.Combine(dir, keyFile);

        try
        {
            await _fileWriter.WriteCsrAsync(result.Csr, csrPath);
            await _fileWriter.WritePrivateKeyAsync(result.PrivateKey, keyPath);

            result.CsrFilePath = csrPath;
            result.PrivateKeyFilePath = keyPath;

            return CommandResult<CertificateGenerationResult>.Ok(result);
        }
        catch (Exception ex)
        {
            return CommandResult<CertificateGenerationResult>.Fail($"Failed to save files: {ex.Message}");
        }
    }
}
