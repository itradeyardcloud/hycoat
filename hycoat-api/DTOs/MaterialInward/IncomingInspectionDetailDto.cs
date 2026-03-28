using HycoatApi.DTOs.Common;

namespace HycoatApi.DTOs.MaterialInward;

public class IncomingInspectionDetailDto : IncomingInspectionDto
{
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<IncomingInspectionLineDetailDto> Lines { get; set; } = new();
    public List<FileAttachmentDto> Photos { get; set; } = new();
}

public class IncomingInspectionLineDetailDto
{
    public int Id { get; set; }
    public int MaterialInwardLineId { get; set; }
    public string SectionNumber { get; set; } = string.Empty;
    public int QtyReceived { get; set; }
    public bool? WatermarkOk { get; set; }
    public bool? ScratchOk { get; set; }
    public bool? DentOk { get; set; }
    public bool? DimensionalCheckOk { get; set; }
    public bool BuffingRequired { get; set; }
    public decimal? BuffingCharge { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
