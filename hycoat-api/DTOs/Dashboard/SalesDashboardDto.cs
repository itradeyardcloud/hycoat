namespace HycoatApi.DTOs.Dashboard;

public class SalesDashboardDto
{
    public int OpenInquiries { get; set; }
    public int QuotationsSentThisMonth { get; set; }
    public int PIsAwaitingApproval { get; set; }
    public int ActiveWorkOrders { get; set; }
    public decimal QuotationToWOConversionRate { get; set; }
    public List<InquiryAgingDto> InquiryAging { get; set; } = [];
    public List<ChartPointDto> MonthlyQuotations { get; set; } = [];
}
