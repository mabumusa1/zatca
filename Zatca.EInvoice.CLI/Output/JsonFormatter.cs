using System.Text.Json;
using Zatca.EInvoice.CLI.Models;

namespace Zatca.EInvoice.CLI.Output;

/// <summary>
/// JSON output formatter for programmatic consumption.
/// </summary>
public class JsonFormatter : IOutputFormatter
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void WriteSuccess(string message) => WriteJsonMessage("success", message);
    public void WriteError(string message) => WriteJsonMessage("error", message);
    public void WriteWarning(string message) => WriteJsonMessage("warning", message);
    public void WriteInfo(string message) => WriteJsonMessage("info", message);
    public void WriteLine(string message = "") { }
    public void WriteHeader(string title) { }
    public void WriteKeyValue(string key, string? value) { }

    public void WriteResult<T>(CommandResult<T> result)
    {
        var output = new
        {
            success = result.Success,
            data = result.Data,
            error = result.ErrorMessage,
            warnings = result.Warnings,
            info = result.InfoMessages
        };
        Console.WriteLine(JsonSerializer.Serialize(output, _jsonOptions));
    }

    public void WriteTestResult(string testName, TestResult result)
    {
        var output = new
        {
            test = testName,
            passed = result.Passed,
            skipped = result.Skipped,
            message = result.Message,
            errorDetails = result.ErrorDetails,
            durationMs = result.Duration.TotalMilliseconds
        };
        Console.WriteLine(JsonSerializer.Serialize(output, _jsonOptions));
    }

    public void WriteTestSummary(TestRunSummary summary)
    {
        var output = new
        {
            total = summary.TotalTests,
            passed = summary.Passed,
            failed = summary.Failed,
            skipped = summary.Skipped,
            durationMs = summary.TotalDuration.TotalMilliseconds,
            results = summary.Results.Select(r => new
            {
                test = r.Name,
                passed = r.Result.Passed,
                skipped = r.Result.Skipped,
                message = r.Result.Message
            })
        };
        Console.WriteLine(JsonSerializer.Serialize(output, _jsonOptions));
    }

    public void WriteJson<T>(T obj)
    {
        Console.WriteLine(JsonSerializer.Serialize(obj, _jsonOptions));
    }

    private void WriteJsonMessage(string type, string message)
    {
        var output = new { type, message };
        Console.WriteLine(JsonSerializer.Serialize(output, _jsonOptions));
    }
}
