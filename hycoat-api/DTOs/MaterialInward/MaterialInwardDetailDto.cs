using HycoatApi.DTOs.Common;

namespace HycoatApi.DTOs.MaterialInward;

public class MaterialInwardDetailDto : MaterialInwardDto
{
    public DateTime? CustomerDCDate { get; set; }
    public string? UnloadingLocation { get; set; }
    public int? ProcessTypeId { get; set; }
    public string? ProcessTypeName { get; set; }
    public int? PowderColorId { get; set; }
    public string? PowderColorName { get; set; }
    public string? ReceivedByName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<MaterialInwardLineDto> Lines { get; set; } = new();
    public List<FileAttachmentDto> Photos { get; set; } = new();
}

public class MaterialInwardLineDto
{
    public int Id { get; set; }
    public int SectionProfileId { get; set; }
    public string SectionNumber { get; set; } = string.Empty;
    public decimal LengthMM { get; set; }
    public int QtyAsPerDC { get; set; }
    public int QtyReceived { get; set; }
    public decimal? WeightKg { get; set; }
    public int Discrepancy { get; set; }
    public string? Remarks { get; set; }
}
