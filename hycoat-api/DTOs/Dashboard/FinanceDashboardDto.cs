namespace HycoatApi.DTOs.Dashboard;

public class FinanceDashboardDto
{
    public decimal MonthlyInvoicedAmount { get; set; }
    public int InvoicesSentThisMonth { get; set; }
    public int UnpaidInvoices { get; set; }
    public decimal OutstandingAmount { get; set; }
    public List<ChartPointDto> MonthlyRevenue { get; set; } = [];
    public List<StatusCountDto> InvoicesByStatus { get; set; } = [];
}
