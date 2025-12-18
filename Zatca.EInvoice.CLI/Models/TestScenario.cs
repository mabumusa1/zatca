namespace Zatca.EInvoice.CLI.Models;

/// <summary>
/// Test scenario categories.
/// </summary>
public enum TestCategory
{
    Invoice,
    Certificate,
    Signing,
    Api,
    Validation,
    Xml,
    All
}

/// <summary>
/// Defines a test scenario.
/// </summary>
public class TestScenario
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TestCategory Category { get; set; }
    public Func<Task<TestResult>> Execute { get; set; } = () => Task.FromResult(TestResult.Skip("Not implemented"));
}

/// <summary>
/// Result of a test scenario execution.
/// </summary>
public class TestResult
{
    public bool Passed { get; set; }
    public bool Skipped { get; set; }
    public string? Message { get; set; }
    public string? ErrorDetails { get; set; }
    public TimeSpan Duration { get; set; }

    public static TestResult Pass(string? message = null) => new()
    {
        Passed = true,
        Message = message
    };

    public static TestResult Fail(string message, string? errorDetails = null) => new()
    {
        Passed = false,
        Message = message,
        ErrorDetails = errorDetails
    };

    public static TestResult Skip(string reason) => new()
    {
        Skipped = true,
        Message = reason
    };
}

/// <summary>
/// Summary of test run results.
/// </summary>
public class TestRunSummary
{
    public int TotalTests { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public List<(string Name, TestResult Result)> Results { get; set; } = new();
}
