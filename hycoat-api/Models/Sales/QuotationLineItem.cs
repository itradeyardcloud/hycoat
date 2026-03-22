using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Sales;

public class QuotationLineItem
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    public int ProcessTypeId { get; set; }
    public string? Description { get; set; }
    public decimal RatePerSFT { get; set; }
    public int? WarrantyYears { get; set; }
    public string? MicronRange { get; set; }

    // Navigation
    public Quotation Quotation { get; set; } = null!;
    public ProcessType ProcessType { get; set; } = null!;
}
