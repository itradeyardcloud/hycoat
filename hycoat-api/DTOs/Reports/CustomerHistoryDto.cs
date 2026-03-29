namespace HycoatApi.DTOs.Reports;

public class CustomerHistoryDto
{
    public string CustomerName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalAreaSFT { get; set; }
    public decimal TotalInvoicedAmount { get; set; }
    public List<CustomerOrderDto> Orders { get; set; } = [];
}

public class CustomerOrderDto
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal AreaSFT { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal? InvoiceAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
