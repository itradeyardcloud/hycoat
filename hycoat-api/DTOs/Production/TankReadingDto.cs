namespace HycoatApi.DTOs.Production;

public class TankReadingDto
{
    public string TankName { get; set; } = string.Empty;
    public decimal? Concentration { get; set; }
    public decimal? Temperature { get; set; }
    public string? ChemicalAdded { get; set; }
    public decimal? ChemicalQty { get; set; }
}
