using Zatca.EInvoice.CLI.Models;

namespace Zatca.EInvoice.CLI.Output;

/// <summary>
/// Interface for formatting command output.
/// </summary>
public interface IOutputFormatter
{
    void WriteSuccess(string message);
    void WriteError(string message);
    void WriteWarning(string message);
    void WriteInfo(string message);
    void WriteLine(string message = "");
    void WriteHeader(string title);
    void WriteKeyValue(string key, string? value);
    void WriteResult<T>(CommandResult<T> result);
    void WriteTestResult(string testName, TestResult result);
    void WriteTestSummary(TestRunSummary summary);
    void WriteJson<T>(T obj);
}
