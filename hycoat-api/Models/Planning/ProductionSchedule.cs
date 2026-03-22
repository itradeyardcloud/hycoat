using HycoatApi.Models.Common;
using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Planning;

public class ProductionSchedule : BaseEntity
{
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionWorkOrderId { get; set; }
    public int ProductionUnitId { get; set; }
    public int SortOrder { get; set; }
    public string Status { get; set; } = "Planned";
    public string? Notes { get; set; }

    // Navigation
    public ProductionWorkOrder ProductionWorkOrder { get; set; } = null!;
    public ProductionUnit ProductionUnit { get; set; } = null!;
}
