namespace Zatca.EInvoice.CLI.Models;

/// <summary>
/// Configuration for certificate/CSR generation.
/// </summary>
public class CertificateConfig
{
    public string OrganizationIdentifier { get; set; } = string.Empty;
    public string SolutionName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string CommonName { get; set; } = string.Empty;
    public string CountryName { get; set; } = "SA";
    public string OrganizationName { get; set; } = string.Empty;
    public string OrganizationalUnitName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int InvoiceType { get; set; } = 1100;
    public string BusinessCategory { get; set; } = string.Empty;
    public bool IsProduction { get; set; } = false;
}

/// <summary>
/// Result of CSR generation.
/// </summary>
public class CertificateGenerationResult
{
    public string Csr { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string? CsrFilePath { get; set; }
    public string? PrivateKeyFilePath { get; set; }
}
