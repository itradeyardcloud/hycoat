namespace HycoatApi.DTOs.Sales;

public class WorkOrderDetailDto : WorkOrderDto
{
    public int CustomerId { get; set; }
    public int ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public int? ProformaInvoiceId { get; set; }
    public string? PINumber { get; set; }
    public string? Notes { get; set; }
    public int MaterialInwardCount { get; set; }
    public int ProductionWorkOrderCount { get; set; }
    public int DeliveryChallanCount { get; set; }
    public int InvoiceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
