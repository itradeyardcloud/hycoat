using HycoatApi.DTOs.Quality;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HycoatApi.Services.Quality;

public class TestCertificatePdfService
{
    public byte[] Generate(TestCertificateDetailDto tc)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, tc));
                page.Content().Element(c => ComposeContent(c, tc));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, TestCertificateDetailDto tc)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("HYCOAT SYSTEMS PVT. LTD.")
                .Bold().FontSize(16);

            column.Item().AlignCenter().Text("TEST CERTIFICATE")
                .Bold().FontSize(14).FontColor(Colors.Grey.Darken2);

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("Certificate No: ").Bold();
                        t.Span(tc.CertificateNumber);
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Customer: ").Bold();
                        t.Span(tc.CustomerName);
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Project: ").Bold();
                        t.Span(tc.ProjectName ?? "-");
                    });
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("Date: ").Bold();
                        t.Span(tc.Date.ToString("dd-MMM-yyyy"));
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("WO: ").Bold();
                        t.Span(tc.WorkOrderNumber);
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("RAL: ").Bold();
                        t.Span(tc.ProductCode ?? "-");
                    });
                });
            });

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("Lot Qty: ").Bold();
                    t.Span(tc.LotQuantity.ToString());
                });
                row.RelativeItem().Text(t =>
                {
                    t.Span("Warranty: ").Bold();
                    t.Span(tc.Warranty ?? "-");
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private void ComposeContent(IContainer container, TestCertificateDetailDto tc)
    {
        container.PaddingTop(15).Column(column =>
        {
            column.Item().Text("TEST RESULTS").Bold().FontSize(12);

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Parameter").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Result").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                        .Text("Standard Ref").Bold();
                });

                // Rows
                AddTestRow(table, "Substrate", tc.SubstrateResult, "");
                AddTestRow(table, "Baking Temperature", tc.BakingTempResult, "");
                AddTestRow(table, "Baking Time", tc.BakingTimeResult, "");
                AddTestRow(table, "Color", tc.ColorResult, "");
                AddTestRow(table, "DFT", tc.DFTResult, "60-80 µm");
                AddTestRow(table, "MEK Resistance", tc.MEKResult, "AAMA 2604");
                AddTestRow(table, "Cross-Cut Adhesion", tc.CrossCutResult, "ISO 2409");
                AddTestRow(table, "Conical Mandrel Bend", tc.ConicalMandrelResult, "ASTM D522");
                AddTestRow(table, "Boiling Water", tc.BoilingWaterResult, "");
            });

            // Overall Status
            column.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("Overall Status: ").Bold().FontSize(12);
                    var statusColor = tc.OverallStatus == "Approved"
                        ? Colors.Green.Darken2
                        : Colors.Red.Darken2;
                    t.Span(tc.OverallStatus.ToUpper()).Bold().FontSize(12).FontColor(statusColor);
                });
            });

            // Signature area
            column.Item().PaddingTop(40).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(5).Text("QA Manager").FontSize(9);
                    col.Item().Text("Date: _______________").FontSize(9);
                });
                row.ConstantItem(50);
                row.RelativeItem().Column(col =>
                {
                    col.Item().PaddingTop(20).Text("Company Seal").FontSize(9).Italic()
                        .FontColor(Colors.Grey.Darken1);
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

    private static void AddTestRow(TableDescriptor table, string parameter, string? result, string standard)
    {
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
            .Text(parameter);
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
            .Text(result ?? "-");
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
            .Text(standard);
    }
}
