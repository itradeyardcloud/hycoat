using HycoatApi.Models.Common;

namespace HycoatApi.Models.Masters;

public class ProcessType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal? DefaultRatePerSFT { get; set; }
    public string? Description { get; set; }
}
