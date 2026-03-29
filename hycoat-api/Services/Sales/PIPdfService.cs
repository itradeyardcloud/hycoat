using HycoatApi.DTOs.Sales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HycoatApi.Services.Sales;

public class PIPdfService
{
    public byte[] Generate(PIDetailDto pi)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComposeHeader(c, pi));
                page.Content().Element(c => ComposeContent(c, pi));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, PIDetailDto pi)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("HYCOAT SYSTEMS PVT. LTD.")
                .Bold().FontSize(16);
            column.Item().AlignCenter().Text("Powder Coating & Surface Treatment")
                .FontSize(8).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(5).AlignCenter().Text("PROFORMA INVOICE")
                .Bold().FontSize(14).FontColor(Colors.Blue.Darken2);

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            column.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text(t =>
                    {
                        t.Span("PI No: ").Bold();
                        t.Span(pi.PINumber);
                    });
                    left.Item().Text(t =>
                    {
                        t.Span("Date: ").Bold();
                        t.Span(pi.Date.ToString("dd-MMM-yyyy"));
                    });
                    if (!string.IsNullOrEmpty(pi.QuotationNumber))
                    {
                        left.Item().Text(t =>
                        {
                            t.Span("Ref: ").Bold();
                            t.Span(pi.QuotationNumber);
                        });
                    }
                });

                row.RelativeItem().Column(right =>
                {
                    right.Item().Text("To:").Bold();
                    right.Item().Text(pi.CustomerName).Bold();
                    if (!string.IsNullOrEmpty(pi.CustomerAddress))
                        right.Item().Text(pi.CustomerAddress);
                    if (!string.IsNullOrEmpty(pi.CustomerGSTIN))
                    {
                        right.Item().Text(t =>
                        {
                            t.Span("GSTIN: ").Bold();
                            t.Span(pi.CustomerGSTIN);
                        });
                    }
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private void ComposeContent(IContainer container, PIDetailDto pi)
    {
        container.PaddingTop(10).Column(column =>
        {
            // LINE ITEMS TABLE
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);      // Sr
                    columns.RelativeColumn(2.5f);    // Section
                    columns.RelativeColumn(1.5f);    // Length
                    columns.ConstantColumn(35);      // Qty
                    columns.RelativeColumn(1.5f);    // Perimeter
                    columns.RelativeColumn(2);       // Area SFT
                    columns.RelativeColumn(1.5f);    // Rate
                    columns.RelativeColumn(2);       // Amount
                });

                table.Header(header =>
                {
                    header.Cell().Element(HCell).Text("Sr").FontColor(Colors.White).SemiBold().FontSize(8);
                    header.Cell().Element(HCell).Text("Section").FontColor(Colors.White).SemiBold().FontSize(8);
                    header.Cell().Element(HCell).Text("Len(mm)").FontColor(Colors.White).SemiBold().FontSize(8);
                    header.Cell().Element(HCell).Text("Qty").FontColor(Colors.White).SemiBold().FontSize(8);
                    header.Cell().Element(HCell).Text("Peri(mm)").FontColor(Colors.White).SemiBold().FontSize(8);
                    header.Cell().Element(HCell).Text("Area(SFT)").FontColor(Colors.White).SemiBold().FontSize(8);
                    header.Cell().Element(HCell).Text("Rate/SFT").FontColor(Colors.White).SemiBold().FontSize(8);
                    header.Cell().Element(HCell).Text("Amount (₹)").FontColor(Colors.White).SemiBold().FontSize(8);
                });

                for (var i = 0; i < pi.LineItems.Count; i++)
                {
                    var li = pi.LineItems[i];
                    DCell(table, (i + 1).ToString());
                    DCell(table, li.SectionNumber);
                    DCell(table, li.LengthMM.ToString("N0"));
                    DCell(table, li.Quantity.ToString());
                    DCell(table, li.PerimeterMM.ToString("N2"));
                    DCell(table, li.AreaSFT.ToString("N2"));
                    DCell(table, li.RatePerSFT.ToString("N2"));
                    DCell(table, li.Amount.ToString("N2"));
                }
            });

            // TOTALS
            column.Item().PaddingTop(10).AlignRight().Column(totals =>
            {
                TotalRow(totals, "Subtotal:", pi.SubTotal);
                if (pi.PackingCharges > 0)
                    TotalRow(totals, "Packing Charges:", pi.PackingCharges);
                if (pi.TransportCharges > 0)
                    TotalRow(totals, "Transport Charges:", pi.TransportCharges);
                TotalRow(totals, "Taxable Amount:", pi.TaxableAmount);

                if (pi.IsInterState)
                {
                    TotalRow(totals, $"IGST ({pi.IGSTRate}%):", pi.IGSTAmount);
                }
                else
                {
                    TotalRow(totals, $"CGST ({pi.CGSTRate}%):", pi.CGSTAmount);
                    TotalRow(totals, $"SGST ({pi.SGSTRate}%):", pi.SGSTAmount);
                }

                totals.Item().PaddingTop(3).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                totals.Item().PaddingTop(3).Row(row =>
                {
                    row.ConstantItem(120).Text("Grand Total:").Bold().FontSize(11);
                    row.ConstantItem(100).AlignRight().Text($"₹{pi.GrandTotal:N2}").Bold().FontSize(11);
                });
            });

            // ANNEXURE — Area Calculation Breakdown
            column.Item().PaddingTop(20).Text("ANNEXURE — Area Calculation").Bold().FontSize(11);
            column.Item().PaddingTop(5).Column(annexure =>
            {
                foreach (var li in pi.LineItems)
                {
                    annexure.Item().Text(t =>
                    {
                        t.Span($"Section {li.SectionNumber}: ").Bold();
                        t.Span($"Perimeter={li.PerimeterMM:N2}mm × Length={li.LengthMM:N0}mm × Qty={li.Quantity} / 92903.04 = ");
                        t.Span($"{li.AreaSFT:N2} SFT").Bold();
                    });
                }
            });

            // BANK DETAILS & T&C
            column.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Column(bank =>
                {
                    bank.Item().Text("Bank Details:").Bold();
                    bank.Item().Text("Bank: [Bank Name]");
                    bank.Item().Text("A/C No: [Account Number]");
                    bank.Item().Text("IFSC: [IFSC Code]");
                    bank.Item().Text("Branch: [Branch Name]");
                });

                row.RelativeItem().Column(tc =>
                {
                    tc.Item().Text("Terms & Conditions:").Bold();
                    tc.Item().Text("1. Validity: 15 days");
                    tc.Item().Text("2. Payment: 30 days from invoice");
                    tc.Item().Text("3. Delivery as per schedule");
                    tc.Item().Text("4. Subject to Pune jurisdiction");
                });
            });

            if (!string.IsNullOrEmpty(pi.Notes))
            {
                column.Item().PaddingTop(10).Text(t =>
                {
                    t.Span("Notes: ").Bold();
                    t.Span(pi.Notes);
                });
            }

            // SIGNATURE
            column.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("For HYCOAT SYSTEMS PVT. LTD.").Bold();
                    col.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(5).Text("Authorized Signatory").FontSize(8);
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

    private static IContainer HCell(IContainer container)
    {
        return container.Background(Colors.Blue.Darken2).Padding(4);
    }

    private static void DCell(TableDescriptor table, string text)
    {
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
            .Text(text).FontSize(8);
    }

    private static void TotalRow(ColumnDescriptor column, string label, decimal value)
    {
        column.Item().Row(row =>
        {
            row.ConstantItem(120).Text(label).FontSize(9);
            row.ConstantItem(100).AlignRight().Text($"₹{value:N2}").FontSize(9);
        });
    }
}
