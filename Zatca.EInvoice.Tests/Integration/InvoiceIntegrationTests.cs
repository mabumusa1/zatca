using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Xunit;
using Zatca.EInvoice.Mappers;
using Zatca.EInvoice.Xml;

namespace Zatca.EInvoice.Tests.Integration
{
    /// <summary>
    /// End-to-end integration tests for invoice generation flow.
    /// Tests the complete workflow from invoice data to XML generation.
    /// </summary>
    public class InvoiceIntegrationTests
    {
        /// <summary>
        /// Test the complete invoice generation flow:
        /// Create invoice data -> Map to Invoice -> Generate XML -> Verify output.
        /// </summary>
        [Fact]
        public void TestCompleteInvoiceGenerationFlow()
        {
            // Arrange - Create comprehensive invoice data
            var invoiceData = CreateTestInvoiceData();

            // Act - Map to Invoice object
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            // Assert - Verify mapped invoice
            Assert.NotNull(invoice);
            Assert.Equal("INV-2024-001", invoice.Id);
            Assert.Equal("12345678-1234-1234-1234-123456789012", invoice.UUID);
            Assert.Equal("SAR", invoice.InvoiceCurrencyCode);
            Assert.Equal("SAR", invoice.TaxCurrencyCode);

            // Verify supplier
            Assert.NotNull(invoice.AccountingSupplierParty);
            Assert.Equal("Test Company Ltd", invoice.AccountingSupplierParty.LegalEntity?.RegistrationName);

            // Verify customer
            Assert.NotNull(invoice.AccountingCustomerParty);
            Assert.Equal("Customer Company", invoice.AccountingCustomerParty.LegalEntity?.RegistrationName);

            // Verify invoice lines
            Assert.NotNull(invoice.InvoiceLines);
            Assert.Equal(2, invoice.InvoiceLines.Count);
            Assert.Equal("1", invoice.InvoiceLines[0].Id);
            Assert.Equal("Product A", invoice.InvoiceLines[0].Item?.Name);

            // Verify tax total
            Assert.NotNull(invoice.TaxTotal);
            Assert.Equal(75.00m, invoice.TaxTotal.TaxAmount);

            // Verify legal monetary total
            Assert.NotNull(invoice.LegalMonetaryTotal);
            Assert.Equal(500.00m, invoice.LegalMonetaryTotal.LineExtensionAmount);
            Assert.Equal(575.00m, invoice.LegalMonetaryTotal.TaxInclusiveAmount);

            // Act - Generate XML
            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);

            // Assert - Verify XML is well-formed
            Assert.NotNull(xmlOutput);
            Assert.NotEmpty(xmlOutput);

            var doc = XDocument.Parse(xmlOutput);
            Assert.NotNull(doc.Root);

            // Verify XML contains key elements
            var ns = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            var cbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            var cacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            Assert.Equal("Invoice", doc.Root.Name.LocalName);

            // Verify basic invoice fields
            var profileId = doc.Root.Element(XName.Get("ProfileID", cbcNs));
            Assert.NotNull(profileId);

            var id = doc.Root.Element(XName.Get("ID", cbcNs));
            Assert.NotNull(id);
            Assert.Equal("INV-2024-001", id.Value);

            var uuid = doc.Root.Element(XName.Get("UUID", cbcNs));
            Assert.NotNull(uuid);
            Assert.Equal("12345678-1234-1234-1234-123456789012", uuid.Value);

            var issueDate = doc.Root.Element(XName.Get("IssueDate", cbcNs));
            Assert.NotNull(issueDate);

            var issueTime = doc.Root.Element(XName.Get("IssueTime", cbcNs));
            Assert.NotNull(issueTime);

            // Verify supplier party
            var supplierParty = doc.Root.Element(XName.Get("AccountingSupplierParty", cacNs));
            Assert.NotNull(supplierParty);

            var supplierPartyElement = supplierParty.Element(XName.Get("Party", cacNs));
            Assert.NotNull(supplierPartyElement);

            var supplierLegalEntity = supplierPartyElement.Element(XName.Get("PartyLegalEntity", cacNs));
            Assert.NotNull(supplierLegalEntity);

            var supplierName = supplierLegalEntity.Element(XName.Get("RegistrationName", cbcNs));
            Assert.NotNull(supplierName);
            Assert.Equal("Test Company Ltd", supplierName.Value);

            // Verify customer party
            var customerParty = doc.Root.Element(XName.Get("AccountingCustomerParty", cacNs));
            Assert.NotNull(customerParty);

            // Verify tax total
            var taxTotalElements = doc.Root.Elements(XName.Get("TaxTotal", cacNs)).ToList();
            Assert.NotEmpty(taxTotalElements);

            var taxAmount = taxTotalElements[0].Element(XName.Get("TaxAmount", cbcNs));
            Assert.NotNull(taxAmount);
            Assert.Equal("75.00", taxAmount.Value);
            Assert.Equal("SAR", taxAmount.Attribute("currencyID")?.Value);

            // Verify legal monetary total
            var legalMonetaryTotal = doc.Root.Element(XName.Get("LegalMonetaryTotal", cacNs));
            Assert.NotNull(legalMonetaryTotal);

            var lineExtensionAmount = legalMonetaryTotal.Element(XName.Get("LineExtensionAmount", cbcNs));
            Assert.NotNull(lineExtensionAmount);
            Assert.Equal("500.00", lineExtensionAmount.Value);

            var taxInclusiveAmount = legalMonetaryTotal.Element(XName.Get("TaxInclusiveAmount", cbcNs));
            Assert.NotNull(taxInclusiveAmount);
            Assert.Equal("575.00", taxInclusiveAmount.Value);

            // Verify invoice lines
            var invoiceLines = doc.Root.Elements(XName.Get("InvoiceLine", cacNs)).ToList();
            Assert.Equal(2, invoiceLines.Count);

            var firstLineId = invoiceLines[0].Element(XName.Get("ID", cbcNs));
            Assert.NotNull(firstLineId);
            Assert.Equal("1", firstLineId.Value);

            var item = invoiceLines[0].Element(XName.Get("Item", cacNs));
            Assert.NotNull(item);

            var itemName = item.Element(XName.Get("Name", cbcNs));
            Assert.NotNull(itemName);
            Assert.Equal("Product A", itemName.Value);
        }

        /// <summary>
        /// Test invoice generation with minimal required fields.
        /// </summary>
        [Fact]
        public void TestMinimalInvoiceGeneration()
        {
            // Arrange - Minimal invoice data
            var invoiceData = new Dictionary<string, object>
            {
                ["uuid"] = Guid.NewGuid().ToString(),
                ["id"] = "MIN-001",
                ["issueDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
                ["issueTime"] = DateTime.Now.ToString("HH:mm:ss"),
                ["invoiceType"] = new Dictionary<string, object>
                {
                    ["invoice"] = "simplified",
                    ["type"] = "388"
                },
                ["currencyCode"] = "SAR",
                ["supplier"] = new Dictionary<string, object>
                {
                    ["registrationName"] = "Minimal Supplier",
                    ["taxId"] = "300000000000003",
                    ["address"] = new Dictionary<string, object>
                    {
                        ["street"] = "Main St",
                        ["city"] = "Riyadh",
                        ["country"] = "SA"
                    }
                },
                ["customer"] = new Dictionary<string, object>
                {
                    ["registrationName"] = "Minimal Customer",
                    ["address"] = new Dictionary<string, object>
                    {
                        ["city"] = "Jeddah",
                        ["country"] = "SA"
                    }
                },
                ["taxTotal"] = new Dictionary<string, object>
                {
                    ["taxAmount"] = 15.00m
                },
                ["legalMonetaryTotal"] = new Dictionary<string, object>
                {
                    ["lineExtensionAmount"] = 100.00m,
                    ["taxExclusiveAmount"] = 100.00m,
                    ["taxInclusiveAmount"] = 115.00m,
                    ["payableAmount"] = 115.00m
                },
                ["invoiceLines"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "1",
                        ["quantity"] = 1.0m,
                        ["unitCode"] = "PCE",
                        ["lineExtensionAmount"] = 100.00m,
                        ["item"] = new Dictionary<string, object>
                        {
                            ["name"] = "Test Item",
                            ["classifiedTaxCategory"] = new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    ["id"] = "S",
                                    ["percent"] = 15.00m,
                                    ["taxScheme"] = new Dictionary<string, object>
                                    {
                                        ["id"] = "VAT"
                                    }
                                }
                            }
                        },
                        ["price"] = new Dictionary<string, object>
                        {
                            ["amount"] = 100.00m
                        }
                    }
                }
            };

            // Act
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);

            // Assert
            Assert.NotNull(invoice);
            Assert.NotNull(xmlOutput);

            var doc = XDocument.Parse(xmlOutput);
            Assert.NotNull(doc.Root);
            Assert.Equal("Invoice", doc.Root.Name.LocalName);
        }

        /// <summary>
        /// Test invoice generation with billing references.
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationWithBillingReferences()
        {
            // Arrange
            var invoiceData = CreateTestInvoiceData();
            invoiceData["billingReferences"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["id"] = "INV-2023-999",
                    ["invoiceDocumentReference"] = new Dictionary<string, object>
                    {
                        ["id"] = "INV-2023-999"
                    }
                }
            };

            // Act
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);

            // Assert
            Assert.NotNull(invoice);
            Assert.NotNull(invoice.BillingReferences);
            Assert.Single(invoice.BillingReferences);

            var doc = XDocument.Parse(xmlOutput);
            var ns = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var billingRef = doc.Root?.Element(XName.Get("BillingReference", ns));
            Assert.NotNull(billingRef);
        }

        /// <summary>
        /// Test invoice generation with allowance charges.
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationWithAllowanceCharges()
        {
            // Arrange
            var invoiceData = CreateTestInvoiceData();
            invoiceData["allowanceCharges"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["isCharge"] = false,
                    ["reason"] = "Discount",
                    ["amount"] = 50.00m,
                    ["taxCategories"] = new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            ["percent"] = 15.00m,
                            ["taxScheme"] = new Dictionary<string, object>
                            {
                                ["id"] = "VAT"
                            }
                        }
                    }
                }
            };

            // Act
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            // Assert
            Assert.NotNull(invoice);
            Assert.NotNull(invoice.AllowanceCharges);
            Assert.Single(invoice.AllowanceCharges);
            Assert.False(invoice.AllowanceCharges[0].ChargeIndicator);
            Assert.Equal(50.00m, invoice.AllowanceCharges[0].Amount);
        }

        /// <summary>
        /// Test invoice generation with multiple currency scenarios.
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationWithDifferentCurrencies()
        {
            // Arrange
            var currencyPairs = new[] { "SAR", "USD", "EUR", "GBP" };

            foreach (var currency in currencyPairs)
            {
                var invoiceData = CreateTestInvoiceData();
                invoiceData["currencyCode"] = currency;
                invoiceData["taxCurrencyCode"] = currency;

                // Act
                var invoiceGenerator = new InvoiceGenerator(currency);
                var xmlOutput = invoiceGenerator.Generate(invoiceData);

                // Assert
                Assert.NotNull(xmlOutput);
                var doc = XDocument.Parse(xmlOutput);
                var cbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

                var currencyCode = doc.Root?.Element(XName.Get("DocumentCurrencyCode", cbcNs));
                Assert.NotNull(currencyCode);
                Assert.Equal(currency, currencyCode.Value);
            }
        }

        /// <summary>
        /// Test invoice generation with zero-rated and exempt tax scenarios.
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationWithTaxExemptions()
        {
            // Arrange
            var invoiceData = CreateTestInvoiceData();
            invoiceData["taxTotal"] = new Dictionary<string, object>
            {
                ["taxAmount"] = 0.00m,
                ["subTotals"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["taxableAmount"] = 500.00m,
                        ["taxAmount"] = 0.00m,
                        ["taxCategory"] = new Dictionary<string, object>
                        {
                            ["percent"] = 0.00m,
                            ["reasonCode"] = "VATEX-SA-32",
                            ["reason"] = "Export",
                            ["taxScheme"] = new Dictionary<string, object>
                            {
                                ["id"] = "VAT"
                            }
                        }
                    }
                }
            };

            // Act
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);

            // Assert
            Assert.NotNull(invoice);
            Assert.NotNull(invoice.TaxTotal);
            Assert.Equal(0.00m, invoice.TaxTotal.TaxAmount);

            var doc = XDocument.Parse(xmlOutput);
            Assert.NotNull(doc.Root);
        }

        /// <summary>
        /// Test invoice generation with prepaid amount scenarios.
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationWithPrepaidAmount()
        {
            // Arrange
            var invoiceData = CreateTestInvoiceData();
            invoiceData["legalMonetaryTotal"] = new Dictionary<string, object>
            {
                ["lineExtensionAmount"] = 500.00m,
                ["taxExclusiveAmount"] = 500.00m,
                ["taxInclusiveAmount"] = 575.00m,
                ["prepaidAmount"] = 100.00m,
                ["payableAmount"] = 475.00m,
                ["allowanceTotalAmount"] = 0.00m
            };

            // Act
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            // Assert
            Assert.NotNull(invoice);
            Assert.NotNull(invoice.LegalMonetaryTotal);
            Assert.Equal(100.00m, invoice.LegalMonetaryTotal.PrepaidAmount);
            Assert.Equal(475.00m, invoice.LegalMonetaryTotal.PayableAmount);
        }

        /// <summary>
        /// Test invoice generation with delivery information.
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationWithDeliveryInfo()
        {
            // Arrange
            var invoiceData = CreateTestInvoiceData();
            invoiceData["delivery"] = new Dictionary<string, object>
            {
                ["actualDeliveryDate"] = "2024-01-20",
                ["latestDeliveryDate"] = "2024-01-25"
            };

            // Act
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);

            // Assert
            Assert.NotNull(invoice);
            Assert.NotNull(invoice.Delivery);
            Assert.NotNull(invoice.Delivery.ActualDeliveryDate);
            Assert.NotNull(invoice.Delivery.LatestDeliveryDate);
            Assert.Equal(new DateOnly(2024, 1, 20), invoice.Delivery.ActualDeliveryDate);
            Assert.Equal(new DateOnly(2024, 1, 25), invoice.Delivery.LatestDeliveryDate);

            var doc = XDocument.Parse(xmlOutput);
            var cacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var delivery = doc.Root?.Element(XName.Get("Delivery", cacNs));
            Assert.NotNull(delivery);
        }

        /// <summary>
        /// Test invoice generation with large number of invoice lines (performance test).
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationWithManyInvoiceLines()
        {
            // Arrange
            var invoiceData = CreateTestInvoiceData();
            var invoiceLines = new List<object>();

            for (int i = 1; i <= 50; i++)
            {
                invoiceLines.Add(new Dictionary<string, object>
                {
                    ["id"] = i.ToString(),
                    ["quantity"] = 1.0m,
                    ["unitCode"] = "PCE",
                    ["lineExtensionAmount"] = 10.00m,
                    ["item"] = new Dictionary<string, object>
                    {
                        ["name"] = $"Product {i}",
                        ["classifiedTaxCategory"] = new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                ["id"] = "S",
                                ["percent"] = 15.00m,
                                ["taxScheme"] = new Dictionary<string, object>
                                {
                                    ["id"] = "VAT"
                                }
                            }
                        }
                    },
                    ["price"] = new Dictionary<string, object>
                    {
                        ["amount"] = 10.00m
                    }
                });
            }

            invoiceData["invoiceLines"] = invoiceLines;

            // Act
            var startTime = DateTime.Now;
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);
            var endTime = DateTime.Now;

            // Assert
            Assert.NotNull(invoice);
            Assert.NotNull(invoice.InvoiceLines);
            Assert.Equal(50, invoice.InvoiceLines.Count);

            var doc = XDocument.Parse(xmlOutput);
            var cacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var lines = doc.Root?.Elements(XName.Get("InvoiceLine", cacNs)).ToList();
            Assert.Equal(50, lines?.Count);

            // Performance check - should complete in reasonable time (< 5 seconds)
            var duration = endTime - startTime;
            Assert.True(duration.TotalSeconds < 5, $"Invoice generation took too long: {duration.TotalSeconds} seconds");
        }

        /// <summary>
        /// Test complete invoice flow with validation errors.
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationWithValidationErrors()
        {
            // Arrange - Invalid invoice data (missing required fields)
            var invalidInvoiceData = new Dictionary<string, object>
            {
                ["id"] = "INV-001"
                // Missing uuid, issueDate, issueTime, supplier, customer, etc.
            };

            // Act & Assert - Model validation throws ArgumentException for invalid/empty values
            var invoiceMapper = new InvoiceMapper();
            Assert.Throws<ArgumentException>(() => invoiceMapper.MapToInvoice(invalidInvoiceData));
        }

        /// <summary>
        /// Test invoice generation with all invoice types.
        /// </summary>
        [Theory]
        [InlineData("standard", "388")]
        [InlineData("simplified", "388")]
        [InlineData("standard", "381")] // Credit note
        [InlineData("standard", "383")] // Debit note
        public void TestInvoiceGenerationWithDifferentInvoiceTypes(string invoiceCategory, string typeCode)
        {
            // Arrange
            var invoiceData = CreateTestInvoiceData();
            invoiceData["invoiceType"] = new Dictionary<string, object>
            {
                ["invoice"] = invoiceCategory,
                ["type"] = typeCode
            };

            // Act
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);

            // Assert
            Assert.NotNull(invoice);
            Assert.Equal(invoiceCategory, invoice.InvoiceType.Invoice);

            var doc = XDocument.Parse(xmlOutput);
            var cbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            var invoiceTypeCode = doc.Root?.Element(XName.Get("InvoiceTypeCode", cbcNs));
            Assert.NotNull(invoiceTypeCode);
            Assert.Equal(typeCode, invoiceTypeCode.Value);
            Assert.Equal(invoiceCategory, invoiceTypeCode.Attribute("name")?.Value);
        }

        /// <summary>
        /// Test invoice XML namespace declarations.
        /// </summary>
        [Fact]
        public void TestInvoiceXmlNamespaceDeclarations()
        {
            // Arrange
            var invoiceData = CreateTestInvoiceData();

            // Act
            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);

            // Assert
            var doc = XDocument.Parse(xmlOutput);
            Assert.NotNull(doc.Root);

            // Verify all required namespaces are declared
            var invoiceNs = doc.Root.GetNamespaceOfPrefix("Invoice");
            var cacNs = doc.Root.GetNamespaceOfPrefix("cac");
            var cbcNs = doc.Root.GetNamespaceOfPrefix("cbc");
            var extNs = doc.Root.GetNamespaceOfPrefix("ext");

            Assert.NotNull(cacNs);
            Assert.NotNull(cbcNs);
            Assert.NotNull(extNs);

            Assert.Equal("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2", cacNs.NamespaceName);
            Assert.Equal("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2", cbcNs.NamespaceName);
        }

        /// <summary>
        /// Test concurrent invoice generation (thread safety).
        /// </summary>
        [Fact]
        public async Task TestConcurrentInvoiceGeneration()
        {
            // Arrange
            var tasks = new List<Task<string>>();
            var numberOfConcurrentInvoices = 10;

            // Act
            for (int i = 0; i < numberOfConcurrentInvoices; i++)
            {
                var invoiceNumber = i;
                var task = Task.Run(() =>
                {
                    var invoiceData = CreateTestInvoiceData();
                    invoiceData["id"] = $"INV-{invoiceNumber:000}";
                    invoiceData["uuid"] = Guid.NewGuid().ToString();

                    var invoiceMapper = new InvoiceMapper();
                    var invoice = invoiceMapper.MapToInvoice(invoiceData);

                    var invoiceGenerator = new InvoiceGenerator();
                    return invoiceGenerator.Generate(invoiceData);
                });

                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.NotNull(result);
                Assert.NotEmpty(result);

                var doc = XDocument.Parse(result);
                Assert.NotNull(doc.Root);
            }

            // All invoices should be unique
            var invoiceIds = results.Select(r =>
            {
                var doc = XDocument.Parse(r);
                var cbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                return doc.Root?.Element(XName.Get("ID", cbcNs))?.Value;
            }).Distinct().ToList();

            Assert.Equal(numberOfConcurrentInvoices, invoiceIds.Count);
        }

        /// <summary>
        /// Test invoice generation with Arabic characters.
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationWithArabicCharacters()
        {
            // Arrange
            var invoiceData = CreateTestInvoiceData();
            invoiceData["note"] = "فاتورة اختبار - Test Invoice";

            var supplier = (Dictionary<string, object>)invoiceData["supplier"];
            supplier["registrationName"] = "شركة الاختبار المحدودة";

            var customer = (Dictionary<string, object>)invoiceData["customer"];
            customer["registrationName"] = "العميل التجريبي";

            // Act
            var invoiceMapper = new InvoiceMapper();
            var invoice = invoiceMapper.MapToInvoice(invoiceData);

            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);

            // Assert
            Assert.NotNull(invoice);
            Assert.Contains("فاتورة", invoice.Note);

            var doc = XDocument.Parse(xmlOutput);
            Assert.NotNull(doc.Root);

            // Verify Arabic text is preserved in XML
            Assert.Contains("فاتورة", xmlOutput);
            Assert.Contains("شركة", xmlOutput);
        }

        /// <summary>
        /// Test invoice generation preserves decimal precision.
        /// </summary>
        [Fact]
        public void TestInvoiceGenerationDecimalPrecision()
        {
            // Arrange - Use 2 decimal places as per ZATCA requirements
            var invoiceData = CreateTestInvoiceData();
            invoiceData["legalMonetaryTotal"] = new Dictionary<string, object>
            {
                ["lineExtensionAmount"] = 123.46m,
                ["taxExclusiveAmount"] = 123.46m,
                ["taxInclusiveAmount"] = 141.98m,
                ["payableAmount"] = 141.98m
            };

            // Act
            var invoiceGenerator = new InvoiceGenerator();
            var xmlOutput = invoiceGenerator.Generate(invoiceData);

            // Assert
            var doc = XDocument.Parse(xmlOutput);
            var cacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var cbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

            var legalMonetaryTotal = doc.Root?.Element(XName.Get("LegalMonetaryTotal", cacNs));
            Assert.NotNull(legalMonetaryTotal);

            var lineExtensionAmount = legalMonetaryTotal.Element(XName.Get("LineExtensionAmount", cbcNs));
            Assert.NotNull(lineExtensionAmount);

            // Verify precision - ZATCA uses 2 decimal places for currency amounts
            var amount = decimal.Parse(lineExtensionAmount.Value);
            Assert.Equal(123.46m, amount);
        }

        /// <summary>
        /// Creates comprehensive test invoice data for testing.
        /// </summary>
        private Dictionary<string, object> CreateTestInvoiceData()
        {
            return new Dictionary<string, object>
            {
                ["uuid"] = "12345678-1234-1234-1234-123456789012",
                ["id"] = "INV-2024-001",
                ["issueDate"] = "2024-01-15",
                ["issueTime"] = "10:30:00",
                ["invoiceType"] = new Dictionary<string, object>
                {
                    ["invoice"] = "standard",
                    ["type"] = "388",
                    ["isThirdParty"] = false,
                    ["isNominal"] = false,
                    ["isExport"] = false,
                    ["isSummary"] = false,
                    ["isSelfBilled"] = false
                },
                ["currencyCode"] = "SAR",
                ["taxCurrencyCode"] = "SAR",
                ["languageID"] = "en",
                ["note"] = "Test invoice for integration testing",
                ["supplier"] = new Dictionary<string, object>
                {
                    ["partyIdentification"] = "CRN12345",
                    ["partyIdentificationId"] = "CRN",
                    ["registrationName"] = "Test Company Ltd",
                    ["taxId"] = "300000000000003",
                    ["address"] = new Dictionary<string, object>
                    {
                        ["street"] = "King Fahd Road",
                        ["additionalStreet"] = "Building 123",
                        ["buildingNumber"] = "1234",
                        ["city"] = "Riyadh",
                        ["postalZone"] = "12345",
                        ["countrySubentity"] = "Riyadh Province",
                        ["country"] = "SA"
                    },
                    ["taxScheme"] = new Dictionary<string, object>
                    {
                        ["id"] = "VAT"
                    }
                },
                ["customer"] = new Dictionary<string, object>
                {
                    ["registrationName"] = "Customer Company",
                    ["taxId"] = "399999999900003",
                    ["address"] = new Dictionary<string, object>
                    {
                        ["street"] = "Al Madinah Road",
                        ["city"] = "Jeddah",
                        ["postalZone"] = "54321",
                        ["country"] = "SA"
                    },
                    ["taxScheme"] = new Dictionary<string, object>
                    {
                        ["id"] = "VAT"
                    }
                },
                ["paymentMeans"] = new Dictionary<string, object>
                {
                    ["code"] = "10",
                    ["instructionNote"] = "Payment by cash"
                },
                ["taxTotal"] = new Dictionary<string, object>
                {
                    ["taxAmount"] = 75.00m,
                    ["subTotals"] = new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            ["taxableAmount"] = 500.00m,
                            ["taxAmount"] = 75.00m,
                            ["taxCategory"] = new Dictionary<string, object>
                            {
                                ["percent"] = 15.00m,
                                ["taxScheme"] = new Dictionary<string, object>
                                {
                                    ["id"] = "VAT"
                                }
                            }
                        }
                    }
                },
                ["legalMonetaryTotal"] = new Dictionary<string, object>
                {
                    ["lineExtensionAmount"] = 500.00m,
                    ["taxExclusiveAmount"] = 500.00m,
                    ["taxInclusiveAmount"] = 575.00m,
                    ["payableAmount"] = 575.00m,
                    ["prepaidAmount"] = 0.00m,
                    ["allowanceTotalAmount"] = 0.00m
                },
                ["invoiceLines"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "1",
                        ["quantity"] = 5.0m,
                        ["unitCode"] = "PCE",
                        ["lineExtensionAmount"] = 300.00m,
                        ["taxTotal"] = new Dictionary<string, object>
                        {
                            ["taxAmount"] = 45.00m,
                            ["roundingAmount"] = 0.00m
                        },
                        ["item"] = new Dictionary<string, object>
                        {
                            ["name"] = "Product A",
                            ["classifiedTaxCategory"] = new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    ["id"] = "S",
                                    ["percent"] = 15.00m,
                                    ["taxScheme"] = new Dictionary<string, object>
                                    {
                                        ["id"] = "VAT"
                                    }
                                }
                            }
                        },
                        ["price"] = new Dictionary<string, object>
                        {
                            ["amount"] = 60.00m,
                            ["baseQuantity"] = 1.0m,
                            ["baseQuantityUnitCode"] = "PCE"
                        }
                    },
                    new Dictionary<string, object>
                    {
                        ["id"] = "2",
                        ["quantity"] = 2.0m,
                        ["unitCode"] = "PCE",
                        ["lineExtensionAmount"] = 200.00m,
                        ["taxTotal"] = new Dictionary<string, object>
                        {
                            ["taxAmount"] = 30.00m,
                            ["roundingAmount"] = 0.00m
                        },
                        ["item"] = new Dictionary<string, object>
                        {
                            ["name"] = "Product B",
                            ["classifiedTaxCategory"] = new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    ["id"] = "S",
                                    ["percent"] = 15.00m,
                                    ["taxScheme"] = new Dictionary<string, object>
                                    {
                                        ["id"] = "VAT"
                                    }
                                }
                            }
                        },
                        ["price"] = new Dictionary<string, object>
                        {
                            ["amount"] = 100.00m,
                            ["baseQuantity"] = 1.0m,
                            ["baseQuantityUnitCode"] = "PCE"
                        }
                    }
                }
            };
        }
    }
}
