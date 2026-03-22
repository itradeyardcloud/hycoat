using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Dispatch;

public class InvoiceLineItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int SectionProfileId { get; set; }
    public string? SectionNumber { get; set; }
    public string? DCNumber { get; set; }
    public string? Color { get; set; }
    public string? MicronRange { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal AreaSFT { get; set; }
    public decimal RatePerSFT { get; set; }
    public decimal Amount { get; set; }

    // Navigation
    public Invoice Invoice { get; set; } = null!;
    public SectionProfile SectionProfile { get; set; } = null!;
}
