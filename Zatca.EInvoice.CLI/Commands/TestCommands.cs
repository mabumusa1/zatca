using System.CommandLine;
using Zatca.EInvoice.CLI.Models;
using Zatca.EInvoice.CLI.Output;
using Zatca.EInvoice.CLI.Services;

namespace Zatca.EInvoice.CLI.Commands;

/// <summary>
/// Test command handlers.
/// </summary>
public static class TestCommands
{
    public static Command CreateTestCommand(ITestService testService, IOutputFormatter formatter)
    {
        var testCommand = new Command("test", "Run built-in test scenarios");

        testCommand.AddCommand(CreateListCommand(testService, formatter));
        testCommand.AddCommand(CreateRunCommand(testService, formatter));
        testCommand.AddCommand(CreateAllCommand(testService, formatter));

        return testCommand;
    }

    private static Command CreateListCommand(ITestService testService, IOutputFormatter formatter)
    {
        var listCommand = new Command("list", "List available test scenarios");

        var categoryOption = new Option<string?>("--category", "Filter by category: invoice|cert|sign|api|validation|xml|all");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        listCommand.AddOption(categoryOption);
        listCommand.AddOption(jsonOption);

        listCommand.SetHandler((category, jsonOutput) =>
        {
            var cat = ParseCategory(category);
            var scenarios = testService.GetScenarios(cat);

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    count = scenarios.Count,
                    category = cat?.ToString() ?? "all",
                    scenarios = scenarios.Select(s => new
                    {
                        name = s.Name,
                        description = s.Description,
                        category = s.Category.ToString()
                    })
                });
            }
            else
            {
                formatter.WriteHeader("Available Test Scenarios");
                formatter.WriteLine($"\nTotal: {scenarios.Count} scenarios\n");

                var grouped = scenarios.GroupBy(s => s.Category);
                foreach (var group in grouped)
                {
                    formatter.WriteInfo($"{group.Key}:");
                    foreach (var scenario in group)
                    {
                        formatter.WriteLine($"    {scenario.Name,-25} {scenario.Description}");
                    }
                    formatter.WriteLine();
                }
            }
        }, categoryOption, jsonOption);

        return listCommand;
    }

    private static Command CreateRunCommand(ITestService testService, IOutputFormatter formatter)
    {
        var runCommand = new Command("run", "Run specific test scenario(s)");

        var nameArgument = new Argument<string[]>("names", "Test scenario name(s) to run");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        runCommand.AddArgument(nameArgument);
        runCommand.AddOption(jsonOption);

        runCommand.SetHandler(async (names, jsonOutput) =>
        {
            var results = new List<(string Name, TestResult Result)>();

            foreach (var name in names)
            {
                var result = await testService.RunScenarioAsync(name);
                results.Add((name, result));
            }

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    count = results.Count,
                    passed = results.Count(r => r.Result.Passed),
                    failed = results.Count(r => !r.Result.Passed && !r.Result.Skipped),
                    skipped = results.Count(r => r.Result.Skipped),
                    results = results.Select(r => new
                    {
                        name = r.Name,
                        passed = r.Result.Passed,
                        skipped = r.Result.Skipped,
                        message = r.Result.Message,
                        durationMs = r.Result.Duration.TotalMilliseconds
                    })
                });
            }
            else
            {
                formatter.WriteHeader("Test Results");
                formatter.WriteLine();

                foreach (var (name, result) in results)
                {
                    formatter.WriteTestResult(name, result);
                }

                formatter.WriteLine();
                var passed = results.Count(r => r.Result.Passed);
                var failed = results.Count(r => !r.Result.Passed && !r.Result.Skipped);
                formatter.WriteInfo($"Results: {passed} passed, {failed} failed out of {results.Count} tests");
            }
        }, nameArgument, jsonOption);

        return runCommand;
    }

    private static Command CreateAllCommand(ITestService testService, IOutputFormatter formatter)
    {
        var allCommand = new Command("all", "Run all test scenarios");

        var categoryOption = new Option<string?>("--category", "Filter by category: invoice|cert|sign|api|validation|xml|all");
        var jsonOption = new Option<bool>("--json", () => false, "Output as JSON");

        allCommand.AddOption(categoryOption);
        allCommand.AddOption(jsonOption);

        allCommand.SetHandler(async (category, jsonOutput) =>
        {
            var cat = ParseCategory(category);
            var summary = await testService.RunAllAsync(cat);

            if (jsonOutput)
            {
                formatter.WriteJson(new
                {
                    total = summary.TotalTests,
                    passed = summary.Passed,
                    failed = summary.Failed,
                    skipped = summary.Skipped,
                    durationMs = summary.TotalDuration.TotalMilliseconds,
                    results = summary.Results.Select(r => new
                    {
                        name = r.Name,
                        passed = r.Result.Passed,
                        skipped = r.Result.Skipped,
                        message = r.Result.Message,
                        durationMs = r.Result.Duration.TotalMilliseconds
                    })
                });
            }
            else
            {
                formatter.WriteHeader("Running All Tests");
                formatter.WriteLine();

                foreach (var (name, result) in summary.Results)
                {
                    formatter.WriteTestResult(name, result);
                }

                formatter.WriteTestSummary(summary);
            }
        }, categoryOption, jsonOption);

        return allCommand;
    }

    private static TestCategory? ParseCategory(string? category)
    {
        if (string.IsNullOrEmpty(category) || category.Equals("all", StringComparison.OrdinalIgnoreCase))
            return null;

        return category.ToLowerInvariant() switch
        {
            "invoice" => TestCategory.Invoice,
            "cert" or "certificate" => TestCategory.Certificate,
            "sign" or "signing" => TestCategory.Signing,
            "api" => TestCategory.Api,
            "validation" => TestCategory.Validation,
            "xml" => TestCategory.Xml,
            _ => null
        };
    }
}
