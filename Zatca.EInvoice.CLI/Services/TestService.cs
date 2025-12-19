using System.Diagnostics;
using Zatca.EInvoice.Certificates;
using Zatca.EInvoice.CLI.Models;
using Zatca.EInvoice.Mappers;
using Zatca.EInvoice.Validation;
using Zatca.EInvoice.Xml;

namespace Zatca.EInvoice.CLI.Services;

/// <summary>
/// Service for test operations.
/// </summary>
public class TestService : ITestService
{
    private readonly List<TestScenario> _scenarios;

    public TestService()
    {
        _scenarios = InitializeScenarios();
    }

    /// <inheritdoc/>
    public List<TestScenario> GetScenarios(TestCategory? category = null)
    {
        if (category == null || category == TestCategory.All)
        {
            return _scenarios;
        }

        return _scenarios.Where(s => s.Category == category).ToList();
    }

    /// <inheritdoc/>
    public async Task<TestResult> RunScenarioAsync(string name)
    {
        var scenario = _scenarios.FirstOrDefault(s =>
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (scenario == null)
        {
            return TestResult.Fail($"Test scenario '{name}' not found");
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await scenario.Execute();
            result.Duration = sw.Elapsed;
            return result;
        }
        catch (Exception ex)
        {
            return TestResult.Fail($"Test threw exception: {ex.Message}", ex.StackTrace);
        }
        finally
        {
            sw.Stop();
        }
    }

    /// <inheritdoc/>
    public async Task<TestRunSummary> RunAllAsync(TestCategory? category = null)
    {
        var scenarios = GetScenarios(category);
        var summary = new TestRunSummary
        {
            TotalTests = scenarios.Count
        };

        var overallSw = Stopwatch.StartNew();

        foreach (var scenario in scenarios)
        {
            var sw = Stopwatch.StartNew();
            TestResult result;

            try
            {
                result = await scenario.Execute();
            }
            catch (Exception ex)
            {
                result = TestResult.Fail($"Test threw exception: {ex.Message}", ex.StackTrace);
            }

            result.Duration = sw.Elapsed;
            summary.Results.Add((scenario.Name, result));

            if (result.Skipped)
                summary.Skipped++;
            else if (result.Passed)
                summary.Passed++;
            else
                summary.Failed++;
        }

        summary.TotalDuration = overallSw.Elapsed;
        return summary;
    }

    private List<TestScenario> InitializeScenarios()
    {
        return new List<TestScenario>
        {
            // Certificate Tests
            new TestScenario
            {
                Name = "cert-generation",
                Description = "Test CSR and private key generation",
                Category = TestCategory.Certificate,
                Execute = TestCertificateGeneration
            },
            new TestScenario
            {
                Name = "cert-validation",
                Description = "Test certificate parameter validation",
                Category = TestCategory.Certificate,
                Execute = TestCertificateValidation
            },

            // Invoice Tests
            new TestScenario
            {
                Name = "standard-invoice",
                Description = "Test standard invoice (B2B) creation",
                Category = TestCategory.Invoice,
                Execute = TestStandardInvoice
            },
            new TestScenario
            {
                Name = "simplified-invoice",
                Description = "Test simplified invoice (B2C) creation",
                Category = TestCategory.Invoice,
                Execute = TestSimplifiedInvoice
            },
            new TestScenario
            {
                Name = "debit-note",
                Description = "Test debit note creation",
                Category = TestCategory.Invoice,
                Execute = TestDebitNote
            },
            new TestScenario
            {
                Name = "credit-note",
                Description = "Test credit note creation",
                Category = TestCategory.Invoice,
                Execute = TestCreditNote
            },
            new TestScenario
            {
                Name = "multi-line-invoice",
                Description = "Test invoice with multiple lines",
                Category = TestCategory.Invoice,
                Execute = TestMultiLineInvoice
            },

            // Validation Tests
            new TestScenario
            {
                Name = "invoice-validation",
                Description = "Test invoice data validation",
                Category = TestCategory.Validation,
                Execute = TestInvoiceValidation
            },
            new TestScenario
            {
                Name = "amount-validation",
                Description = "Test invoice amount validation",
                Category = TestCategory.Validation,
                Execute = TestAmountValidation
            },

            // XML Tests
            new TestScenario
            {
                Name = "xml-generation",
                Description = "Test UBL XML generation",
                Category = TestCategory.Xml,
                Execute = TestXmlGeneration
            },
            new TestScenario
            {
                Name = "xml-namespaces",
                Description = "Test XML namespace handling",
                Category = TestCategory.Xml,
                Execute = TestXmlNamespaces
            }
        };
    }

    #region Test Implementations

    private Task<TestResult> TestCertificateGeneration()
    {
        try
        {
            var builder = new CertificateBuilder()
                .SetOrganizationIdentifier("399999999900003")
                .SetSerialNumber("TST", "TST", "ed22f1d8-e6a2-1118-9b58-d9a8f11e445f")
                .SetCommonName("TST-886431145-399999999900003")
                .SetCountryName("SA")
                .SetOrganizationName("Test Company LLC")
                .SetOrganizationalUnitName("IT Department")
                .SetAddress("Riyadh Saudi Arabia")
                .SetInvoiceType(1100)
                .SetBusinessCategory("IT Services")
                .SetProduction(false);

            builder.Generate();

            var csr = builder.GetCsr();
            var privateKey = builder.GetPrivateKey();

            if (string.IsNullOrEmpty(csr))
                return Task.FromResult(TestResult.Fail("CSR is empty"));
            if (string.IsNullOrEmpty(privateKey))
                return Task.FromResult(TestResult.Fail("Private key is empty"));
            if (!csr.Contains("BEGIN CERTIFICATE REQUEST"))
                return Task.FromResult(TestResult.Fail("CSR format is invalid"));
            if (!privateKey.Contains("BEGIN EC PRIVATE KEY"))
                return Task.FromResult(TestResult.Fail("Private key format is invalid"));

            return Task.FromResult(TestResult.Pass($"CSR: {csr.Length} chars, Key: {privateKey.Length} chars"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestCertificateValidation()
    {
        try
        {
            // Test invalid organization ID
            try
            {
                new CertificateBuilder().SetOrganizationIdentifier("123456789");
                return Task.FromResult(TestResult.Fail("Should have rejected invalid org ID"));
            }
            catch { }

            // Test invalid country code
            try
            {
                new CertificateBuilder().SetCountryName("USA");
                return Task.FromResult(TestResult.Fail("Should have rejected invalid country code"));
            }
            catch { }

            return Task.FromResult(TestResult.Pass("Validation correctly rejects invalid inputs"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestStandardInvoice()
    {
        try
        {
            var data = GetStandardInvoiceData();
            var mapper = new InvoiceMapper();
            var invoice = mapper.MapToInvoice(data);

            if (invoice.InvoiceType?.Invoice != "standard")
                return Task.FromResult(TestResult.Fail("Invoice type should be 'standard'"));
            if (invoice.AccountingCustomerParty == null)
                return Task.FromResult(TestResult.Fail("Standard invoice should have customer"));

            return Task.FromResult(TestResult.Pass($"Invoice ID: {invoice.Id}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestSimplifiedInvoice()
    {
        try
        {
            var data = GetSimplifiedInvoiceData();
            var mapper = new InvoiceMapper();
            var invoice = mapper.MapToInvoice(data);

            if (invoice.InvoiceType?.Invoice != "simplified")
                return Task.FromResult(TestResult.Fail("Invoice type should be 'simplified'"));

            return Task.FromResult(TestResult.Pass($"Invoice ID: {invoice.Id}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestDebitNote()
    {
        try
        {
            var data = GetDebitNoteData();
            var mapper = new InvoiceMapper();
            var invoice = mapper.MapToInvoice(data);

            if (invoice.InvoiceType?.InvoiceSubType != "debit")
                return Task.FromResult(TestResult.Fail("Invoice sub-type should be 'debit'"));
            if (invoice.BillingReferences?.Count == 0)
                return Task.FromResult(TestResult.Fail("Debit note should have billing reference"));

            return Task.FromResult(TestResult.Pass($"Invoice ID: {invoice.Id}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestCreditNote()
    {
        try
        {
            var data = GetCreditNoteData();
            var mapper = new InvoiceMapper();
            var invoice = mapper.MapToInvoice(data);

            if (invoice.InvoiceType?.InvoiceSubType != "credit")
                return Task.FromResult(TestResult.Fail("Invoice sub-type should be 'credit'"));

            return Task.FromResult(TestResult.Pass($"Invoice ID: {invoice.Id}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestMultiLineInvoice()
    {
        try
        {
            var data = GetMultiLineInvoiceData();
            var mapper = new InvoiceMapper();
            var invoice = mapper.MapToInvoice(data);

            if (invoice.InvoiceLines?.Count < 2)
                return Task.FromResult(TestResult.Fail("Should have multiple invoice lines"));

            return Task.FromResult(TestResult.Pass($"Lines: {invoice.InvoiceLines?.Count}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestInvoiceValidation()
    {
        try
        {
            var validator = new InvoiceValidator();

            // Test valid data
            var validData = GetSimplifiedInvoiceData();
            var validResult = validator.Validate(validData);
            if (!validResult.IsValid)
                return Task.FromResult(TestResult.Fail("Valid data should pass validation"));

            // Test invalid data
            var invalidData = new Dictionary<string, object>(); // Missing required fields
            var invalidResult = validator.Validate(invalidData);
            if (invalidResult.IsValid)
                return Task.FromResult(TestResult.Fail("Invalid data should fail validation"));

            return Task.FromResult(TestResult.Pass("Validation works correctly"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestAmountValidation()
    {
        try
        {
            var data = GetStandardInvoiceData();
            var result = InvoiceAmountValidator.ValidateMonetaryTotals(data);

            return Task.FromResult(result.IsValid
                ? TestResult.Pass("Amount validation passed")
                : TestResult.Fail($"Amount validation failed: {string.Join(", ", result.Errors)}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestXmlGeneration()
    {
        try
        {
            var data = GetStandardInvoiceData();
            var generator = new InvoiceGenerator("SAR");
            var xml = generator.Generate(data);

            if (string.IsNullOrEmpty(xml))
                return Task.FromResult(TestResult.Fail("Generated XML is empty"));
            if (!xml.Contains("<Invoice"))
                return Task.FromResult(TestResult.Fail("XML should contain Invoice element"));
            if (!xml.Contains("cbc:ID"))
                return Task.FromResult(TestResult.Fail("XML should contain ID element"));

            return Task.FromResult(TestResult.Pass($"XML length: {xml.Length} chars"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    private Task<TestResult> TestXmlNamespaces()
    {
        try
        {
            var data = GetStandardInvoiceData();
            var generator = new InvoiceGenerator("SAR");
            var xml = generator.Generate(data);

            if (!xml.Contains("xmlns:cac"))
                return Task.FromResult(TestResult.Fail("Missing cac namespace"));
            if (!xml.Contains("xmlns:cbc"))
                return Task.FromResult(TestResult.Fail("Missing cbc namespace"));

            return Task.FromResult(TestResult.Pass("All required namespaces present"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(TestResult.Fail(ex.Message));
        }
    }

    #endregion

    #region Test Data

    private Dictionary<string, object> GetStandardInvoiceData()
    {
        return new Dictionary<string, object>
        {
            { "uuid", Guid.NewGuid().ToString() },
            { "id", "INV-STD-001" },
            { "issueDate", DateTime.Now.ToString("yyyy-MM-dd") },
            { "issueTime", DateTime.Now.ToString("HH:mm:ss") },
            { "currencyCode", "SAR" },
            { "taxCurrencyCode", "SAR" },
            { "invoiceType", new Dictionary<string, object>
                {
                    { "invoice", "standard" },
                    { "type", "invoice" }
                }
            },
            { "supplier", GetSupplierData() },
            { "customer", GetCustomerData() },
            { "paymentMeans", new Dictionary<string, object> { { "code", "10" } } },
            { "taxTotal", new Dictionary<string, object> { { "taxAmount", 15.0m } } },
            { "legalMonetaryTotal", new Dictionary<string, object>
                {
                    { "lineExtensionAmount", 100.0m },
                    { "taxExclusiveAmount", 100.0m },
                    { "taxInclusiveAmount", 115.0m },
                    { "payableAmount", 115.0m }
                }
            },
            { "invoiceLines", new List<object> { GetInvoiceLineData() } }
        };
    }

    private Dictionary<string, object> GetSimplifiedInvoiceData()
    {
        var data = GetStandardInvoiceData();
        ((Dictionary<string, object>)data["invoiceType"])["invoice"] = "simplified";
        data.Remove("customer");
        return data;
    }

    private Dictionary<string, object> GetDebitNoteData()
    {
        var data = GetStandardInvoiceData();
        data["id"] = "DBN-001";
        ((Dictionary<string, object>)data["invoiceType"])["type"] = "debit";
        data["billingReferences"] = new List<object>
        {
            new Dictionary<string, object> { { "id", "INV-REF-001" } }
        };
        return data;
    }

    private Dictionary<string, object> GetCreditNoteData()
    {
        var data = GetStandardInvoiceData();
        data["id"] = "CRN-001";
        ((Dictionary<string, object>)data["invoiceType"])["type"] = "credit";
        data["billingReferences"] = new List<object>
        {
            new Dictionary<string, object> { { "id", "INV-REF-001" } }
        };
        return data;
    }

    private Dictionary<string, object> GetMultiLineInvoiceData()
    {
        var data = GetStandardInvoiceData();
        data["invoiceLines"] = new List<object>
        {
            GetInvoiceLineData("1", "Product A", 50.0m),
            GetInvoiceLineData("2", "Product B", 30.0m),
            GetInvoiceLineData("3", "Service C", 20.0m)
        };
        return data;
    }

    private Dictionary<string, object> GetSupplierData()
    {
        return new Dictionary<string, object>
        {
            { "registrationName", "Test Supplier LLC" },
            { "taxId", "399999999900003" },
            { "address", new Dictionary<string, object>
                {
                    { "street", "King Fahd Road" },
                    { "buildingNumber", "1234" },
                    { "city", "Riyadh" },
                    { "postalZone", "12345" },
                    { "country", "SA" }
                }
            }
        };
    }

    private Dictionary<string, object> GetCustomerData()
    {
        return new Dictionary<string, object>
        {
            { "registrationName", "Test Customer LLC" },
            { "taxId", "311111111100003" },
            { "address", new Dictionary<string, object>
                {
                    { "street", "Customer Street" },
                    { "buildingNumber", "5678" },
                    { "city", "Jeddah" },
                    { "postalZone", "23456" },
                    { "country", "SA" }
                }
            }
        };
    }

    private Dictionary<string, object> GetInvoiceLineData(string id = "1", string name = "Test Product", decimal price = 100.0m)
    {
        return new Dictionary<string, object>
        {
            { "id", id },
            { "unitCode", "PCE" },
            { "quantity", 1.0m },
            { "lineExtensionAmount", price },
            { "item", new Dictionary<string, object>
                {
                    { "name", name },
                    { "classifiedTaxCategory", new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                { "percent", 15.0m },
                                { "taxScheme", new Dictionary<string, object> { { "id", "VAT" } } }
                            }
                        }
                    }
                }
            },
            { "price", new Dictionary<string, object>
                {
                    { "amount", price },
                    { "baseQuantity", 1.0m }
                }
            },
            { "taxTotal", new Dictionary<string, object>
                {
                    { "taxAmount", price * 0.15m },
                    { "roundingAmount", price * 1.15m }
                }
            }
        };
    }

    #endregion
}
