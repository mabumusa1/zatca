using Zatca.EInvoice.CLI.Models;

namespace Zatca.EInvoice.CLI.Services;

/// <summary>
/// Service interface for certificate operations.
/// </summary>
public interface ICertificateService
{
    /// <summary>
    /// Generates a CSR and private key.
    /// </summary>
    CommandResult<CertificateGenerationResult> GenerateCsr(CertificateConfig config);

    /// <summary>
    /// Generates a CSR and saves to files.
    /// </summary>
    Task<CommandResult<CertificateGenerationResult>> GenerateAndSaveAsync(
        CertificateConfig config,
        string? outputDir = null,
        string? csrFileName = null,
        string? keyFileName = null);
}
