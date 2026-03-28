using HycoatApi.Models.Dispatch;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HycoatApi.Services.Dispatch;

public static class InvoicePdfService
{
    public static byte[] Generate(Invoice invoice)
    {
        var document = Document.Create(container =>
        {
            // Page 1: Invoice
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeInvoiceHeader(c, invoice));
                page.Content().Element(c => ComposeInvoiceContent(c, invoice));
                page.Footer().Element(ComposeFooter);
            });

            // Page 2: Annexure
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeAnnexureHeader(c, invoice));
                page.Content().Element(c => ComposeAnnexureContent(c, invoice));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeInvoiceHeader(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("HYCOAT SYSTEMS PVT. LTD.")
                .Bold().FontSize(16);

            column.Item().AlignCenter().Text(t =>
            {
                if (!string.IsNullOrEmpty(invoice.OurGSTIN))
                {
                    t.Span($"GSTIN: {invoice.OurGSTIN}");
                    if (!string.IsNullOrEmpty(invoice.HSNSACCode))
                        t.Span($" | HSN: {invoice.HSNSACCode}");
                }
            });

            column.Item().AlignCenter().Text("TAX INVOICE")
                .Bold().FontSize(14).FontColor(Colors.Grey.Darken2);

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("Invoice No: ").Bold();
                        t.Span(invoice.InvoiceNumber);
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Customer: ").Bold();
                        t.Span(invoice.CustomerName ?? invoice.Customer?.Name ?? "");
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("Address: ").Bold();
                        t.Span(invoice.CustomerAddress ?? "");
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("GSTIN: ").Bold();
                        t.Span(invoice.CustomerGSTIN ?? "");
                    });
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("Date: ").Bold();
                        t.Span(invoice.Date.ToString("dd-MMM-yyyy"));
                    });
                    col.Item().Text(t =>
                    {
                        t.Span("WO Ref: ").Bold();
                        t.Span(invoice.WorkOrder?.WONumber ?? "");
                    });
                    if (invoice.DeliveryChallan != null)
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("DC Ref: ").Bold();
                            t.Span(invoice.DeliveryChallan.DCNumber);
                        });
                    }
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private static void ComposeInvoiceContent(IContainer container, Invoice invoice)
    {
        container.PaddingTop(15).Column(column =>
        {
            // Line items table
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);   // Sr
                    columns.RelativeColumn(2);     // Section
                    columns.RelativeColumn(1.5f);  // Color
                    columns.RelativeColumn(1);     // Qty
                    columns.RelativeColumn(1.5f);  // Area(SFT)
                    columns.RelativeColumn(1.2f);  // Rate
                    columns.RelativeColumn(1.5f);  // Amount
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Sr").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Section").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Color").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Qty").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Area(SFT)").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Rate").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Amount").Bold();
                });

                var sr = 1;
                foreach (var line in invoice.LineItems)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(sr.ToString());
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(line.SectionNumber ?? "");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(line.Color ?? "");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(line.Quantity.ToString());
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(line.AreaSFT.ToString("N2"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(line.RatePerSFT.ToString("N2"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(line.Amount.ToString("N2"));
                    sr++;
                }
            });

            // Totals section
            column.Item().PaddingTop(10).AlignRight().Width(250).Column(totals =>
            {
                AddTotalRow(totals, "Sub Total:", invoice.SubTotal);
                if (invoice.PackingCharges > 0)
                    AddTotalRow(totals, "Packing Charges:", invoice.PackingCharges);
                if (invoice.TransportCharges > 0)
                    AddTotalRow(totals, "Transport Charges:", invoice.TransportCharges);
                AddTotalRow(totals, "Taxable Amount:", invoice.TaxableAmount);

                if (invoice.IsInterState)
                {
                    AddTotalRow(totals, $"IGST @ {invoice.IGSTRate}%:", invoice.IGSTAmount);
                }
                else
                {
                    AddTotalRow(totals, $"CGST @ {invoice.CGSTRate}%:", invoice.CGSTAmount);
                    AddTotalRow(totals, $"SGST @ {invoice.SGSTRate}%:", invoice.SGSTAmount);
                }

                if (invoice.RoundOff != 0)
                    AddTotalRow(totals, "Round Off:", invoice.RoundOff);

                totals.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                totals.Item().PaddingTop(3).Row(row =>
                {
                    row.RelativeItem().Text("GRAND TOTAL:").Bold().FontSize(11);
                    row.RelativeItem().AlignRight().Text($"₹ {invoice.GrandTotal:N2}").Bold().FontSize(11);
                });
            });

            // Amount in words
            if (!string.IsNullOrEmpty(invoice.AmountInWords))
            {
                column.Item().PaddingTop(10).Text(t =>
                {
                    t.Span("Amount in Words: ").Bold();
                    t.Span(invoice.AmountInWords);
                });
            }

            // Bank details
            column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Medium);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    if (!string.IsNullOrEmpty(invoice.BankName))
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("Bank: ").Bold();
                            t.Span(invoice.BankName);
                        });
                    }
                    if (!string.IsNullOrEmpty(invoice.BankAccountNo))
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("A/C: ").Bold();
                            t.Span(invoice.BankAccountNo);
                        });
                    }
                    if (!string.IsNullOrEmpty(invoice.BankIFSC))
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("IFSC: ").Bold();
                            t.Span(invoice.BankIFSC);
                        });
                    }
                });
                row.RelativeItem().Column(col =>
                {
                    if (!string.IsNullOrEmpty(invoice.PaymentTerms))
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("Terms: ").Bold();
                            t.Span(invoice.PaymentTerms);
                        });
                    }
                });
            });

            // Signature
            column.Item().PaddingTop(40).AlignRight().Column(col =>
            {
                col.Item().Text("For HYCOAT SYSTEMS PVT. LTD.").Bold();
                col.Item().PaddingTop(25).Text("Authorised Signatory");
            });
        });
    }

    private static void ComposeAnnexureHeader(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("ANNEXURE")
                .Bold().FontSize(14);

            column.Item().AlignCenter().Text($"Invoice Ref: {invoice.InvoiceNumber}")
                .FontSize(11);

            column.Item().PaddingTop(10).AlignCenter().Text("Area Calculation Breakdown")
                .FontSize(10).FontColor(Colors.Grey.Darken2);

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private static void ComposeAnnexureContent(IContainer container, Invoice invoice)
    {
        container.PaddingTop(15).Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);   // Sr
                    columns.RelativeColumn(2);     // Section
                    columns.RelativeColumn(2);     // Perimeter(mm)
                    columns.RelativeColumn(2);     // Length(mm)
                    columns.RelativeColumn(1);     // Qty
                    columns.RelativeColumn(1.5f);  // SFT
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Sr").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Section").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Perimeter (mm)").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Length (mm)").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Qty").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("SFT").Bold();
                });

                var sr = 1;
                decimal totalSFT = 0;
                foreach (var line in invoice.LineItems)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(sr.ToString());
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(line.SectionNumber ?? "");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.PerimeterMM.ToString("N0"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.LengthMM.ToString("N0"));
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.Quantity.ToString());
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(line.AreaSFT.ToString("N2"));

                    totalSFT += line.AreaSFT;
                    sr++;
                }

                // Total row
                table.Cell().ColumnSpan(5).Padding(5).AlignRight().Text("Total Area:").Bold();
                table.Cell().Padding(5).AlignRight().Text($"{totalSFT:N2} SFT").Bold();
            });

            // Formula reference
            column.Item().PaddingTop(20).Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
            {
                col.Item().Text("Formula: (Perimeter × Length × Qty) ÷ 92,903.04 = SFT")
                    .Bold().FontSize(10);
                col.Item().PaddingTop(3).Text("1 Square Foot = 92,903.04 Square Millimeters")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private static void AddTotalRow(ColumnDescriptor column, string label, decimal amount)
    {
        column.Item().PaddingVertical(2).Row(row =>
        {
            row.RelativeItem().Text(label);
            row.RelativeItem().AlignRight().Text($"₹ {amount:N2}");
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
