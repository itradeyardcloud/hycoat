using HycoatApi.Models.Common;
using HycoatApi.Models.Masters;
using HycoatApi.Models.Sales;

namespace HycoatApi.Models.Planning;

public class ProductionWorkOrder : BaseEntity
{
    public string PWONumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int WorkOrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public int ProductionUnitId { get; set; }
    public string? PowderCode { get; set; }
    public string? ColorName { get; set; }
    public decimal? PreTreatmentTimeHrs { get; set; }
    public decimal? PostTreatmentTimeHrs { get; set; }
    public decimal? TotalTimeHrs { get; set; }
    public string? ShiftAllocation { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? PackingType { get; set; }
    public string? SpecialInstructions { get; set; }
    public string Status { get; set; } = "Created";

    // Navigation
    public WorkOrder WorkOrder { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ProcessType ProcessType { get; set; } = null!;
    public PowderColor? PowderColor { get; set; }
    public ProductionUnit ProductionUnit { get; set; } = null!;
    public ICollection<PWOLineItem> LineItems { get; set; } = new List<PWOLineItem>();
    public ICollection<ProductionSchedule> Schedules { get; set; } = new List<ProductionSchedule>();
    public ICollection<Production.PretreatmentLog> PretreatmentLogs { get; set; } = new List<Production.PretreatmentLog>();
    public ICollection<Production.ProductionLog> ProductionLogs { get; set; } = new List<Production.ProductionLog>();
    public ICollection<Quality.InProcessInspection> InProcessInspections { get; set; } = new List<Quality.InProcessInspection>();
    public ICollection<Quality.PanelTest> PanelTests { get; set; } = new List<Quality.PanelTest>();
    public ICollection<Quality.FinalInspection> FinalInspections { get; set; } = new List<Quality.FinalInspection>();
}
