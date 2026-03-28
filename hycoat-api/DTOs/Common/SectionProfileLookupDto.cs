namespace HycoatApi.DTOs.Common;

public class SectionProfileLookupDto
{
    public int Id { get; set; }
    public string SectionNumber { get; set; } = string.Empty;
    public decimal PerimeterMM { get; set; }
}
