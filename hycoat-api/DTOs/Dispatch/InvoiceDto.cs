namespace HycoatApi.DTOs.Dispatch;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string WONumber { get; set; } = string.Empty;
    public decimal TotalSFT { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasPdf { get; set; }
}
