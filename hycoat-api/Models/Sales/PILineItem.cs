using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Sales;

public class PILineItem
{
    public int Id { get; set; }
    public int ProformaInvoiceId { get; set; }
    public int SectionProfileId { get; set; }
    public string? SectionNumber { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal AreaSqMtr { get; set; }
    public decimal AreaSFT { get; set; }
    public decimal RatePerSFT { get; set; }
    public decimal Amount { get; set; }

    // Navigation
    public ProformaInvoice ProformaInvoice { get; set; } = null!;
    public SectionProfile SectionProfile { get; set; } = null!;
}
