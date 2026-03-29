using HycoatApi.DTOs.Purchase;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HycoatApi.Services.Purchase;

public class PurchaseOrderPdfService
{
    public byte[] Generate(PurchaseOrderDetailDto po)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, po));
                page.Content().Element(c => ComposeContent(c, po));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, PurchaseOrderDetailDto po)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("HYCOAT SYSTEMS PVT. LTD.")
                .Bold().FontSize(16);

            column.Item().AlignCenter().Text("PURCHASE ORDER")
                .Bold().FontSize(14).FontColor(Colors.Grey.Darken2);

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("PO Number: ").Bold();
                        t.Span(po.PONumber);
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Vendor: ").Bold();
                        t.Span(po.VendorName);
                    });
                    if (po.IndentNumber != null)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("Indent Ref: ").Bold();
                            t.Span(po.IndentNumber);
                        });
                    }
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Text(t =>
                    {
                        t.Span("Date: ").Bold();
                        t.Span(po.Date.ToString("dd-MMM-yyyy"));
                    });
                    col.Item().AlignRight().Text(t =>
                    {
                        t.Span("Status: ").Bold();
                        t.Span(po.Status);
                    });
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private void ComposeContent(IContainer container, PurchaseOrderDetailDto po)
    {
        container.PaddingTop(15).Column(column =>
        {
            column.Item().Text("ORDER ITEMS").Bold().FontSize(12);

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);       // #
                    columns.RelativeColumn(3);         // Powder / Color
                    columns.RelativeColumn(1.5f);      // Qty (kg)
                    columns.RelativeColumn(1.5f);      // Rate/kg
                    columns.RelativeColumn(2);         // Amount
                    columns.RelativeColumn(2);         // Required By
                });

                // Header
                table.Header(header =>
                {
                    var headerStyle = TextStyle.Default.Bold().FontColor(Colors.White);

                    header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                        .Text("#").Style(headerStyle);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                        .Text("Powder / Color").Style(headerStyle);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                        .AlignRight().Text("Qty (kg)").Style(headerStyle);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                        .AlignRight().Text("Rate/kg").Style(headerStyle);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                        .AlignRight().Text("Amount (₹)").Style(headerStyle);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                        .Text("Required By").Style(headerStyle);
                });

                // Data rows
                for (var i = 0; i < po.Lines.Count; i++)
                {
                    var line = po.Lines[i];
                    var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).Text((i + 1).ToString());

                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).Text($"{line.PowderCode} - {line.ColorName}" +
                            (line.RALCode != null ? $" ({line.RALCode})" : ""));

                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).AlignRight().Text(line.QtyKg.ToString("N2"));

                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).AlignRight().Text(line.RatePerKg.ToString("N2"));

                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).AlignRight().Text(line.Amount.ToString("N2"));

                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .Padding(5).Text(line.RequiredByDate?.ToString("dd-MMM-yyyy") ?? "-");
                }
            });

            // Total
            column.Item().PaddingTop(10).AlignRight().Text(t =>
            {
                t.Span("Total Amount: ₹ ").Bold().FontSize(12);
                t.Span(po.TotalAmount.ToString("N2")).Bold().FontSize(12);
            });

            // Notes
            if (!string.IsNullOrWhiteSpace(po.Notes))
            {
                column.Item().PaddingTop(20).Text(t =>
                {
                    t.Span("Notes: ").Bold();
                    t.Span(po.Notes);
                });
            }

            // Signature area
            column.Item().PaddingTop(50).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(5).Text("Authorized Signatory").FontSize(9);
                });
                row.ConstantItem(100);
                row.RelativeItem().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(5).Text("Vendor Acknowledgment").FontSize(9);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(t =>
        {
            t.Span("This is a computer-generated document. ").FontSize(8).FontColor(Colors.Grey.Darken1);
            t.CurrentPageNumber().FontSize(8);
            t.Span(" / ").FontSize(8);
            t.TotalPages().FontSize(8);
        });
    }
}
