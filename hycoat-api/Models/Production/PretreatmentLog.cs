using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Planning;

namespace HycoatApi.Models.Production;

public class PretreatmentLog : BaseEntity
{
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionWorkOrderId { get; set; }
    public int BasketNumber { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public decimal? EtchTimeMins { get; set; }
    public string? OperatorUserId { get; set; }
    public string? QASignOffUserId { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public ProductionWorkOrder ProductionWorkOrder { get; set; } = null!;
    public AppUser? OperatorUser { get; set; }
    public AppUser? QASignOffUser { get; set; }
    public ICollection<PretreatmentTankReading> TankReadings { get; set; } = new List<PretreatmentTankReading>();
}
