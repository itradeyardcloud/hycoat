namespace HycoatApi.DTOs.Sales;

public class InquiryStatsDto
{
    public int New { get; set; }
    public int QuotationSent { get; set; }
    public int BOMReceived { get; set; }
    public int PISent { get; set; }
    public int Confirmed { get; set; }
    public int Lost { get; set; }
    public int Closed { get; set; }
    public int Total { get; set; }
}
