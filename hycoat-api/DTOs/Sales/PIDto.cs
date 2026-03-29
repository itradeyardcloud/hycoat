namespace HycoatApi.DTOs.Sales;

public class PIDto
{
    public int Id { get; set; }
    public string PINumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? QuotationNumber { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}
