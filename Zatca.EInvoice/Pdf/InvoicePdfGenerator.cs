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

            // Billing reference for credit/debit notes
            if (_data.BillingReferences?.Any() == true)
            {
                column.Item().PaddingTop(5).Background(Colors.Yellow.Lighten4).Padding(5).Column(col =>
                {
                    foreach (var reference in _data.BillingReferences)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("مرجع الفاتورة الأصلية / Original Invoice Reference: ").Bold();
                            text.Span(reference.InvoiceDocumentReference?.Id ?? "");
                        });
                    }
                });
            }
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
}
