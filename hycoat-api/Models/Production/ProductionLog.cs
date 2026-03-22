using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Planning;

namespace HycoatApi.Models.Production;

public class ProductionLog : BaseEntity
{
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionWorkOrderId { get; set; }
    public decimal? ConveyorSpeedMtrPerMin { get; set; }
    public decimal? OvenTemperature { get; set; }
    public string? PowderBatchNo { get; set; }
    public string? SupervisorUserId { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public ProductionWorkOrder ProductionWorkOrder { get; set; } = null!;
    public AppUser? SupervisorUser { get; set; }
    public ICollection<ProductionPhoto> Photos { get; set; } = new List<ProductionPhoto>();
}
