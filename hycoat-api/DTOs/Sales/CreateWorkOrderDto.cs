namespace HycoatApi.DTOs.Sales;

public class CreateWorkOrderDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? ProformaInvoiceId { get; set; }
    public string? ProjectName { get; set; }
    public int ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? Notes { get; set; }
}
