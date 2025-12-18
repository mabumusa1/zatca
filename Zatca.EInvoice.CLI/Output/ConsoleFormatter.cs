using System.Text.Json;
using Zatca.EInvoice.CLI.Models;

namespace Zatca.EInvoice.CLI.Output;

/// <summary>
/// Console output formatter with colors and structure.
/// </summary>
public class ConsoleFormatter : IOutputFormatter
{
    private readonly bool _useColors;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ConsoleFormatter(bool useColors = true)
    {
        _useColors = useColors;
    }

    public void WriteSuccess(string message)
    {
        WriteColored($"✓ {message}", ConsoleColor.Green);
    }

    public void WriteError(string message)
    {
        WriteColored($"✗ {message}", ConsoleColor.Red);
    }

    public void WriteWarning(string message)
    {
        WriteColored($"⚠ {message}", ConsoleColor.Yellow);
    }

    public void WriteInfo(string message)
    {
        WriteColored($"ℹ {message}", ConsoleColor.Cyan);
    }

    public void WriteLine(string message = "")
    {
        Console.WriteLine(message);
    }

    public void WriteHeader(string title)
    {
        var line = new string('═', Math.Max(title.Length + 4, 40));
        Console.WriteLine();
        WriteColored(line, ConsoleColor.DarkCyan);
        WriteColored($"  {title}", ConsoleColor.White);
        WriteColored(line, ConsoleColor.DarkCyan);
    }

    public void WriteKeyValue(string key, string? value)
    {
        WriteColored($"  {key}: ", ConsoleColor.Gray, newLine: false);
        Console.WriteLine(value ?? "(null)");
    }

    public void WriteResult<T>(CommandResult<T> result)
    {
        if (result.Success)
        {
            WriteSuccess("Operation completed successfully");
        }
        else
        {
            WriteError(result.ErrorMessage ?? "Operation failed");
        }

        foreach (var warning in result.Warnings)
        {
            WriteWarning(warning);
        }

        foreach (var info in result.InfoMessages)
        {
            WriteInfo(info);
        }
    }

    public void WriteTestResult(string testName, TestResult result)
    {
        if (result.Skipped)
        {
            WriteColored($"  ○ {testName} - SKIPPED: {result.Message}", ConsoleColor.Gray);
        }
        else if (result.Passed)
        {
            WriteColored($"  ✓ {testName} - PASSED", ConsoleColor.Green);
            if (!string.IsNullOrEmpty(result.Message))
            {
                WriteColored($"    {result.Message}", ConsoleColor.DarkGray);
            }
        }
        else
        {
            WriteColored($"  ✗ {testName} - FAILED", ConsoleColor.Red);
            if (!string.IsNullOrEmpty(result.Message))
            {
                WriteColored($"    {result.Message}", ConsoleColor.Red);
            }
            if (!string.IsNullOrEmpty(result.ErrorDetails))
            {
                WriteColored($"    Details: {result.ErrorDetails}", ConsoleColor.DarkRed);
            }
        }
    }

    public void WriteTestSummary(TestRunSummary summary)
    {
        WriteHeader("Test Summary");
        WriteLine();
        WriteKeyValue("Total Tests", summary.TotalTests.ToString());
        WriteColored($"  Passed: {summary.Passed}", ConsoleColor.Green);
        WriteColored($"  Failed: {summary.Failed}", ConsoleColor.Red);
        WriteColored($"  Skipped: {summary.Skipped}", ConsoleColor.Gray);
        WriteKeyValue("Duration", $"{summary.TotalDuration.TotalMilliseconds:F0}ms");
        WriteLine();
    }

    public void WriteJson<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, _jsonOptions);
        Console.WriteLine(json);
    }

    private void WriteColored(string message, ConsoleColor color, bool newLine = true)
    {
        if (_useColors)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);
            Console.ForegroundColor = originalColor;
        }
        else
        {
            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);
        }
    }
}
