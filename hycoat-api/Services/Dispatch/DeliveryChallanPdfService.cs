using HycoatApi.Models.Dispatch;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HycoatApi.Services.Dispatch;

public static class DeliveryChallanPdfService
{
    public static byte[] Generate(DeliveryChallan dc)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, dc));
                page.Content().Element(c => ComposeContent(c, dc));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, DeliveryChallan dc)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("HYCOAT SYSTEMS PVT. LTD.")
                .Bold().FontSize(16);

            column.Item().AlignCenter().Text("DELIVERY CHALLAN")
                .Bold().FontSize(14).FontColor(Colors.Grey.Darken2);

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("DC No: ").Bold();
                        t.Span(dc.DCNumber);
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Customer: ").Bold();
                        t.Span(dc.Customer?.Name ?? "");
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Address: ").Bold();
                        t.Span(dc.CustomerAddress ?? "");
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("GSTIN: ").Bold();
                        t.Span(dc.CustomerGSTIN ?? "");
                    });
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("Date: ").Bold();
                        t.Span(dc.Date.ToString("dd-MMM-yyyy"));
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("WO Ref: ").Bold();
                        t.Span(dc.WorkOrder?.WONumber ?? "");
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Vehicle: ").Bold();
                        t.Span(dc.VehicleNumber ?? "");
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Driver: ").Bold();
                        t.Span(dc.DriverName ?? "");
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("LR No: ").Bold();
                        t.Span(dc.LRNumber ?? "");
                    });
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private static void ComposeContent(IContainer container, DeliveryChallan dc)
    {
        container.PaddingTop(15).Column(column =>
        {
            // Line items table
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);   // Sr
                    columns.RelativeColumn(3);     // Section
                    columns.RelativeColumn(2);     // Length(mm)
                    columns.RelativeColumn(1.5f);  // Qty
                    columns.RelativeColumn(3);     // Remarks
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Sr").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Section").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Length (mm)").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Qty").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Remarks").Bold();
                });

                var sr = 1;
                var totalQty = 0;
                foreach (var line in dc.LineItems)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(sr.ToString());
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(line.SectionProfile?.SectionNumber ?? "");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(line.LengthMM.ToString("N0"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .AlignRight().Text(line.Quantity.ToString());
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(line.Remarks ?? "");

                    totalQty += line.Quantity;
                    sr++;
                }

                // Total row
                table.Cell().ColumnSpan(3).Padding(5).AlignRight().Text("Total Quantity:").Bold();
                table.Cell().Padding(5).AlignRight().Text(totalQty.ToString()).Bold();
                table.Cell().Padding(5).Text("");
            });

            // Material value & bundles
            column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            if (dc.MaterialValueApprox.HasValue)
            {
                column.Item().PaddingTop(5).Text(t =>
                {
                    t.Span("Material Value (Approx): ").Bold();
                    t.Span($"₹ {dc.MaterialValueApprox.Value:N0}");
                });
            }

            // Signature block
            column.Item().PaddingTop(40).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Prepared By: _______________");
                    col.Item().PaddingTop(20).Text("Received By: _______________");
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Checked By: _______________");
                    col.Item().PaddingTop(20).Text("Date: _______________");
                });
            });
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(t =>
        {
            t.Span("Page ");
            t.CurrentPageNumber();
            t.Span(" of ");
            t.TotalPages();
        });
    }
}
