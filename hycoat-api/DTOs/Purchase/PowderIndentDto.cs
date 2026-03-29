namespace HycoatApi.DTOs.Purchase;

public class PowderIndentDto
{
    public int Id { get; set; }
    public string IndentNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? PWONumber { get; set; }
    public string? RequestedByName { get; set; }
    public int LineCount { get; set; }
    public decimal TotalQtyKg { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PowderIndentDetailDto : PowderIndentDto
{
    public int? ProductionWorkOrderId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PowderIndentLineDto> Lines { get; set; } = [];
}

public class PowderIndentLineDto
{
    public int Id { get; set; }
    public int PowderColorId { get; set; }
    public string PowderCode { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string? RALCode { get; set; }
    public decimal RequiredQtyKg { get; set; }
}
