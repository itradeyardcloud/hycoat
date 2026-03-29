using HycoatApi.DTOs.Sales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HycoatApi.Services.Sales;

public class QuotationPdfService
{
    public byte[] Generate(QuotationDetailDto quotation)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, quotation));
                page.Content().Element(c => ComposeContent(c, quotation));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, QuotationDetailDto q)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("HYCOAT SYSTEMS PVT. LTD.")
                .Bold().FontSize(16);
            column.Item().AlignCenter().Text("Powder Coating & Surface Treatment")
                .FontSize(9).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(5).AlignCenter().Text("QUOTATION")
                .Bold().FontSize(14).FontColor(Colors.Blue.Darken2);

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text(t =>
                    {
                        t.Span("Quotation No: ").Bold();
                        t.Span(q.QuotationNumber);
                    });
                    left.Item().Text(t =>
                    {
                        t.Span("Date: ").Bold();
                        t.Span(q.Date.ToString("dd-MMM-yyyy"));
                    });
                    if (!string.IsNullOrEmpty(q.InquiryNumber))
                    {
                        left.Item().Text(t =>
                        {
                            t.Span("Ref Inquiry: ").Bold();
                            t.Span(q.InquiryNumber);
                        });
                    }
                });

                row.RelativeItem().Column(right =>
                {
                    right.Item().Text(t =>
                    {
                        t.Span("To: ").Bold();
                        t.Span(q.CustomerName);
                    });
                    right.Item().Text(t =>
                    {
                        t.Span("Validity: ").Bold();
                        t.Span($"{q.ValidityDays} days");
                    });
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private void ComposeContent(IContainer container, QuotationDetailDto q)
    {
        container.PaddingTop(15).Column(column =>
        {
            column.Item().Text("Dear Sir/Madam,").FontSize(10);
            column.Item().PaddingTop(5).Text("We are pleased to quote the following rates for your kind consideration:")
                .FontSize(10);

            // Line items table
            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);     // Sr
                    columns.RelativeColumn(4);       // Process
                    columns.RelativeColumn(3);       // Description
                    columns.RelativeColumn(2);       // Rate/SFT
                    columns.RelativeColumn(2);       // Warranty
                    columns.RelativeColumn(2);       // Micron
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Sr").FontColor(Colors.White).SemiBold().FontSize(9);
                    header.Cell().Element(HeaderCell).Text("Process Type").FontColor(Colors.White).SemiBold().FontSize(9);
                    header.Cell().Element(HeaderCell).Text("Description").FontColor(Colors.White).SemiBold().FontSize(9);
                    header.Cell().Element(HeaderCell).Text("Rate/SFT (₹)").FontColor(Colors.White).SemiBold().FontSize(9);
                    header.Cell().Element(HeaderCell).Text("Warranty").FontColor(Colors.White).SemiBold().FontSize(9);
                    header.Cell().Element(HeaderCell).Text("Micron Range").FontColor(Colors.White).SemiBold().FontSize(9);
                });

                // Rows
                for (var i = 0; i < q.LineItems.Count; i++)
                {
                    var li = q.LineItems[i];
                    DataCell(table, (i + 1).ToString());
                    DataCell(table, li.ProcessTypeName);
                    DataCell(table, li.Description ?? "-");
                    DataCell(table, li.RatePerSFT.ToString("N2"));
                    DataCell(table, li.WarrantyYears.HasValue ? $"{li.WarrantyYears} yrs" : "-");
                    DataCell(table, li.MicronRange ?? "-");
                }
            });

            // Terms & Conditions
            column.Item().PaddingTop(25).Text("Terms & Conditions:").Bold().FontSize(11);
            column.Item().PaddingTop(5).Column(tc =>
            {
                tc.Item().Text("1. GST @ 18% extra as applicable.");
                tc.Item().Text("2. Packing charges extra as applicable.");
                tc.Item().Text("3. Transport charges extra as applicable.");
                tc.Item().Text($"4. Quotation valid for {q.ValidityDays} days from the date of issue.");
                tc.Item().Text("5. Payment terms: 30 days from invoice date.");
                tc.Item().Text("6. Delivery as per mutually agreed schedule.");
            });

            if (!string.IsNullOrEmpty(q.Notes))
            {
                column.Item().PaddingTop(15).Text(t =>
                {
                    t.Span("Notes: ").Bold();
                    t.Span(q.Notes);
                });
            }

            // Signature
            column.Item().PaddingTop(40).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Thanking you,");
                    col.Item().PaddingTop(5).Text("For HYCOAT SYSTEMS PVT. LTD.").Bold();
                    col.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(5).Text("Authorized Signatory").FontSize(9);
                });
                row.ConstantItem(100);
                row.RelativeItem();
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

    private static IContainer HeaderCell(IContainer container)
    {
        return container.Background(Colors.Blue.Darken2).Padding(6);
    }

    private static void DataCell(TableDescriptor table, string text)
    {
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
            .Text(text).FontSize(9);
    }
}
