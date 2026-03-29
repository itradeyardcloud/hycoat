namespace HycoatApi.DTOs.Sales;

public class CalculateAreaRequestDto
{
    public List<AreaCalcLineDto> Lines { get; set; } = [];
}

public class AreaCalcLineDto
{
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
}

public class CalculateAreaResponseDto
{
    public List<AreaCalcResultDto> Lines { get; set; } = [];
    public decimal TotalAreaSFT { get; set; }
}

public class AreaCalcResultDto
{
    public int SectionProfileId { get; set; }
    public string SectionNumber { get; set; } = string.Empty;
    public decimal PerimeterMM { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal AreaSFT { get; set; }
}
