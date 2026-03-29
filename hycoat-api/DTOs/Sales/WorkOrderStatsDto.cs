namespace HycoatApi.DTOs.Sales;

public class WorkOrderStatsDto
{
    public int Created { get; set; }
    public int MaterialAwaited { get; set; }
    public int MaterialReceived { get; set; }
    public int InProduction { get; set; }
    public int QAComplete { get; set; }
    public int Dispatched { get; set; }
    public int Invoiced { get; set; }
    public int Closed { get; set; }
    public int Total { get; set; }
}
