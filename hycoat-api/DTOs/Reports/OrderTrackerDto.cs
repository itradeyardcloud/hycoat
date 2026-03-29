namespace HycoatApi.DTOs.Reports;

public class OrderTrackerDto
{
    public int WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public int CompletionPercent { get; set; }
    public bool InquiryDone { get; set; }
    public bool QuotationDone { get; set; }
    public bool PIDone { get; set; }
    public bool MaterialReceived { get; set; }
    public bool IncomingInspectionDone { get; set; }
    public bool PWOCreated { get; set; }
    public bool PretreatmentDone { get; set; }
    public bool CoatingDone { get; set; }
    public bool FinalInspectionDone { get; set; }
    public bool Dispatched { get; set; }
    public bool Invoiced { get; set; }
    public int DaysInProcess { get; set; }
}
