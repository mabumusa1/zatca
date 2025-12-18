using Zatca.EInvoice.CLI.Models;

namespace Zatca.EInvoice.CLI.Services;

/// <summary>
/// Service interface for test operations.
/// </summary>
public interface ITestService
{
    /// <summary>
    /// Gets all available test scenarios.
    /// </summary>
    List<TestScenario> GetScenarios(TestCategory? category = null);

    /// <summary>
    /// Runs a specific test scenario by name.
    /// </summary>
    Task<TestResult> RunScenarioAsync(string name);

    /// <summary>
    /// Runs all test scenarios.
    /// </summary>
    Task<TestRunSummary> RunAllAsync(TestCategory? category = null);
}
