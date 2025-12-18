using System.CommandLine;
using Zatca.EInvoice.CLI.Commands;
using Zatca.EInvoice.CLI.Output;
using Zatca.EInvoice.CLI.Services;

namespace Zatca.EInvoice.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Create services
        var formatter = new ConsoleFormatter();
        var fileWriter = new FileWriter(formatter);
        var certService = new CertificateService(fileWriter);
        var invoiceService = new InvoiceService();
        var apiService = new ApiService();
        var testService = new TestService();

        // Create root command
        var rootCommand = new RootCommand("ZATCA E-Invoice CLI - Comprehensive testing tool for ZATCA e-invoicing")
        {
            Name = "zatca-cli"
        };

        // Add subcommands
        rootCommand.AddCommand(CertCommands.CreateCertCommand(certService, formatter, fileWriter));
        rootCommand.AddCommand(InvoiceCommands.CreateInvoiceCommand(invoiceService, formatter, fileWriter));
        rootCommand.AddCommand(ApiCommands.CreateApiCommand(apiService, formatter, fileWriter));
        rootCommand.AddCommand(TestCommands.CreateTestCommand(testService, formatter));
        rootCommand.AddCommand(SampleCommands.CreateSampleCommand(formatter, fileWriter));

        // Handle root command (show help or interactive menu)
        rootCommand.SetHandler(() =>
        {
            ShowBanner();
            Console.WriteLine("Use --help to see available commands.\n");
            Console.WriteLine("Quick Start:");
            Console.WriteLine("  zatca-cli cert generate --help     Generate CSR and private key");
            Console.WriteLine("  zatca-cli invoice xml --help        Generate invoice XML from JSON");
            Console.WriteLine("  zatca-cli test all                  Run all built-in tests");
            Console.WriteLine("  zatca-cli sample invoice            Generate sample invoice JSON");
            Console.WriteLine();
        });

        return await rootCommand.InvokeAsync(args);
    }

    static void ShowBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
 _____    _             _____ _____            _
|__  /_ _| |_ ___ __ _ | ____|_   _|_ ____   _(_) ___ ___
  / /| '_|  _/ __/ _` ||  _|   | | | '_ \ \ / / |/ _ | __|
 / /_| | | || (_| (_| || |___  | | | | | \ V /| | (_) \__ \
/____|_|  \__\___\__,_||_____| |_| |_| |_|\_/ |_|\___/|___/

");
        Console.ResetColor();
        Console.WriteLine("ZATCA E-Invoice CLI - Version 1.0.0");
        Console.WriteLine("Comprehensive testing tool for Saudi Arabia e-invoicing\n");
    }
}
