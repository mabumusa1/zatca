using System.Reflection;
using System.Text.Json;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;

namespace Zatca.EInvoice.Pdf;

/// <summary>
/// Generates PDF invoices for ZATCA e-invoicing compliance.
/// Supports Standard Invoices, Credit Notes, and Debit Notes with Arabic/English bilingual layout.
/// </summary>
public class InvoicePdfGenerator
{
    private readonly InvoicePdfData _data;
    private readonly byte[]? _qrCodeImage;
    private static readonly string ArabicFontFamily = "Noto Sans Arabic";

    static InvoicePdfGenerator()
    {
        // Set QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;

        // Register Arabic font from embedded resources
        RegisterEmbeddedFonts();
    }

    private static void RegisterEmbeddedFonts()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();

        foreach (var resourceName in resourceNames.Where(n => n.EndsWith(".ttf")))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                FontManager.RegisterFont(stream);
            }
        }
    }

    /// <summary>
    /// Creates a new invoice PDF generator from JSON invoice data.
    /// </summary>
    /// <param name="jsonData">JSON string containing invoice data</param>
    /// <param name="qrCodeBase64">Base64-encoded QR code TLV data</param>
    public InvoicePdfGenerator(string jsonData, string? qrCodeBase64 = null)
    {
        _data = JsonSerializer.Deserialize<InvoicePdfData>(jsonData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new ArgumentException("Invalid JSON data", nameof(jsonData));

        if (!string.IsNullOrEmpty(qrCodeBase64))
        {
            _qrCodeImage = GenerateQrCodeImage(qrCodeBase64);
        }
    }

    /// <summary>
    /// Creates a new invoice PDF generator from structured data.
    /// </summary>
    public InvoicePdfGenerator(InvoicePdfData data, string? qrCodeBase64 = null)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));

        if (!string.IsNullOrEmpty(qrCodeBase64))
        {
            _qrCodeImage = GenerateQrCodeImage(qrCodeBase64);
        }
    }

    /// <summary>
    /// Generates the PDF and saves it to a file.
    /// </summary>
    /// <param name="outputPath">Path to save the PDF file</param>
    /// <param name="xmlContent">Optional XML content to embed in the PDF</param>
    public void GeneratePdf(string outputPath, string? xmlContent = null)
    {
        var document = CreateDocument(xmlContent);
        document.GeneratePdf(outputPath);
    }

    /// <summary>
    /// Generates the PDF and returns it as a byte array.
    /// </summary>
    /// <param name="xmlContent">Optional XML content to embed in the PDF</param>
    public byte[] GeneratePdfBytes(string? xmlContent = null)
    {
        var document = CreateDocument(xmlContent);
        return document.GeneratePdf();
    }

    private Document CreateDocument(string? xmlContent)
    {
        if (IsSimplifiedInvoice())
        {
            return CreateReceiptDocument(xmlContent);
        }

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(20);
                page.MarginHorizontal(25);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(ArabicFontFamily));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        });
    }

    /// <summary>
    /// Creates a POS receipt-style document for simplified invoices.
    /// Uses 80mm thermal printer width format.
    /// </summary>
    private Document CreateReceiptDocument(string? xmlContent)
    {
        // Standard thermal receipt width: 80mm (approximately 226 points)
        const float receiptWidth = 226f;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(receiptWidth, PageSizes.A4.Height);
                page.MarginVertical(10);
                page.MarginHorizontal(8);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily(ArabicFontFamily));

                page.Content().Element(ComposeReceiptContent);
            });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        var invoiceTitle = GetInvoiceTitle();

        container.Column(column =>
        {
            // Title row
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(invoiceTitle.Arabic)
                        .FontSize(16).Bold().FontColor(Colors.Green.Darken3);
                    col.Item().Text(invoiceTitle.English)
                        .FontSize(12).Bold().FontColor(Colors.Green.Darken3);
                });

                if (_qrCodeImage != null)
                {
                    row.ConstantItem(80).Image(_qrCodeImage);
                }
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Green.Darken3);

            // Invoice details row
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("رقم الفاتورة / Invoice No: ").Bold();
                        text.Span(_data.Id ?? "");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("التاريخ / Date: ").Bold();
                        text.Span($"{_data.IssueDate} {_data.IssueTime}");
                    });
                    if (!string.IsNullOrEmpty(_data.Uuid))
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("UUID: ").Bold();
                            text.Span(_data.Uuid).FontSize(7);
                        });
                    }
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("العملة / Currency: ").Bold();
                        text.Span(_data.CurrencyCode ?? "SAR");
                    });
                    if (_data.InvoiceType != null)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("النوع / Type: ").Bold();
                            text.Span(_data.InvoiceType.TypeCode?.ToString() ?? "");
                        });
                    }
                });
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Seller and Buyer info
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => ComposePartyInfo(c, _data.Supplier, "البائع / Seller", true));
                row.ConstantItem(20);
                row.RelativeItem().Element(c => ComposePartyInfo(c, _data.Customer, "المشتري / Buyer", false));
            });

            column.Item().PaddingVertical(10);

            // Invoice lines table
            column.Item().Element(ComposeTable);

            column.Item().PaddingVertical(10);

            // Totals
            column.Item().Element(ComposeTotals);

            // Billing Reference section for credit/debit notes (ZATCA format - placed after totals)
            if (_data.BillingReferences?.Any() == true && IsCreditOrDebitNote())
            {
                column.Item().PaddingTop(10).Element(ComposeBillingReference);
            }

            // Notes
            if (!string.IsNullOrEmpty(_data.Note))
            {
                column.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(8).Column(col =>
                {
                    col.Item().Text("ملاحظات / Notes:").Bold();
                    col.Item().Text(_data.Note);
                });
            }
        });
    }

    private void ComposePartyInfo(IContainer container, PartyPdfData? party, string title, bool isSeller)
    {
        var bgColor = isSeller ? Colors.Green.Lighten5 : Colors.Blue.Lighten5;

        container.Background(bgColor).Padding(8).Column(column =>
        {
            column.Item().Text(title).Bold().FontSize(10);
            column.Item().PaddingBottom(3).LineHorizontal(0.5f);

            if (party == null) return;

            column.Item().Text(party.RegistrationName ?? "").Bold();

            if (party.Address != null)
            {
                var addr = party.Address;
                if (!string.IsNullOrEmpty(addr.Street))
                    column.Item().Text($"{addr.Street} {addr.BuildingNumber}".Trim());
                if (!string.IsNullOrEmpty(addr.CitySubdivisionName))
                    column.Item().Text(addr.CitySubdivisionName);
                if (!string.IsNullOrEmpty(addr.City))
                    column.Item().Text($"{addr.City} {addr.PostalZone}".Trim());
                if (!string.IsNullOrEmpty(addr.Country))
                    column.Item().Text(addr.Country);
            }

            if (!string.IsNullOrEmpty(party.TaxId))
            {
                column.Item().PaddingTop(3).Text(text =>
                {
                    text.Span("الرقم الضريبي / VAT: ").Bold().FontSize(8);
                    text.Span(party.TaxId).FontSize(8);
                });
            }
        });
    }

    private void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);  // #
                columns.RelativeColumn(3);   // Description
                columns.RelativeColumn(1);   // Qty
                columns.RelativeColumn(1);   // Unit Price
                columns.RelativeColumn(1);   // Discount
                columns.RelativeColumn(1);   // VAT %
                columns.RelativeColumn(1);   // VAT Amount
                columns.RelativeColumn(1.2f); // Total
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).AlignCenter().Text("#").Bold();
                header.Cell().Element(CellStyle).Text("الوصف / Description").Bold();
                header.Cell().Element(CellStyle).AlignCenter().Text("الكمية\nQty").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("السعر\nPrice").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("الخصم\nDisc.").Bold();
                header.Cell().Element(CellStyle).AlignCenter().Text("الضريبة%\nVAT%").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("الضريبة\nVAT").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("الإجمالي\nTotal").Bold();

                static IContainer CellStyle(IContainer c) =>
                    c.Background(Colors.Green.Darken3).Padding(4).DefaultTextStyle(x => x.FontColor(Colors.White).FontSize(8));
            });

            // Body
            var lineNumber = 1;
            foreach (var line in _data.InvoiceLines ?? Enumerable.Empty<InvoiceLinePdfData>())
            {
                var bgColor = lineNumber % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                table.Cell().Element(c => BodyCellStyle(c, bgColor)).AlignCenter().Text(lineNumber.ToString());
                table.Cell().Element(c => BodyCellStyle(c, bgColor)).Text(line.Item?.Name ?? "");
                table.Cell().Element(c => BodyCellStyle(c, bgColor)).AlignCenter().Text(line.Quantity?.ToString("N2") ?? "0");
                table.Cell().Element(c => BodyCellStyle(c, bgColor)).AlignRight().Text(line.Price?.Amount?.ToString("N2") ?? "0");
                table.Cell().Element(c => BodyCellStyle(c, bgColor)).AlignRight().Text(GetLineDiscount(line).ToString("N2"));
                table.Cell().Element(c => BodyCellStyle(c, bgColor)).AlignCenter().Text($"{line.Item?.ClassifiedTaxCategory?.FirstOrDefault()?.Percent ?? 15}%");
                table.Cell().Element(c => BodyCellStyle(c, bgColor)).AlignRight().Text(line.TaxTotal?.TaxAmount?.ToString("N2") ?? "0");
                table.Cell().Element(c => BodyCellStyle(c, bgColor)).AlignRight().Text(line.LineExtensionAmount?.ToString("N2") ?? "0");

                lineNumber++;
            }

            static IContainer BodyCellStyle(IContainer c, string bgColor) =>
                c.Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).DefaultTextStyle(x => x.FontSize(8));
        });
    }

    private void ComposeBillingReference(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Darken1).Column(column =>
        {
            // Header row
            column.Item().Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
            {
                row.RelativeItem().Text("Reference").Bold().FontSize(10);
                row.RelativeItem().AlignRight().Text("المرجع").Bold().FontSize(10);
            });

            // Reference description
            column.Item().Padding(8).Row(row =>
            {
                row.RelativeItem().Text("A reference to the original invoice(s) that the credit/debit note is related to")
                    .FontSize(8);
                row.RelativeItem().AlignRight().Text("الإشارة إلى الفاتورة الصادرة عن التوريد المبدئي الذي يتعلق به الإشعار الدائن أو المدين")
                    .FontSize(8);
            });

            // Original invoice reference(s)
            foreach (var reference in _data.BillingReferences!)
            {
                column.Item().BorderTop(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(8).Row(row =>
                {
                    row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(5)
                        .Text(reference.InvoiceDocumentReference?.Id ?? "").FontSize(9);
                    row.RelativeItem().AlignRight().Background(Colors.Grey.Lighten4).Padding(5)
                        .Text(reference.InvoiceDocumentReference?.Id ?? "").FontSize(9);
                });
            }

            // Reason for issuance (if note contains reason)
            if (!string.IsNullOrEmpty(_data.Note))
            {
                column.Item().BorderTop(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(reasonCol =>
                {
                    reasonCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Reason for issuance of credit/debit note as per the VAT implementing Regulation")
                            .FontSize(8);
                        row.RelativeItem().AlignRight().Text("سبب إصدار الإشعار من الأسباب التالية")
                            .FontSize(8);
                    });

                    reasonCol.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(5)
                            .Text(_data.Note).FontSize(9);
                    });
                });
            }
        });
    }

    private void ComposeTotals(IContainer container)
    {
        container.AlignRight().Width(250).Column(column =>
        {
            var totals = _data.LegalMonetaryTotal;
            var taxTotal = _data.TaxTotal;

            AddTotalRow(column, "المجموع الفرعي / Subtotal", totals?.LineExtensionAmount);

            // Allowances/Discounts
            if (_data.AllowanceCharges?.Any() == true)
            {
                foreach (var ac in _data.AllowanceCharges.Where(x => x.ChargeIndicator == "false"))
                {
                    AddTotalRow(column, $"خصم / Discount ({ac.AllowanceChargeReason})", ac.Amount, true);
                }
            }

            AddTotalRow(column, "المبلغ الخاضع للضريبة / Taxable Amount", totals?.TaxExclusiveAmount);

            // Tax subtotals
            if (taxTotal?.SubTotals?.Any() == true)
            {
                foreach (var sub in taxTotal.SubTotals)
                {
                    var rate = sub.TaxCategory?.Percent ?? 15;
                    AddTotalRow(column, $"ضريبة القيمة المضافة / VAT ({rate}%)", sub.TaxAmount);
                }
            }
            else
            {
                AddTotalRow(column, "ضريبة القيمة المضافة / VAT", taxTotal?.TaxAmount);
            }

            column.Item().PaddingVertical(3).LineHorizontal(1).LineColor(Colors.Green.Darken3);

            // Grand total
            column.Item().Background(Colors.Green.Darken3).Padding(6).Row(row =>
            {
                row.RelativeItem().Text("الإجمالي / Total").Bold().FontColor(Colors.White);
                row.RelativeItem().AlignRight().Text($"{totals?.TaxInclusiveAmount?.ToString("N2") ?? "0"} {_data.CurrencyCode ?? "SAR"}")
                    .Bold().FontColor(Colors.White);
            });
        });
    }

    private static void AddTotalRow(ColumnDescriptor column, string label, decimal? amount, bool isNegative = false)
    {
        column.Item().PaddingVertical(2).Row(row =>
        {
            row.RelativeItem().Text(label).FontSize(8);
            var displayAmount = isNegative ? $"-{amount?.ToString("N2") ?? "0"}" : amount?.ToString("N2") ?? "0";
            row.RelativeItem().AlignRight().Text(displayAmount).FontSize(8);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("تم إنشاء هذه الفاتورة إلكترونياً وفقاً لمتطلبات هيئة الزكاة والضريبة والجمارك").FontSize(7);
                });
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("This invoice was generated electronically per ZATCA requirements").FontSize(7);
                });
            });

            column.Item().AlignCenter().Text(text =>
            {
                text.CurrentPageNumber().FontSize(8);
                text.Span(" / ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }

    private (string Arabic, string English) GetInvoiceTitle()
    {
        var typeCode = _data.InvoiceType?.TypeCode;
        var typeName = _data.InvoiceType?.Name ?? "";

        // Check if simplified (starts with 02)
        var isSimplified = typeName.StartsWith("02");
        var prefix = isSimplified ? "مبسطة / Simplified " : "";

        return typeCode switch
        {
            381 => ($"إشعار دائن {prefix}", $"{prefix}Credit Note"),
            383 => ($"إشعار مدين {prefix}", $"{prefix}Debit Note"),
            386 => ($"فاتورة دفعة مقدمة {prefix}", $"{prefix}Prepayment Invoice"),
            _ => ($"فاتورة ضريبية {prefix}", $"{prefix}Tax Invoice")
        };
    }

    private bool IsCreditOrDebitNote()
    {
        var typeCode = _data.InvoiceType?.TypeCode;
        return typeCode == 381 || typeCode == 383; // Credit Note or Debit Note
    }

    private bool IsSimplifiedInvoice()
    {
        var typeName = _data.InvoiceType?.Name ?? "";
        return typeName.StartsWith("02"); // Simplified invoices start with 02
    }

    private string GetCreditDebitReferenceTitle()
    {
        var typeCode = _data.InvoiceType?.TypeCode;
        return typeCode switch
        {
            381 => "إشعار دائن - مرجع الفاتورة الأصلية / Credit Note - Original Invoice Reference",
            383 => "إشعار مدين - مرجع الفاتورة الأصلية / Debit Note - Original Invoice Reference",
            _ => "مرجع الفاتورة / Invoice Reference"
        };
    }

    private static decimal GetLineDiscount(InvoiceLinePdfData line)
    {
        return line.AllowanceCharges?
            .Where(x => x.ChargeIndicator == "false")
            .Sum(x => x.Amount ?? 0) ?? 0;
    }

    private static byte[] GenerateQrCodeImage(string base64Data)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(base64Data, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(5);
    }

    #region POS Receipt Layout Methods

    /// <summary>
    /// Main content composer for POS receipt-style layout.
    /// </summary>
    private void ComposeReceiptContent(IContainer container)
    {
        container.Column(column =>
        {
            // Store header
            column.Item().Element(ComposeReceiptHeader);

            // Divider
            column.Item().PaddingVertical(5).Element(ReceiptDivider);

            // Invoice info
            column.Item().Element(ComposeReceiptInvoiceInfo);

            // Divider
            column.Item().PaddingVertical(5).Element(ReceiptDivider);

            // Line items
            column.Item().Element(ComposeReceiptItems);

            // Divider
            column.Item().PaddingVertical(3).Element(ReceiptDivider);

            // Totals
            column.Item().Element(ComposeReceiptTotals);

            // Double divider before QR
            column.Item().PaddingVertical(5).Element(ReceiptDoubleDivider);

            // QR Code
            column.Item().Element(ComposeReceiptQrCode);

            // Footer
            column.Item().PaddingTop(5).Element(ComposeReceiptFooter);
        });
    }

    private void ComposeReceiptHeader(IContainer container)
    {
        container.Column(column =>
        {
            // Store name - large and centered
            var storeName = _data.Supplier?.RegistrationName ?? "";
            var parts = storeName.Split('|');

            if (parts.Length >= 2)
            {
                // Arabic name
                column.Item().AlignCenter().Text(parts[0].Trim())
                    .FontSize(12).Bold().FontColor(Colors.Green.Darken3);
                // English name
                column.Item().AlignCenter().Text(parts[1].Trim())
                    .FontSize(10).Bold().FontColor(Colors.Green.Darken3);
            }
            else
            {
                column.Item().AlignCenter().Text(storeName)
                    .FontSize(11).Bold().FontColor(Colors.Green.Darken3);
            }

            // VAT number
            if (!string.IsNullOrEmpty(_data.Supplier?.TaxId))
            {
                column.Item().PaddingTop(3).AlignCenter().Text(text =>
                {
                    text.Span("الرقم الضريبي: ").FontSize(7);
                    text.Span(_data.Supplier.TaxId).FontSize(7).Bold();
                });
            }

            // Address (compact)
            if (_data.Supplier?.Address != null)
            {
                var addr = _data.Supplier.Address;
                var addressLine = string.Join(" - ", new[] { addr.City, addr.CitySubdivisionName }
                    .Where(x => !string.IsNullOrEmpty(x)));
                if (!string.IsNullOrEmpty(addressLine))
                {
                    column.Item().AlignCenter().Text(addressLine).FontSize(7);
                }
            }

            // Invoice title
            var invoiceTitle = GetInvoiceTitle();
            column.Item().PaddingTop(5).AlignCenter().Background(Colors.Green.Darken3).Padding(4)
                .Text($"{invoiceTitle.Arabic}").FontSize(10).Bold().FontColor(Colors.White);
        });
    }

    private void ComposeReceiptInvoiceInfo(IContainer container)
    {
        container.Column(column =>
        {
            // Invoice number
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("رقم الفاتورة:").FontSize(7);
                row.RelativeItem().AlignRight().Text(_data.Id ?? "").FontSize(7).Bold();
            });

            // Date and time
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("التاريخ:").FontSize(7);
                row.RelativeItem().AlignRight().Text($"{_data.IssueDate} {_data.IssueTime}").FontSize(7);
            });

            // Cashier/Counter (if available from note or could be added)
            if (!string.IsNullOrEmpty(_data.Uuid))
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("UUID:").FontSize(6);
                    row.RelativeItem().AlignRight().Text(_data.Uuid).FontSize(5);
                });
            }
        });
    }

    private void ComposeReceiptItems(IContainer container)
    {
        container.Column(column =>
        {
            // Items header
            column.Item().Row(row =>
            {
                row.RelativeItem(2).Text("الصنف").FontSize(7).Bold();
                row.RelativeItem().AlignCenter().Text("الكمية").FontSize(7).Bold();
                row.RelativeItem().AlignRight().Text("المبلغ").FontSize(7).Bold();
            });

            column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

            // Items
            foreach (var line in _data.InvoiceLines ?? Enumerable.Empty<InvoiceLinePdfData>())
            {
                // Item name on first row
                column.Item().Text(line.Item?.Name ?? "").FontSize(7);

                // Qty x Price = Total on second row
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).Text(text =>
                    {
                        text.Span($"  {line.Quantity?.ToString("N0") ?? "1"} x ").FontSize(6);
                        text.Span($"{line.Price?.Amount?.ToString("N2") ?? "0"}").FontSize(6);
                    });
                    row.RelativeItem().AlignCenter().Text($"{line.Quantity?.ToString("N0") ?? "1"}").FontSize(7);
                    row.RelativeItem().AlignRight().Text($"{line.LineExtensionAmount?.ToString("N2") ?? "0"}").FontSize(7);
                });

                // VAT info
                var vatPercent = line.Item?.ClassifiedTaxCategory?.FirstOrDefault()?.Percent ?? 15;
                var vatAmount = line.TaxTotal?.TaxAmount ?? 0;
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"  ض.ق.م {vatPercent}%: {vatAmount:N2}").FontSize(6).FontColor(Colors.Grey.Darken1);
                });

                column.Item().PaddingVertical(1);
            }
        });
    }

    private void ComposeReceiptTotals(IContainer container)
    {
        var totals = _data.LegalMonetaryTotal;
        var taxTotal = _data.TaxTotal;

        container.Column(column =>
        {
            // Subtotal
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("المجموع الفرعي:").FontSize(7);
                row.RelativeItem().AlignRight().Text($"{totals?.LineExtensionAmount?.ToString("N2") ?? "0"}").FontSize(7);
            });

            // Discounts if any
            if (_data.AllowanceCharges?.Any(x => x.ChargeIndicator == "false" && x.Amount > 0) == true)
            {
                var totalDiscount = _data.AllowanceCharges
                    .Where(x => x.ChargeIndicator == "false")
                    .Sum(x => x.Amount ?? 0);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("الخصم:").FontSize(7);
                    row.RelativeItem().AlignRight().Text($"-{totalDiscount:N2}").FontSize(7);
                });
            }

            // VAT
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("ضريبة القيمة المضافة (15%):").FontSize(7);
                row.RelativeItem().AlignRight().Text($"{taxTotal?.TaxAmount?.ToString("N2") ?? "0"}").FontSize(7);
            });

            // Grand total - highlighted
            column.Item().PaddingTop(3).Background(Colors.Grey.Lighten3).Padding(4).Row(row =>
            {
                row.RelativeItem().Text("الإجمالي:").FontSize(9).Bold();
                row.RelativeItem().AlignRight().Text($"{totals?.TaxInclusiveAmount?.ToString("N2") ?? "0"} {_data.CurrencyCode ?? "SAR"}").FontSize(9).Bold();
            });

            // Payment method
            column.Item().PaddingTop(3).AlignCenter().Text("طريقة الدفع: نقداً").FontSize(6);
        });
    }

    private void ComposeReceiptQrCode(IContainer container)
    {
        if (_qrCodeImage == null) return;

        container.Column(column =>
        {
            column.Item().AlignCenter().Text("امسح للتحقق").FontSize(7);
            column.Item().AlignCenter().Text("Scan to Verify").FontSize(6);
            column.Item().PaddingTop(3).AlignCenter().Width(100).Image(_qrCodeImage);
        });
    }

    private void ComposeReceiptFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Element(ReceiptDivider);

            column.Item().PaddingTop(3).AlignCenter()
                .Text("شكراً لزيارتكم").FontSize(8).Bold();
            column.Item().AlignCenter()
                .Text("Thank you for visiting").FontSize(7);

            column.Item().PaddingTop(5).AlignCenter()
                .Text("فاتورة ضريبية مبسطة").FontSize(6);
            column.Item().AlignCenter()
                .Text("Simplified Tax Invoice").FontSize(5);

            column.Item().PaddingTop(3).AlignCenter()
                .Text("متوافق مع متطلبات هيئة الزكاة والضريبة والجمارك")
                .FontSize(5).FontColor(Colors.Grey.Darken1);
            column.Item().AlignCenter()
                .Text("ZATCA Compliant")
                .FontSize(5).FontColor(Colors.Grey.Darken1);
        });
    }

    private static void ReceiptDivider(IContainer container)
    {
        container.AlignCenter().Text("--------------------------------").FontSize(6);
    }

    private static void ReceiptDoubleDivider(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text("================================").FontSize(6);
        });
    }

    #endregion
}
