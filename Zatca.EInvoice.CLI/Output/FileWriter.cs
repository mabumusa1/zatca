using System.Text;
using System.Text.Json;

namespace Zatca.EInvoice.CLI.Output;

/// <summary>
/// Utility for writing output to files.
/// </summary>
public class FileWriter
{
    private readonly IOutputFormatter _formatter;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FileWriter(IOutputFormatter formatter)
    {
        _formatter = formatter;
    }

    public async Task<string> WriteTextAsync(string content, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
        _formatter.WriteInfo($"Written to: {filePath}");
        return filePath;
    }

    public async Task<string> WriteJsonAsync<T>(T content, string filePath)
    {
        var json = JsonSerializer.Serialize(content, _jsonOptions);
        return await WriteTextAsync(json, filePath);
    }

    public async Task<string> WriteXmlAsync(string xml, string filePath)
    {
        return await WriteTextAsync(xml, filePath);
    }

    public async Task<string> WriteCsrAsync(string csr, string filePath)
    {
        var csrContent = csr.StartsWith("-----BEGIN")
            ? csr
            : $"-----BEGIN CERTIFICATE REQUEST-----\n{csr}\n-----END CERTIFICATE REQUEST-----";
        return await WriteTextAsync(csrContent, filePath);
    }

    public async Task<string> WritePrivateKeyAsync(string privateKey, string filePath)
    {
        var keyContent = privateKey.StartsWith("-----BEGIN")
            ? privateKey
            : $"-----BEGIN EC PRIVATE KEY-----\n{privateKey}\n-----END EC PRIVATE KEY-----";
        return await WriteTextAsync(keyContent, filePath);
    }

    public async Task<string> WriteCertificateAsync(string certificate, string filePath)
    {
        var certContent = certificate.StartsWith("-----BEGIN")
            ? certificate
            : $"-----BEGIN CERTIFICATE-----\n{certificate}\n-----END CERTIFICATE-----";
        return await WriteTextAsync(certContent, filePath);
    }

    public string GetDefaultOutputPath(string baseName, string extension)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(Directory.GetCurrentDirectory(), $"{baseName}_{timestamp}.{extension}");
    }

    public string EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
}
